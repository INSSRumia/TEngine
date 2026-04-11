namespace GameLogic.Gameplay.Combat.Equipment
{
    public class EquipmentRuntimeData : RuntimeData
    {
        public EquipmentSlot Slot { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsBroken { get; set; }

        public EquipmentRuntimeData(
            string configId, 
            int instId, 
            EquipmentSlot slot,
            bool isEquipped,
            bool isBroken) : base(configId, instId)
        {
            SetData(configId, instId, slot, isEquipped, isBroken);
        }

        public void SetData(
            string configId, 
            int instId, 
            EquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken)
        {
            base.SetData(configId, instId);
            Slot = slot;
            IsEquipped = isEquipped;
            IsBroken = isBroken;
        }
    }
}
