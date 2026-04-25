using System;
using System.Collections.Generic;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class CombatSessionRequest
    {
        public string CombatSessionInstId;
        public string NodeConfigId;
        public string CombatEncounterConfigId;
        public string BattlefieldConfigId;
        public string Title;
        public List<MarblePersistentData?> LstAlliedMarble = new ();
        public List<MarbleSpawnConfig> LstEnemyMarble = new ();
    }

    [Serializable]
    public sealed class CombatSessionResult
    {
        public bool IsVictory;
        public string Summary;
        public List<MarblePersistentData?> LstMarbleResult = new ();
    }
}
