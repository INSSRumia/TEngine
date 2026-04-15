using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowRuntimeData : WeaponRuntimeData
    {
        public bool CanFire { get; set; }
        public BowRuntimeData(EquipmentConfig config, BowLevelConfig levelConfig) : base(config, levelConfig)
        {
            CanFire = false;
        }
    }
}
