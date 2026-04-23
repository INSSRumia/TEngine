using System;
using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class CombatSessionRequest
    {
        public string SessionId;
        public string NodeId;
        public string CombatId;
        public string Title;
        public List<string> ActiveBuffIds = new List<string>();
        public List<MarblePersistentData?> AlliedMarbles = new List<MarblePersistentData?>();
        public List<ExpeditionTable.ExpeditionEnemyMarbleConfig> EnemyMarbles = new List<ExpeditionTable.ExpeditionEnemyMarbleConfig>();
    }

    [Serializable]
    public sealed class CombatSessionResult
    {
        public bool IsVictory;
        public string Summary;
        public List<MarblePersistentData?> MarbleResults = new List<MarblePersistentData?>();
    }
}
