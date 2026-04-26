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
        private const string TOKEN_COUNT = "count";
        private const string TOKEN_MARBLE_NAME = "marble_name";
        private static readonly Dictionary<string, string> _dictTokenValue = new Dictionary<string, string>()
        {
            {TOKEN_COUNT, "0"},
            {TOKEN_MARBLE_NAME, "Marble"},
        };

        public AddPlayerMarbleEffect(ExpeditionTable.AddPlayerMarbleEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context?.RunState?.LstMarbleSnapshot == null || _config == null)
                return;

            var targetCount = ExpeditionRewardResolver.ResolveMarbleCount(context, _config.MarbleCountTier, _config.MarbleCountValue);
            if (targetCount <= 0)
            {
                context.AddSummaryTemplate(
                    _config.Summary,
                    _dictTokenValue,
                    "没有新的 Marble 加入队伍。");
                return;
            }

            var lstAddedDisplayName = new List<string>();
            var addedCount = 0;
            for (int i = 0; i < targetCount; i++)
            {
                var marbleSpawnConfig = ExpeditionRewardResolver.ResolveRecruitCandidate(context, _config.MarbleTypeTier, _config.MarbleTypeValue);
                if (marbleSpawnConfig == null)
                    continue;

                var marbleInstId = CreateMarbleInstId(marbleSpawnConfig.MarbleConfigId);
                context.RunState.LstMarbleSnapshot.Add(MarblePersistentData.CreateDefault(marbleInstId, marbleSpawnConfig));
                lstAddedDisplayName.Add(GetDisplayName(marbleSpawnConfig));
                addedCount++;
            }

            var marbleName = ResolveSummaryMarbleName(lstAddedDisplayName);
            _dictTokenValue[TOKEN_COUNT] = addedCount.ToString();
            _dictTokenValue[TOKEN_MARBLE_NAME] = marbleName;
            var fallbackSummary = addedCount > 0
                ? $"队伍加入 {addedCount} 名 {marbleName}。"
                : "未能招募到任何 Marble。";
            context.AddSummaryTemplate(_config.Summary, _dictTokenValue, fallbackSummary);
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
