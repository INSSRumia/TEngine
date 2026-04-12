namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SwordRuntimeData : WeaponRuntimeData
    {
        public SwordRuntimeData(string configId, 
            int instId, 
            EnumEquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int? attack, 
            bool isDamageByVelocity, 
            float cooldown) : base(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown)
        {
            SetData(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown, 0);
        }

        public new void SetData(
            string configId, 
            int instId, 
            EnumEquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int? attack, 
            bool isDamageByVelocity, 
            float cooldown, 
            float cooldownRemaining)
        {
            base.SetData(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown, cooldownRemaining);
        }
    }
}
