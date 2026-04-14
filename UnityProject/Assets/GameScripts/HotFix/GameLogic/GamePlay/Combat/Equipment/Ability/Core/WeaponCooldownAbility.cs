namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponCooldownAbility : EquipmentAbility, IAbilityUpdate
    {
        private WeaponEquipment _owner;
        public float Cooldown {get; private set;}
        public WeaponCooldownAbility(float cooldown)
        {
            Cooldown = cooldown;
        }
        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is WeaponEquipment weaponEquipment)
                _owner = weaponEquipment;
        }
        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }
        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_owner.RuntimeData.IsBroken)
                return;

            _owner.RuntimeData.CooldownRemaining -= elapseSeconds;
            if (_owner.RuntimeData.CooldownRemaining < 0f)
                _owner.RuntimeData.CooldownRemaining = 0f;
        }

        public bool TryConsumeCooldown()
        {
            _owner.RuntimeData.CooldownRemaining = Cooldown;
            return true;
        }
    }
}
