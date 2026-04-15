using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class EquipmentRuntimeData : RuntimeData
    {
        public EnumEquipmentSlot Slot { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsBroken { get; set; }

        public EquipmentRuntimeData(EquipmentConfig config, EquipmentLevelConfig levelConfig) : base(config.ConfigId, levelConfig.Level, EquipmentFactory.GetNextInstId)
        {
        }
    }
}
