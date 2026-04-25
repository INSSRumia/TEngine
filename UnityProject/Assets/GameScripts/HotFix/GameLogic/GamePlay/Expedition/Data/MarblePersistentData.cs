using System;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public partial struct MarblePersistentData
    {
        public string MarbleInstId;
        public string MarbleConfigId;
        public string CampConfigId;
        public string DisplayName;
        public int Level;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;

        public static MarblePersistentData CreateDefault(string marbleInstId, MarbleSpawnConfig marbleSpawnConfig)
        {
            if (marbleSpawnConfig == null)
                return default;

            return CreateDefault(
                marbleInstId,
                marbleSpawnConfig.MarbleConfigId,
                marbleSpawnConfig.CampConfigId,
                ExpeditionConfigBridge.ResolveMarbleDisplayName(marbleSpawnConfig.MarbleConfigId),
                marbleSpawnConfig.Level);
        }

        public static MarblePersistentData CreateDefault(string marbleInstId, string marbleConfigId, string displayName, int level)
        {
            return CreateDefault(marbleInstId, marbleConfigId, string.Empty, displayName, level);
        }

        public static MarblePersistentData CreateDefault(string marbleInstId, string marbleConfigId, string campConfigId, string displayName, int level)
        {
            var maxHp = ExpeditionConfigBridge.ResolveMarbleMaxHp(marbleConfigId, level);
            return new MarblePersistentData
            {
                MarbleInstId = marbleInstId,
                MarbleConfigId = marbleConfigId,
                CampConfigId = campConfigId,
                DisplayName = displayName,
                Level = level,
                CurrentHp = maxHp,
                MaxHp = maxHp,
                Exp = 0,
                IsDead = false,
            };
        }
    }
}
