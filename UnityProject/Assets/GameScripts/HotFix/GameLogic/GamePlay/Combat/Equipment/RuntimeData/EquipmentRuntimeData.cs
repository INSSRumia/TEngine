namespace GameLogic.Gameplay.Combat.Equipment
{
    public class EquipmentRuntimeData : RuntimeData
    {
        public EnumEquipmentSlot Slot { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsBroken { get; set; }

        public EquipmentRuntimeData(string configId, int level) : base(configId, level, EquipmentFactory.GetNextInstId) { }
    }
}
