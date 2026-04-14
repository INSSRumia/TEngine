using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponCalculateDamageAbility : EquipmentAbility
    {
        private WeaponEquipment _owner;
        public int? Attack {get; private set;}
        public WeaponCalculateDamageAbility(int? attack)
        {
            Attack = attack;
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
        private const float VELOCITY_DAMAGE_FACTOR = 1f;

        public int CalculateDamage()
        {
            if (_owner.RuntimeData.IsBroken)
                return 0;

            bool isUseOwnerAttack = Attack == null;
            int attack = isUseOwnerAttack ? _owner.OwnerMarble.RuntimeData.Attack : Attack.Value;
            // TODO: Marble应该提供一个计算伤害的核心Ability，计算伤害时应该使用这个Ability
            int attackAddition = _owner.OwnerMarble.RuntimeData.AttackAddition;
            float attackMultiplier = _owner.OwnerMarble.RuntimeData.AttackMultiplier;
            attack = Mathf.RoundToInt((attack + attackAddition) * attackMultiplier);

            return Mathf.Max(0, attack);
        }
    }
}
