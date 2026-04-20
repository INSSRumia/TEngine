using System;
using System.Collections.Generic;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class CombatSessionRequest
    {
        public string SessionId;
        public string NodeId;
        public string CombatId;
        public string Title;
        public int VictoryCrystalReward;
        public int VictoryExpReward;
        public List<string> ActiveBuffIds = new List<string>();
        public List<MarblePersistentDataSnapshot> AlliedMarbles = new List<MarblePersistentDataSnapshot>();
        public List<ExpeditionEnemyMarbleConfig> EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>();
    }

    [Serializable]
    public sealed class CombatSessionResult
    {
        public bool IsVictory;
        public int CrystalReward;
        public string Summary;
        public List<CombatSessionMarbleResult> MarbleResults = new List<CombatSessionMarbleResult>();
    }

    [Serializable]
    public sealed class CombatSessionMarbleResult
    {
        public string PersistentId;
        public string ConfigId;
        public int RemainingHp;
        public int MaxHp;
        public int ExpDelta;
        public bool IsDead;
    }
}
