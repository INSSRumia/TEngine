using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class AddMoneyEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddMoneyEffectConfig _config;

        public AddMoneyEffect(ExpeditionTable.AddMoneyEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            context.RunState.TotalMoneyGained += _config.MoneyDelta;
            context.AddMoneyDelta(_config.MoneyDelta);
            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? $"获得 {_config.MoneyDelta} 晶体。"
                : _config.Summary);
        }
    }
}
