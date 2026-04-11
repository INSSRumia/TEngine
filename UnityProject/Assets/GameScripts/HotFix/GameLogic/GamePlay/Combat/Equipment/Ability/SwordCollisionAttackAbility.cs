using GameLogic.GamePlay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SwordCollisionAttackAbility : EquipmentAbility<SwordEquipment>
    {
        private const float VelocityDamageFactor = 1f;
        public void HandleCollision(Collision2D collision)
        {
            if (collision == null || EquipmentOwner == null || EquipmentOwner.OwnerMarble == null || EquipmentOwner.RuntimeData == null)
                return;

            var target = collision.collider.GetComponentInParent<ASC>();
            if(target == null)
                return;

            int targetCamp = -1;
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

            if (targetReceiveDamage == null)
                return;

            if (targetCamp == EquipmentOwner.OwnerMarble.RuntimeData.Camp)
                return;

            var cooldownAbility = EquipmentOwner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return;

            var relativeVelocity = collision.relativeVelocity.magnitude;
            var damage = EquipmentOwner.GetAbility<WeaponCalculateDamageAbility>()?.CalculateDamage() ?? 0;
            damage = Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * damage);

            if (damage <= 0)
                return;

            targetReceiveDamage?.ReceiveDamage(damage, EquipmentOwner);
        }
    }
}
