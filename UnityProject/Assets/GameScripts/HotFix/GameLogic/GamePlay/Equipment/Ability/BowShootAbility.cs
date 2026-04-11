using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.Equipment
{
    public class BowShootAbility : EquipmentAbility<BowEquipment>
    {
        public bool TryBuildShot(out IReadOnlyList<Vector2> shotDirections)
        {
            shotDirections = null;
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null || !EquipmentOwner.RuntimeData.CanFire)
                return false;

            var cooldownAbility = EquipmentOwner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return false;

            var result = new List<Vector2>();
            var forward = (Vector2)EquipmentOwner.transform.right;
            var count = Mathf.Max(1, EquipmentOwner.RuntimeData.ArrowCount);

            if (EquipmentOwner.RuntimeData.ShootType == 1)
            {
                var centerIndex = 0;
                for (var i = 0; i < count; i++)
                {
                    var offsetIndex = i == 0 ? 0 : (i % 2 == 1 ? centerIndex + 1 : -(centerIndex + 1));
                    if (i % 2 == 0 && i > 0)
                        centerIndex++;
                    var angle = offsetIndex * EquipmentOwner.RuntimeData.ArrowAngleStep;
                    result.Add(Quaternion.Euler(0f, 0f, angle) * forward);
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    result.Add(forward);
                }
            }

            shotDirections = result;
            return true;
        }
    }
}
