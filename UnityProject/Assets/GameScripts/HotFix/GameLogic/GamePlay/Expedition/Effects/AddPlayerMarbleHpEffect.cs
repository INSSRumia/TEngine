using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public class AddPlayerMarbleHpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleHpEffectConfig _config;

        public AddPlayerMarbleHpEffect(ExpeditionTable.AddPlayerMarbleHpEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context.RunState.MarbleSnapshots == null)
                return;

            var hpValue = ExpeditionRewardResolver.ResolveHp(context, _config.Hp);
            var hpDelta = _config.Operation == ExpeditionTable.EnumExpeditionRewardOperation.Subtract
                ? -hpValue
                : hpValue;

            for (int i = 0; i < context.RunState.MarbleSnapshots.Count; i++)
            {
                if (!context.RunState.MarbleSnapshots[i].HasValue)
                    continue;

                var snapshot = context.RunState.MarbleSnapshots[i].Value;
                snapshot.CurrentHp = UnityEngine.Mathf.Clamp(snapshot.CurrentHp + hpDelta, 0, snapshot.MaxHp);
                snapshot.IsDead = snapshot.CurrentHp <= 0;
                context.RunState.MarbleSnapshots[i] = snapshot;
            }

            var dictTokenValue = new Dictionary<string, string>
            {
                ["hp"] = System.Math.Abs(hpDelta).ToString(),
            };
            var fallbackSummary = hpDelta >= 0
                ? $"全队恢复 {hpDelta} 点生命。"
                : $"全队失去 {System.Math.Abs(hpDelta)} 点生命。";
            context.AddSummaryTemplate(_config.Summary, dictTokenValue, fallbackSummary);
        }
    }
}
