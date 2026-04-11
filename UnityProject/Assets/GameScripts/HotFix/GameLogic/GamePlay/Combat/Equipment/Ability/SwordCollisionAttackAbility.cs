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

            var targetMarble = collision.collider.GetComponent<Marble.Marble>();
            if (targetMarble == null || targetMarble.RuntimeData == null)
                return;

            if (!targetMarble.RuntimeData.IsAlive)
                return;

            if (targetMarble.RuntimeData.Camp == EquipmentOwner.OwnerMarble.RuntimeData.Camp)
                return;

            var cooldownAbility = EquipmentOwner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return;

            var relativeVelocity = collision.relativeVelocity.magnitude;
            var damage = EquipmentOwner.GetAbility<WeaponCalculateDamageAbility>()?.CalculateDamage() ?? 0;
            damage = Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * damage);

            if (damage <= 0)
                return;

            targetMarble.GetAbility<IReceiveDamage>()?.ReceiveDamage(damage, EquipmentOwner);
        }
    }
}
