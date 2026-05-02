using System.Collections.Generic;
using System.Linq;
using TEngine;
using GameConfig.Gameplay.Combat;
using GameConfig.Gameplay.Expedition;
using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        private static readonly System.Random _battlefieldRandom = new System.Random();

        public bool StartCombatDebug()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
                return false;

            var hasEncounter = ExpeditionConfigBridge.TryResolveDebugCombatEncounter(string.Empty, out var expeditionConfig, out var encounter);
            if (!hasEncounter || encounter == null)
            {
                Log.Warning("[远征流程控制器] StartCombatDebug 已中止，因为未找到战斗遭遇配置。");
                return false;
            }

            var debugRunState = expeditionConfig == null
                ? null
                : new ExpeditionRunState
                {
                    ExpeditionInstId = "combat_debug_run",
                    ExpeditionConfigId = expeditionConfig.ExpeditionConfigId,
                    CurrentEnvironmentConfigId = expeditionConfig.InitialEnvironmentConfigId,
                    LstRouteConfig = expeditionConfig.Route?.Where(node => node != null).ToList() ?? new List<ExpeditionRouteNodeConfig>(),
                };
            var debugNodeRecord = new ExpeditionNodeRecord
            {
                NodeConfigId = "combat_debug_node",
                EntryOrder = 1,
            };
            var enemyBuildResult = ExpeditionCombatEnemyResolver.BuildEnemyRoster(debugRunState, debugNodeRecord, encounter);
            if (!enemyBuildResult.IsSuccess)
            {
                Log.Warning($"[远征流程控制器] StartCombatDebug 已中止，因为敌方阵容解析失败。{enemyBuildResult.ErrorMessage}");
                return false;
            }

            var request = new CombatSessionRequest
            {
                CombatSessionInstId = "combat_debug_session",
                NodeConfigId = "combat_debug_node",
                CombatEncounterConfigId = encounter.CombatEncounterConfigId,
                Title = encounter.Title,
                LstAlliedMarble = _persistentData.LstMarbles
                    .Where(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0)
                    .ToList(),
                LstEnemyMarble = enemyBuildResult.LstEnemyMarble,
            };
            var battlefieldConfigId = ResolveBattlefieldConfigId(encounter, debugRunState?.CurrentEnvironmentConfigId ?? string.Empty);
            if (string.IsNullOrWhiteSpace(battlefieldConfigId))
            {
                Log.Warning($"[远征流程控制器] StartCombatDebug 已中止，因为无法解析场地。combatEncounterConfigId:{encounter.CombatEncounterConfigId}");
                return false;
            }

            request.BattlefieldConfigId = battlefieldConfigId;

            GameModule.UI.CloseUI<ExpeditionMainUI>();
            return ExpeditionCombatSessionController.Instance.StartSession(request, OnDebugCombatCompleted);
        }

        public CombatSessionRequest BuildCombatSessionRequest()
        {
            var node = GetCurrentNode();
            var record = CurrentRun?.GetCurrentRecord();
            var combatConfig = node == null ? null : ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterConfigId);
            if (combatConfig == null || CurrentRun == null)
                return null;

            var battlefieldConfigId = ResolveBattlefieldConfigId(combatConfig, CurrentRun.CurrentEnvironmentConfigId);
            if (string.IsNullOrWhiteSpace(battlefieldConfigId))
            {
                CurrentRun.DebugTrace.RecordCombat(
                    "节点无法解析场地。",
                    CurrentRun.Phase,
                    node.NodeConfigId,
                    record?.QueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Warning);
                Log.Warning($"[远征流程控制器] Combat 节点无法解析场地。nodeConfigId:{node.NodeConfigId} combatEncounterConfigId:{combatConfig.CombatEncounterConfigId}");
                return null;
            }

            var enemyBuildResult = ExpeditionCombatEnemyResolver.BuildEnemyRoster(CurrentRun, record, combatConfig);
            if (enemyBuildResult.LstBuildLog != null)
            {
                foreach (var log in enemyBuildResult.LstBuildLog)
                {
                    if (string.IsNullOrWhiteSpace(log))
                        continue;

                    CurrentRun.DebugTrace.RecordCombat(log, CurrentRun.Phase, node.NodeConfigId, record?.QueueEntryInstId);
                }
            }

            if (!enemyBuildResult.IsSuccess)
            {
                var errorMessage = string.IsNullOrWhiteSpace(enemyBuildResult.ErrorMessage)
                    ? $"[远征流程控制器] Combat 节点敌方阵容解析失败。nodeConfigId:{node.NodeConfigId}"
                    : $"[远征流程控制器] Combat 节点敌方阵容解析失败。nodeConfigId:{node.NodeConfigId} error:{enemyBuildResult.ErrorMessage}";
                CurrentRun.DebugTrace.RecordCombat(
                    errorMessage,
                    CurrentRun.Phase,
                    node.NodeConfigId,
                    record?.QueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Warning);
                Log.Warning(errorMessage);
                return null;
            }

            return new CombatSessionRequest
            {
                CombatSessionInstId = $"{CurrentRun.ExpeditionInstId}_{combatConfig.CombatEncounterConfigId}",
                NodeConfigId = node.NodeConfigId,
                CombatEncounterConfigId = combatConfig.CombatEncounterConfigId,
                BattlefieldConfigId = battlefieldConfigId,
                Title = combatConfig.Title,
                LstAlliedMarble = CurrentRun.LstMarbleSnapshot
                    .Where(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0)
                    .ToList(),
                // 敌方 roster 在进入 Combat 前已经完全展开：
                // 固定敌人直接保留；
                // 动态敌人先抽类型，再用 enemy profile 覆盖等级。
                LstEnemyMarble = enemyBuildResult.LstEnemyMarble,
            };
        }

        public bool StartCurrentCombatSession()
        {
            var request = BuildCombatSessionRequest();
            if (request == null)
                return false;

            return ExpeditionCombatSessionController.Instance.StartSession(request, SubmitCombatResult);
        }

        private void OnDebugCombatCompleted(CombatSessionResult result)
        {
            Log.Info($"[远征流程控制器] 战斗调试完成。胜利:{result?.IsVictory}");
            OpenEntryUi();
        }

        private static string ResolveBattlefieldConfigId(ExpeditionCombatEncounterConfig encounter, string environmentConfigId)
        {
            if (encounter == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(encounter.BattlefieldConfigId))
            {
                if (BattlefieldFactory.ResolveConfig(encounter.BattlefieldConfigId) != null)
                    return encounter.BattlefieldConfigId;

                Log.Warning($"[远征流程控制器] Combat 遭遇引用了不存在的场地。combatEncounterConfigId:{encounter.CombatEncounterConfigId} battlefieldConfigId:{encounter.BattlefieldConfigId}");
                return string.Empty;
            }

            var environment = ExpeditionConfigBridge.ResolveEnvironment(environmentConfigId);
            if (environment == null)
            {
                Log.Warning($"[远征流程控制器] Combat 遭遇未配置显式场地，且当前环境不存在。combatEncounterConfigId:{encounter.CombatEncounterConfigId} environmentConfigId:{environmentConfigId}");
                return string.Empty;
            }

            var lstCandidate = environment.LstBattlefield?
                .Where(candidate => candidate != null && !string.IsNullOrWhiteSpace(candidate.BattlefieldConfigId) && candidate.Weight > 0)
                .ToList() ?? new List<ExpeditionEnvironmentBattlefieldConfig>();
            var totalWeight = lstCandidate.Sum(candidate => candidate.Weight);
            if (totalWeight <= 0)
            {
                Log.Warning($"[远征流程控制器] 当前环境没有有效场地候选。environmentConfigId:{environment.EnvironmentConfigId}");
                return string.Empty;
            }

            var weight = _battlefieldRandom.Next(totalWeight);
            var cursor = 0;
            foreach (var candidate in lstCandidate)
            {
                if (weight >= cursor + candidate.Weight)
                {
                    cursor += candidate.Weight;
                    continue;
                }

                if (BattlefieldFactory.ResolveConfig(candidate.BattlefieldConfigId) != null)
                    return candidate.BattlefieldConfigId;

                Log.Warning($"[远征流程控制器] 环境场地候选引用了不存在的场地。environmentConfigId:{environment.EnvironmentConfigId} battlefieldConfigId:{candidate.BattlefieldConfigId}");
                return string.Empty;
            }

            return string.Empty;
        }
    }
}
