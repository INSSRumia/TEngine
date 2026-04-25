using System;
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
            if (context?.RunState?.MarbleSnapshots == null || _config?.MarbleSpawnConfig == null)
                return;

            if (_config.Count <= 0)
            {
                context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                    ? "没有新的 Marble 加入队伍。"
                    : _config.Summary);
                return;
            }

            var displayName = ExpeditionConfigBridge.ResolveMarbleDisplayName(_config.MarbleSpawnConfig.MarbleConfigId);
            for (int i = 0; i < _config.Count; i++)
            {
                var marbleInstId = CreateMarbleInstId(_config.MarbleSpawnConfig.MarbleConfigId);
                context.RunState.MarbleSnapshots.Add(MarblePersistentData.CreateDefault(marbleInstId, _config.MarbleSpawnConfig));
            }

            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? $"队伍加入 {_config.Count} 名 {GetDisplayName(displayName, _config.MarbleSpawnConfig.MarbleConfigId)}。"
                : _config.Summary);
        }

        private static string CreateMarbleInstId(string marbleConfigId)
        {
            var safeConfigId = string.IsNullOrWhiteSpace(marbleConfigId) ? "unknown" : marbleConfigId;
            return $"expedition_added_{safeConfigId}_{Guid.NewGuid():N}";
        }

        private static string GetDisplayName(string displayName, string marbleConfigId)
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            return string.IsNullOrWhiteSpace(marbleConfigId) ? "Marble" : marbleConfigId;
        }
    }
}
