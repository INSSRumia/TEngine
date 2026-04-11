using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponCooldownAbility : EquipmentAbility, IAbilityUpdate
    {
        private WeaponEquipment _owner;
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
            if (_owner == null || _owner.RuntimeData == null)
                return;

            if (_owner.RuntimeData.CooldownRemaining <= 0f)
                return;

            _owner.RuntimeData.CooldownRemaining -= elapseSeconds;
            if (_owner.RuntimeData.CooldownRemaining < 0f)
                _owner.RuntimeData.CooldownRemaining = 0f;
        }

        public bool TryConsumeCooldown()
        {
            if (_owner == null || _owner.RuntimeData == null)
                return false;

            if (_owner.RuntimeData.CooldownRemaining > 0f)
                return false;

            _owner.RuntimeData.CooldownRemaining = _owner.RuntimeData.Cooldown;
            return true;
        }
    }
}
