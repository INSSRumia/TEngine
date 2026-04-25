using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SwordCollisionAttackAbility : EquipmentAbility
    {
        private WeaponEquipment _owner;
        public bool IsDamageByVelocity {get; private set;}
        public float VelocityDamageFactor {get; private set;}
        public SwordCollisionAttackAbility(SwordCollisionAttackAbilityConfig config)
        {
            IsDamageByVelocity = config.IsDamageByVelocity;
            VelocityDamageFactor = config.VelocityDamageFactor;
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

        public void HandleCollision(Collision2D collision)
        {
            if (collision == null || _owner == null || _owner.OwnerMarble == null || _owner.RuntimeData == null)
                return;

            if (!_owner.CanDealDamageFromCollider(collision.otherCollider))
                return;

            var target = collision.collider.GetComponentInParent<ASC>();
            if(target == null)
                return;

            int targetCombatSide = _owner.OwnerMarble.RuntimeData.CombatSide;
            IReceiveDamage targetReceiveDamage = null;
            switch(target)
            {
                case Marble.Marble marble:
                    targetCombatSide = marble.RuntimeData.CombatSide;
                    targetReceiveDamage = marble.GetAbility<IReceiveDamage>();
                    break;

                case Equipment equipment:
                    targetCombatSide = equipment.OwnerMarble.RuntimeData.CombatSide;
                    targetReceiveDamage = equipment.GetAbility<IReceiveDamage>();
                    break;
                default:
                    return;
            }

            if (targetCombatSide == _owner.OwnerMarble.RuntimeData.CombatSide)
                return;

            if (targetReceiveDamage == null)
                return;

            var cooldownAbility = _owner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return;

            var damage = _owner.GetAbility<WeaponCalculateDamageAbility>()?.CalculateDamage() ?? 0;
            if(IsDamageByVelocity)
            {
                var relativeVelocity = collision.relativeVelocity.magnitude;
                damage = Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * damage);
            }

            if (damage <= 0)
                return;

            targetReceiveDamage?.ReceiveDamage(damage, _owner);
        }
    }
}
