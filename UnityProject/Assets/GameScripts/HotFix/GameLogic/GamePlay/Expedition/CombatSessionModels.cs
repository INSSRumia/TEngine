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
        public List<MarblePersistentData?> AlliedMarbles = new List<MarblePersistentData?>();
        public List<ExpeditionEnemyMarbleConfig> EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>();
    }

    [Serializable]
    public sealed class CombatSessionResult
    {
        public bool IsVictory;
        public int CrystalReward;
        public string Summary;
        public List<MarblePersistentData?> MarbleResults = new List<MarblePersistentData?>();
    }


}
