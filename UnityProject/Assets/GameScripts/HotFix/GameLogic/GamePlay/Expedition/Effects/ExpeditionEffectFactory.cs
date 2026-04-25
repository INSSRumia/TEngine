using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

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
}
