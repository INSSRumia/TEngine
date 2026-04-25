using System;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public partial struct MarblePersistentData
    {
        public string MarbleInstId;
        public string MarbleConfigId;
        public string DisplayName;
        public int Level;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;

        public static MarblePersistentData CreateDefault(string marbleInstId, string marbleConfigId, string displayName, int level)
        {
            var maxHp = ExpeditionConfigBridge.ResolveMarbleMaxHp(marbleConfigId, level);
            return new MarblePersistentData
            {
                MarbleInstId = marbleInstId,
                MarbleConfigId = marbleConfigId,
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
