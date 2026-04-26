using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Expedition
{
    public class AddPlayerMarbleExpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleExpEffectConfig _config;

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

            var dictTokenValue = DictionaryPool<string, string>.Get();
            dictTokenValue.Add("exp", expDelta.ToString());
            var fallbackSummary = expDelta >= 0
                ? $"全队获得 {expDelta} 点经验。"
                : $"全队失去 {System.Math.Abs(expDelta)} 点经验。";
            context.AddSummaryTemplate(_config.Summary, dictTokenValue, fallbackSummary);
            DictionaryPool<string, string>.Release(dictTokenValue);
        }
    }
}
