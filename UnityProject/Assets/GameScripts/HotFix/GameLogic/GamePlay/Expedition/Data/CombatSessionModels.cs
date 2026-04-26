using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig.Gameplay.Combat;
using ExpeditionTable = GameConfig.Gameplay.Expedition;
using Luban.SimpleJSON;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class CombatSessionRequest
    {
        public string CombatSessionInstId;
        public string NodeConfigId;
        public string CombatEncounterConfigId;
        public string BattlefieldConfigId;
        public string Title;
        public List<MarblePersistentData?> LstAlliedMarble = new ();
        public List<MarbleSpawnConfig> LstEnemyMarble = new ();
    }

    [Serializable]
    public sealed class CombatSessionResult
    {
        public bool IsVictory;
        public string Summary;
        public List<MarblePersistentData?> LstMarbleResult = new ();
    }

    public class ExpeditionCombatEnemyBuildResult
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public List<MarbleSpawnConfig> LstEnemyMarble = new List<MarbleSpawnConfig>();
        public List<string> LstBuildLog = new List<string>();
    }

    public static class ExpeditionCombatEnemyResolver
    {
        private static readonly Random _random = new Random();

        // 动态敌人的职责拆分：
        // 1. 当前环境只提供“可能出现什么敌人类型”，也就是 marble_config_id 与 camp_config_id。
        // 2. 本场实际生成几个、生成成多少级，统一由 enemy profile + 当前阶段 + tier 决定。
        // 这样同一份环境候选池就可以在不同远征阶段复用，而不会把敌人强度写死在环境里。
        public static ExpeditionCombatEnemyBuildResult BuildEnemyRoster(
            ExpeditionRunState runState,
            ExpeditionNodeRecord nodeRecord,
            ExpeditionTable.ExpeditionCombatEncounterConfig encounterConfig)
        {
            var result = new ExpeditionCombatEnemyBuildResult();
            if (encounterConfig == null)
            {
                result.ErrorMessage = "Combat 遭遇配置为空。";
                result.LstBuildLog.Add(result.ErrorMessage);
                return result;
            }

            AddFixedEnemies(result, encounterConfig.EnemyMarbles);
            if (encounterConfig.LstDynamicEnemyGroup == null || encounterConfig.LstDynamicEnemyGroup.Count == 0)
            {
                result.IsSuccess = true;
                result.LstBuildLog.Add($"本场仅使用固定敌人，共 {result.LstEnemyMarble.Count} 个。");
                return result;
            }

            var environment = ExpeditionConfigBridge.ResolveEnvironment(runState?.CurrentEnvironmentConfigId);
            var lstCandidate = environment?.LstEnemyCandidate?
                .Where(candidate => candidate != null
                    && candidate.MarbleSpawnConfig != null
                    && !string.IsNullOrWhiteSpace(candidate.MarbleSpawnConfig.MarbleConfigId)
                    && !string.IsNullOrWhiteSpace(candidate.MarbleSpawnConfig.CampConfigId)
                    && candidate.Weight > 0)
                .ToList() ?? new List<ExpeditionTable.ExpeditionEnvironmentEnemyCandidateConfig>();
            if (lstCandidate.Count == 0)
            {
                result.ErrorMessage = $"当前环境缺少有效敌人候选池。environmentConfigId:{runState?.CurrentEnvironmentConfigId}";
                result.LstBuildLog.Add(result.ErrorMessage);
                return result;
            }

            var expeditionConfig = ExpeditionConfigBridge.ResolveExpedition(runState?.ExpeditionConfigId);
            var enemyProfile = ExpeditionConfigBridge.ResolveEnemyProfile(expeditionConfig?.EnemyProfileConfigId);
            if (enemyProfile == null)
            {
                result.ErrorMessage = $"远征缺少有效敌人强度档位配置。expeditionConfigId:{runState?.ExpeditionConfigId} enemyProfileConfigId:{expeditionConfig?.EnemyProfileConfigId}";
                result.LstBuildLog.Add(result.ErrorMessage);
                return result;
            }

            var progressStage = ExpeditionRewardContext.Create(runState, nodeRecord).ProgressStage;
            foreach (var group in encounterConfig.LstDynamicEnemyGroup.Where(group => group != null))
            {
                if (!TryResolveEnemyValue(enemyProfile.LstEnemyCount, progressStage, group.CountTier, out var enemyCount, out var countError))
                {
                    result.ErrorMessage = countError;
                    result.LstBuildLog.Add(countError);
                    return result;
                }

                if (!TryResolveEnemyValue(enemyProfile.LstEnemyLevel, progressStage, group.LevelTier, out var enemyLevel, out var levelError))
                {
                    result.ErrorMessage = levelError;
                    result.LstBuildLog.Add(levelError);
                    return result;
                }

                if (enemyCount <= 0)
                {
                    result.LstBuildLog.Add($"动态敌人组被解析为 0 个敌人，已跳过。countTier:{group.CountTier} levelTier:{group.LevelTier}");
                    continue;
                }

                if (enemyLevel <= 0)
                {
                    result.ErrorMessage = $"动态敌人组解析出的等级无效。level:{enemyLevel} levelTier:{group.LevelTier}";
                    result.LstBuildLog.Add(result.ErrorMessage);
                    return result;
                }

                for (int i = 0; i < enemyCount; i++)
                {
                    var candidate = DrawEnemyCandidate(lstCandidate);
                    if (candidate?.MarbleSpawnConfig == null)
                    {
                        result.ErrorMessage = $"动态敌人抽取失败。environmentConfigId:{runState?.CurrentEnvironmentConfigId}";
                        result.LstBuildLog.Add(result.ErrorMessage);
                        return result;
                    }

                    var enemySpawnConfig = CloneMarbleSpawnConfig(candidate.MarbleSpawnConfig, enemyLevel);
                    result.LstEnemyMarble.Add(enemySpawnConfig);
                    result.LstBuildLog.Add($"生成动态敌人 {enemySpawnConfig.MarbleConfigId}(Lv{enemySpawnConfig.Level})。");
                }
            }

            result.IsSuccess = true;
            result.LstBuildLog.Add($"本场最终敌方总数 {result.LstEnemyMarble.Count}。");
            return result;
        }

        private static void AddFixedEnemies(ExpeditionCombatEnemyBuildResult result, IEnumerable<MarbleSpawnConfig> lstFixedEnemy)
        {
            if (lstFixedEnemy == null)
                return;

            foreach (var fixedEnemy in lstFixedEnemy.Where(enemy => enemy != null))
            {
                result.LstEnemyMarble.Add(fixedEnemy);
                result.LstBuildLog.Add($"保留固定敌人 {fixedEnemy.MarbleConfigId}(Lv{fixedEnemy.Level})。");
            }
        }

        private static bool TryResolveEnemyValue(
            IEnumerable<ExpeditionTable.ExpeditionStageTierIntValueConfig> lstStageConfig,
            ExpeditionTable.EnumExpeditionRewardProgressStage progressStage,
            ExpeditionTable.EnumExpeditionRewardTier tier,
            out int value,
            out string errorMessage)
        {
            value = 0;
            errorMessage = string.Empty;

            var stageConfig = lstStageConfig?
                .FirstOrDefault(config => config != null && config.ProgressStage == progressStage);
            if (stageConfig == null)
            {
                errorMessage = $"敌人强度档位缺少阶段配置。progressStage:{progressStage}";
                return false;
            }

            var tierValue = stageConfig.LstValue?
                .FirstOrDefault(config => config != null && config.Tier == tier);
            if (tierValue == null)
            {
                errorMessage = $"敌人强度档位缺少档位映射。progressStage:{progressStage} tier:{tier}";
                return false;
            }

            value = tierValue.Value;
            return true;
        }

        private static ExpeditionTable.ExpeditionEnvironmentEnemyCandidateConfig DrawEnemyCandidate(
            List<ExpeditionTable.ExpeditionEnvironmentEnemyCandidateConfig> lstCandidate)
        {
            if (lstCandidate == null || lstCandidate.Count == 0)
                return null;

            var totalWeight = lstCandidate.Sum(candidate => candidate.Weight);
            if (totalWeight <= 0)
                return null;

            var weight = _random.Next(totalWeight);
            var cursor = 0;
            foreach (var candidate in lstCandidate)
            {
                if (weight >= cursor + candidate.Weight)
                {
                    cursor += candidate.Weight;
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static MarbleSpawnConfig CloneMarbleSpawnConfig(MarbleSpawnConfig source, int level)
        {
            // 这里刻意只复用候选里的类型与 camp。
            // 动态敌人的最终等级以 enemy profile 解析值为准，不沿用候选自带 level。
            var json = new JSONObject();
            json.Add("marble_config_id", new JSONString(source?.MarbleConfigId ?? string.Empty));
            json.Add("level", new JSONNumber(level));
            json.Add("camp_config_id", new JSONString(source?.CampConfigId ?? string.Empty));
            return MarbleSpawnConfig.DeserializeMarbleSpawnConfig(json);
        }
    }
}
