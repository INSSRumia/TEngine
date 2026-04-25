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
                _ => null,
            };
        }
    }
}
