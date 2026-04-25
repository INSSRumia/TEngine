using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class AddPlayerMarbleExpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleExpEffectConfig _config;

        public AddPlayerMarbleExpEffect(ExpeditionTable.AddPlayerMarbleExpEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context.RunState.MarbleSnapshots == null)
            {
                return;
            }

            for (int i = 0; i < context.RunState.MarbleSnapshots.Count; i++)
            {
                if (!context.RunState.MarbleSnapshots[i].HasValue)
                {
                    continue;
                }

                var snapshot = context.RunState.MarbleSnapshots[i].Value;
                snapshot.Exp += _config.ExpDelta;
                context.RunState.MarbleSnapshots[i] = snapshot;
            }

            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? $"全队获得 {_config.ExpDelta} 点经验。"
                : _config.Summary);
        }
    }
}
