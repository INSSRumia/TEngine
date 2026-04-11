using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponCalculateDamageAbility : EquipmentAbility
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
        private const float VELOCITY_DAMAGE_FACTOR = 1f;

        public int CalculateDamage()
        {
            if (_owner == null || _owner.RuntimeData == null || _owner.RuntimeData.IsBroken)
                return 0;

            bool isUseOwnerAttack = _owner.RuntimeData.Attack == null;
            int attack = isUseOwnerAttack ? _owner.OwnerMarble.RuntimeData.Attack : _owner.RuntimeData.Attack.Value;
            int attackAddition = _owner.OwnerMarble.RuntimeData.AttackAddition;
            float attackMultiplier = _owner.OwnerMarble.RuntimeData.AttackMultiplier;
            attack = Mathf.RoundToInt((attack + attackAddition) * attackMultiplier);

            return Mathf.Max(0, Mathf.RoundToInt(attack));
        }
    }
}
