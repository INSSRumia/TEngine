using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig.Gameplay.Combat;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public class AddPlayerMarbleEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleEffectConfig _config;

        public AddPlayerMarbleEffect(ExpeditionTable.AddPlayerMarbleEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context?.RunState?.MarbleSnapshots == null || _config?.MarbleCount == null)
                return;

            var targetCount = ExpeditionRewardResolver.ResolveMarbleCount(context, _config.MarbleCount);
            if (targetCount <= 0)
            {
                context.AddSummaryTemplate(
                    _config.Summary,
                    new Dictionary<string, string>
                    {
                        ["count"] = "0",
                        ["marble_name"] = "Marble",
                    },
                    "没有新的 Marble 加入队伍。");
                return;
            }

            var lstAddedDisplayName = new List<string>();
            var addedCount = 0;
            for (int i = 0; i < targetCount; i++)
            {
                var marbleSpawnConfig = ExpeditionRewardResolver.ResolveRecruitCandidate(context, _config.MarbleCount);
                if (marbleSpawnConfig == null)
                    continue;

                var marbleInstId = CreateMarbleInstId(marbleSpawnConfig.MarbleConfigId);
                context.RunState.MarbleSnapshots.Add(MarblePersistentData.CreateDefault(marbleInstId, marbleSpawnConfig));
                lstAddedDisplayName.Add(GetDisplayName(marbleSpawnConfig));
                addedCount++;
            }

            var marbleName = ResolveSummaryMarbleName(lstAddedDisplayName);
            var dictTokenValue = new Dictionary<string, string>
            {
                ["count"] = addedCount.ToString(),
                ["marble_name"] = marbleName,
            };
            var fallbackSummary = addedCount > 0
                ? $"队伍加入 {addedCount} 名 {marbleName}。"
                : "未能招募到任何 Marble。";
            context.AddSummaryTemplate(_config.Summary, dictTokenValue, fallbackSummary);
        }

        private static string CreateMarbleInstId(string marbleConfigId)
        {
            var safeConfigId = string.IsNullOrWhiteSpace(marbleConfigId) ? "unknown" : marbleConfigId;
            return $"expedition_added_{safeConfigId}_{Guid.NewGuid():N}";
        }

        private static string GetDisplayName(MarbleSpawnConfig marbleSpawnConfig)
        {
            var displayName = ExpeditionConfigBridge.ResolveMarbleDisplayName(marbleSpawnConfig?.MarbleConfigId);
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            return string.IsNullOrWhiteSpace(marbleSpawnConfig?.MarbleConfigId) ? "Marble" : marbleSpawnConfig.MarbleConfigId;
        }

        private static string ResolveSummaryMarbleName(IEnumerable<string> lstDisplayName)
        {
            var lstValidName = lstDisplayName?
                .Where(displayName => !string.IsNullOrWhiteSpace(displayName))
                .Distinct()
                .ToList() ?? new List<string>();
            if (lstValidName.Count == 0)
                return "Marble";

            if (lstValidName.Count == 1)
                return lstValidName[0];

            return string.Join("、", lstValidName);
        }
    }
}
