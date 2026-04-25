using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public class AddMoneyEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddMoneyEffectConfig _config;

        public AddMoneyEffect(ExpeditionTable.AddMoneyEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            var moneyValue = ExpeditionRewardResolver.ResolveMoney(context, _config.Money);
            var moneyDelta = _config.Operation == ExpeditionTable.EnumExpeditionRewardOperation.Subtract
                ? -moneyValue
                : moneyValue;
            context.RunState.TotalMoneyGained += moneyDelta;
            context.AddMoneyDelta(moneyDelta);

            var dictTokenValue = new Dictionary<string, string>
            {
                ["money"] = System.Math.Abs(moneyDelta).ToString(),
            };
            var fallbackSummary = moneyDelta >= 0
                ? $"获得 {moneyDelta} 晶体。"
                : $"失去 {System.Math.Abs(moneyDelta)} 晶体。";
            context.AddSummaryTemplate(_config.Summary, dictTokenValue, fallbackSummary);
        }
    }
}
