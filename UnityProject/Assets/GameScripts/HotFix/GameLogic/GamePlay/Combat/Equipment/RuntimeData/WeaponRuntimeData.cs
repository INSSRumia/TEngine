namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponRuntimeData : EquipmentRuntimeData
    {
        public int? Attack { get; set; }
        public bool IsDamageByVelocity { get; set; }
        public float Cooldown { get; set; }
        public float CooldownRemaining { get; set; }
        public WeaponRuntimeData(
            string configId, 
            int instId, 
            EnumEquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int? attack, 
            bool isDamageByVelocity, 
            float cooldown) : base(configId, instId, slot, isEquipped, isBroken)
        {
            SetData(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown, 0);
        }

        public void SetData(
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
            base.SetData(configId, instId, slot, isEquipped, isBroken);
            Attack = attack;
            IsDamageByVelocity = isDamageByVelocity;
            Cooldown = cooldown;
            CooldownRemaining = cooldownRemaining;
        }
    }
}
