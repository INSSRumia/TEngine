using System;
using System.Collections.Generic;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class CombatSessionRequest
    {
        public string SessionId;
        public string NodeId;
        public string CombatId;
        public string Title;
        public List<MarblePersistentData?> LstAlliedMarble = new ();
        public List<ExpeditionEnemyMarbleConfig> LstEnemyMarble = new ();
    }

    [Serializable]
    public sealed class CombatSessionResult
    {
        public bool IsVictory;
        public string Summary;
        public List<MarblePersistentData?> LstMarbleResult = new ();
    }
}
