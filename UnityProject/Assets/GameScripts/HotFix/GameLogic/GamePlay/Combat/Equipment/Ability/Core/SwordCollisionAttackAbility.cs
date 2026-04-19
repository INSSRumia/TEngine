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

            int targetCamp = _owner.OwnerMarble.RuntimeData.Camp;
            IReceiveDamage targetReceiveDamage = null;
            switch(target)
            {
                case Marble.Marble marble:
                    targetCamp = marble.RuntimeData.Camp;
                    targetReceiveDamage = marble.GetAbility<IReceiveDamage>();
                    break;

                case Equipment equipment:
                    targetCamp = equipment.OwnerMarble.RuntimeData.Camp;
                    targetReceiveDamage = equipment.GetAbility<IReceiveDamage>();
                    break;
                default:
                    return;
            }

            if (targetCamp == _owner.OwnerMarble.RuntimeData.Camp)
                return;

            if (targetReceiveDamage == null)
                return;

            var cooldownAbility = _owner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return;

            var damage = _owner.GetAbility<WeaponCalculateDamageAbility>()?.CalculateDamage() ?? 0;
            //TODO: 这里需要优化，如果装备是按速度伤害，则需要计算速度伤害,否则直接使用伤害
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
