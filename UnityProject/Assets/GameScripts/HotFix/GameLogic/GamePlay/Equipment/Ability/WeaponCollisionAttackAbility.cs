using GameLogic.Marble;
using UnityEngine;

namespace GameLogic.Equipment
{
    public class WeaponCollisionAttackAbility : EquipmentAbility<WeaponRuntimeData>
    {
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
            var damage = EquipmentOwner.GetAbility<WeaponCollisionDamageAbility>()?.CalculateDamage(relativeVelocity) ?? 0;
            if (damage <= 0)
                return;

            targetMarble.GetAbility<MarbleAddDamageAbility>()?.AddDamage(damage);
        }
    }
}
