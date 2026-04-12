using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public abstract class WeaponFireAbility : EquipmentAbility, IAbilityUpdate
    {
        protected WeaponEquipment _owner;
        protected float _fireInterval;
        protected float _fireCountdown;

        public override void OnAdd()
        {
            base.OnAdd();
            if (EquipmentOwner is WeaponEquipment weaponEquipment)
                _owner = weaponEquipment;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }

        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_owner == null || _owner.RuntimeData == null)
                return;

            if (_owner.RuntimeData.CooldownRemaining > 0f)
                _owner.RuntimeData.CooldownRemaining -= elapseSeconds;

            if (_fireInterval > 0f && _fireCountdown > 0f)
            {
                _fireCountdown -= elapseSeconds;
                if (_fireCountdown < 0f)
                    _fireCountdown = 0f;
            }

            if (_owner.RuntimeData.CooldownRemaining <= 0f && _fireCountdown <= 0f && CanFire())
            {
                DoFire();
                _fireCountdown = _fireInterval;
            }
        }

        protected abstract bool CanFire();
        protected abstract void DoFire();
    }
}
