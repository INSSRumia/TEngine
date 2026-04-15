using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponRuntimeData : EquipmentRuntimeData
    {
        public float CooldownRemaining { get; set; }
        public WeaponRuntimeData(EquipmentConfig config, WeaponLevelConfig levelConfig) : base(config, levelConfig) { }
    }
}
