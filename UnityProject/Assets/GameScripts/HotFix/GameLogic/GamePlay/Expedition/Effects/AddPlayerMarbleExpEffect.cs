using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Expedition
{
    public class AddPlayerMarbleExpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleExpEffectConfig _config;
        private const string TOKEN_EXP = "exp";
        private static readonly Dictionary<string, string> _dictTokenValue = new Dictionary<string, string>()
        {
            {TOKEN_EXP, "0"},
        };

        public AddPlayerMarbleExpEffect(ExpeditionTable.AddPlayerMarbleExpEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context.RunState.MarbleSnapshots == null)
                return;

            var expDelta = ExpeditionRewardResolver.ResolveExp(context, _config.Tier, _config.Value);

            for (int i = 0; i < context.RunState.MarbleSnapshots.Count; i++)
            {
                if (!context.RunState.MarbleSnapshots[i].HasValue)
                    continue;

                var snapshot = context.RunState.MarbleSnapshots[i].Value;
                snapshot.Exp += expDelta;
                context.RunState.MarbleSnapshots[i] = snapshot;
            }

            _dictTokenValue[TOKEN_EXP] = expDelta.ToString();
            var fallbackSummary = expDelta >= 0
                ? $"全队获得 {expDelta} 点经验。"
                : $"全队失去 {System.Math.Abs(expDelta)} 点经验。";
            context.AddSummaryTemplate(_config.Summary, _dictTokenValue, fallbackSummary);
        }
    }
}
