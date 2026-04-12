using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SwordCollisionAttackAbility : EquipmentAbility
    {
        private SwordEquipment _owner;
        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is SwordEquipment swordEquipment)
                _owner = swordEquipment;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }

        public float VelocityDamageFactor { get; set; } = 1f;
        public void HandleCollision(Collision2D collision)
        {
            if (collision == null || _owner == null || _owner.OwnerMarble == null || _owner.RuntimeData == null)
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
            var relativeVelocity = collision.relativeVelocity.magnitude;
            damage = Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * damage);

            if (damage <= 0)
                return;

            targetReceiveDamage?.ReceiveDamage(damage, _owner);
        }
    }
}
