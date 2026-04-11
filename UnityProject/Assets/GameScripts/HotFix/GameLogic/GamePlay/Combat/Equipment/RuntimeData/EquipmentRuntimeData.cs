namespace GameLogic.Gameplay.Combat.Equipment
{
    public class EquipmentRuntimeData : RuntimeData
    {
        public EquipmentSlot Slot { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsBroken { get; set; }
    }
}
