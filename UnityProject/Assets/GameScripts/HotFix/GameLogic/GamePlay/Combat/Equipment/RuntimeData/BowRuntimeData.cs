using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowRuntimeData : WeaponRuntimeData
    {
        public bool CanFire { get; set; }
        public BowRuntimeData(string configId, int level) : base(configId, level) { }
    }
}
