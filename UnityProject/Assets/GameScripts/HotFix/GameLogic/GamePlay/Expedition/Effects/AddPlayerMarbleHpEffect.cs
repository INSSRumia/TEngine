using System.Collections.Generic;
using GameConfig.Gameplay;
using ExpeditionTable = GameConfig.Gameplay.Expedition;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Expedition
{
    public class AddPlayerMarbleHpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleHpEffectConfig _config;
        private const string TOKEN_HP = "hp";
        private static readonly Dictionary<string, string> _dictTokenValue = new Dictionary<string, string>()
        {
            {TOKEN_HP, "0"},
        };

        public AddPlayerMarbleHpEffect(ExpeditionTable.AddPlayerMarbleHpEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context.RunState.LstMarbleSnapshot == null)
                return;

            var hpValue = ExpeditionRewardResolver.ResolveHp(context, _config.Tier, _config.Value);
            var hpDelta = _config.Operation == EnumOperation.Sub
                ? -hpValue
                : hpValue;

            for (int i = 0; i < context.RunState.LstMarbleSnapshot.Count; i++)
            {
                if (!context.RunState.LstMarbleSnapshot[i].HasValue)
                    continue;

                var snapshot = context.RunState.LstMarbleSnapshot[i].Value;
                snapshot.CurrentHp = UnityEngine.Mathf.Clamp(snapshot.CurrentHp + hpDelta, 0, snapshot.MaxHp);
                snapshot.IsDead = snapshot.CurrentHp <= 0;
                context.RunState.LstMarbleSnapshot[i] = snapshot;
            }

            _dictTokenValue[TOKEN_HP] = System.Math.Abs(hpDelta).ToString();
            var fallbackSummary = hpDelta >= 0
                ? $"全队恢复 {hpDelta} 点生命。"
                : $"全队失去 {System.Math.Abs(hpDelta)} 点生命。";
            context.AddSummaryTemplate(_config.Summary, _dictTokenValue, fallbackSummary);
        }
    }
}
