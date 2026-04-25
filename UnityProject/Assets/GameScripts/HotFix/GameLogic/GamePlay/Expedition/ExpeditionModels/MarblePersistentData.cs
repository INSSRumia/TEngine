using System;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public partial struct MarblePersistentData
    {
        public string PersistentId;
        public string ConfigId;
        public string DisplayName;
        public int Level;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;

        public static MarblePersistentData CreateDefault(string persistentId, string configId, string displayName, int level)
        {
            var maxHp = ExpeditionConfigBridge.ResolveMarbleMaxHp(configId, level);
            return new MarblePersistentData
            {
                PersistentId = persistentId,
                ConfigId = configId,
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
