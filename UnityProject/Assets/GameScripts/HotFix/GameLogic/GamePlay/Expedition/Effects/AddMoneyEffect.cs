using System.Collections.Generic;
using GameConfig.Gameplay;
using UnityEngine.Pool;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public class AddMoneyEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddMoneyEffectConfig _config;
        private const string TOKEN_MONEY = "money";
        private static readonly Dictionary<string, string> _dictTokenValue = new Dictionary<string, string>()
        {
            {TOKEN_MONEY, "0"},
        };

        public AddMoneyEffect(ExpeditionTable.AddMoneyEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            var moneyValue = ExpeditionRewardResolver.ResolveMoney(context, _config.Tier, _config.Value);
            var moneyDelta = _config.Operation == EnumOperation.Sub
                ? -moneyValue
                : moneyValue;
            context.RunState.TotalMoneyGained += moneyDelta;
            context.AddMoneyDelta(moneyDelta);

            _dictTokenValue[TOKEN_MONEY] = System.Math.Abs(moneyDelta).ToString();
            var fallbackSummary = moneyDelta >= 0
                ? $"获得 {moneyDelta} 晶体。"
                : $"失去 {System.Math.Abs(moneyDelta)} 晶体。";
            context.AddSummaryTemplate(_config.Summary, _dictTokenValue, fallbackSummary);
        }
    }
}
