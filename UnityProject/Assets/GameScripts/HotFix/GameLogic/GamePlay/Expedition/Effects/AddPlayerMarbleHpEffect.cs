using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class AddPlayerMarbleHpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleHpEffectConfig _config;

        public AddPlayerMarbleHpEffect(ExpeditionTable.AddPlayerMarbleHpEffectConfig config)
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
                snapshot.CurrentHp = UnityEngine.Mathf.Clamp(snapshot.CurrentHp + _config.HpDelta, 0, snapshot.MaxHp);
                snapshot.IsDead = snapshot.CurrentHp <= 0;
                context.RunState.MarbleSnapshots[i] = snapshot;
            }

            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? (_config.HpDelta >= 0
                    ? $"全队恢复 {_config.HpDelta} 点生命。"
                    : $"全队失去 {System.Math.Abs(_config.HpDelta)} 点生命。")
                : _config.Summary);
        }
    }
}
