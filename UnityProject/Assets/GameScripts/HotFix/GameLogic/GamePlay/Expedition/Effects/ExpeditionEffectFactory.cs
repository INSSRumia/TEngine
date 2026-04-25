using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig.Gameplay.Combat;
using ExpeditionTable = GameConfig.Gameplay.Expedition;
using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionEffectFactory
    {
        public static void ExecuteEffects(IEnumerable<ExpeditionTable.ExpeditionEffectConfig> configs, ExpeditionEffectExecutionContext context)
        {
            if (configs == null || context == null)
            {
                return;
            }

            foreach (var config in configs)
            {
                CreateEffect(config)?.Execute(context);
            }
        }

        public static IExpeditionEffect CreateEffect(ExpeditionTable.ExpeditionEffectConfig config)
        {
            return config switch
            {
                ExpeditionTable.AddMoneyEffectConfig moneyConfig => new AddMoneyEffect(moneyConfig),
                ExpeditionTable.AddPlayerMarbleExpEffectConfig expConfig => new AddPlayerMarbleExpEffect(expConfig),
                ExpeditionTable.AddPlayerMarbleHpEffectConfig hpConfig => new AddPlayerMarbleHpEffect(hpConfig),
                ExpeditionTable.AddPlayerMarbleEffectConfig marbleConfig => new AddPlayerMarbleEffect(marbleConfig),
                ExpeditionTable.ChangeEnvironmentEffectConfig environmentConfig => new ChangeEnvironmentEffect(environmentConfig),
                _ => null,
            };
        }
    }

    public class ChangeEnvironmentEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.ChangeEnvironmentEffectConfig _config;

        public ChangeEnvironmentEffect(ExpeditionTable.ChangeEnvironmentEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context?.RunState == null || _config == null)
                return;

            var isSuccess = context.RunState.ChangeEnvironment(_config.TargetEnvironmentConfigId);
            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? $"环境切换为 {_config.TargetEnvironmentConfigId}。"
                : _config.Summary);

            if (!isSuccess)
                context.NodeRecord?.AddRouteDecisionLog($"环境切换失败，目标环境不存在: {_config.TargetEnvironmentConfigId}");
        }
    }

    public static class ExpeditionRewardResolver
    {
        private static readonly Random _random = new Random();

        public static int ResolveMoney(ExpeditionEffectExecutionContext context, ExpeditionTable.ExpeditionScaledValueConfig scaledValueConfig)
        {
            return ResolveStageValue(
                context,
                scaledValueConfig,
                context?.RewardContext?.RewardProfileConfig?.LstMoney,
                "money");
        }

        public static int ResolveExp(ExpeditionEffectExecutionContext context, ExpeditionTable.ExpeditionScaledValueConfig scaledValueConfig)
        {
            return ResolveStageValue(
                context,
                scaledValueConfig,
                context?.RewardContext?.RewardProfileConfig?.LstExp,
                "exp");
        }

        public static int ResolveHp(ExpeditionEffectExecutionContext context, ExpeditionTable.ExpeditionScaledValueConfig scaledValueConfig)
        {
            return ResolveStageValue(
                context,
                scaledValueConfig,
                context?.RewardContext?.RewardProfileConfig?.LstHp,
                "hp");
        }

        public static int ResolveMarbleCount(ExpeditionEffectExecutionContext context, ExpeditionTable.ExpeditionScaledValueConfig scaledValueConfig)
        {
            return ResolveStageValue(
                context,
                scaledValueConfig,
                context?.RewardContext?.RewardProfileConfig?.LstMarbleCount,
                "marble_count");
        }

        public static MarbleSpawnConfig ResolveRecruitCandidate(ExpeditionEffectExecutionContext context, ExpeditionTable.ExpeditionScaledValueConfig scaledValueConfig)
        {
            if (context?.RewardContext?.RewardProfileConfig == null || scaledValueConfig == null)
            {
                LogMissingConfig(context, "recruit_candidate", scaledValueConfig?.RewardTier.ToString() ?? "<null>");
                return null;
            }

            var lstCandidate = context.RewardContext.RewardProfileConfig.LstRecruitCandidate?
                .Where(candidate => candidate != null
                    && candidate.MarbleSpawnConfig != null
                    && candidate.Weight > 0
                    && candidate.RewardTier == scaledValueConfig.RewardTier)
                .ToList() ?? new List<ExpeditionTable.ExpeditionRecruitRewardCandidateConfig>();
            if (lstCandidate.Count == 0)
            {
                LogMissingConfig(context, "recruit_candidate", scaledValueConfig.RewardTier.ToString());
                return null;
            }

            var totalWeight = lstCandidate.Sum(candidate => candidate.Weight);
            if (totalWeight <= 0)
            {
                LogMissingConfig(context, "recruit_candidate_weight", scaledValueConfig.RewardTier.ToString());
                return null;
            }

            var weight = _random.Next(totalWeight);
            var cursor = 0;
            for (int index = 0; index < lstCandidate.Count; index++)
            {
                var candidate = lstCandidate[index];
                if (weight >= cursor + candidate.Weight)
                {
                    cursor += candidate.Weight;
                    continue;
                }

                return candidate.MarbleSpawnConfig;
            }

            LogMissingConfig(context, "recruit_candidate_draw", scaledValueConfig.RewardTier.ToString());
            return null;
        }

        private static int ResolveStageValue(
            ExpeditionEffectExecutionContext context,
            ExpeditionTable.ExpeditionScaledValueConfig scaledValueConfig,
            IEnumerable<ExpeditionTable.ExpeditionRewardStageValueConfig> lstStageConfig,
            string rewardType)
        {
            if (context?.RewardContext?.RewardProfileConfig == null || scaledValueConfig == null)
            {
                LogMissingConfig(context, rewardType, scaledValueConfig?.RewardTier.ToString() ?? "<null>");
                return 0;
            }

            var stageConfig = lstStageConfig?
                .FirstOrDefault(config => config != null && config.ProgressStage == context.RewardContext.ProgressStage);
            if (stageConfig == null)
            {
                LogMissingConfig(context, $"{rewardType}_stage", context.RewardContext.ProgressStage.ToString());
                return 0;
            }

            var tierValue = stageConfig.LstValue?
                .FirstOrDefault(config => config != null && config.RewardTier == scaledValueConfig.RewardTier);
            if (tierValue == null)
            {
                LogMissingConfig(context, rewardType, $"{context.RewardContext.ProgressStage}:{scaledValueConfig.RewardTier}");
                return 0;
            }

            return tierValue.Value;
        }

        private static void LogMissingConfig(ExpeditionEffectExecutionContext context, string rewardType, string detail)
        {
            var expeditionConfigId = context?.RunState?.ExpeditionConfigId ?? "<unknown>";
            var rewardProfileConfigId = context?.RewardContext?.RewardProfileConfig?.RewardProfileConfigId ?? "<none>";
            var message = $"[远征奖励] 缺少配置 rewardType={rewardType} detail={detail} expeditionConfigId={expeditionConfigId} rewardProfileConfigId={rewardProfileConfigId}";
            Log.Warning(message);
            context?.RunState?.DebugLogs?.Add(message);
            context?.NodeRecord?.AddRouteDecisionLog($"奖励解析缺少配置: {rewardType} ({detail})");
        }
    }
}
