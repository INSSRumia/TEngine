using UnityEngine;
using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowAimAbility : EquipmentAbility<BowEquipment>, IAbilityUpdate
    {
        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null)
                return;

            var targetMarble = EquipmentOwner.OwnerMarble.CombatManager?.GetTarget(EquipmentOwner.RuntimeData.TargetMarbleInstId);
            if (targetMarble == null)
                return;

            var aimDirection = (targetMarble.transform.position - EquipmentOwner.transform.position).normalized;
            if (Mathf.Approximately(aimDirection.sqrMagnitude, 0f))
                return;

            var targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            var currentAngle = EquipmentOwner.transform.eulerAngles.z;
            var nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, EquipmentOwner.RuntimeData.RotateSpeed * elapseSeconds);
            EquipmentOwner.transform.rotation = Quaternion.Euler(0f, 0f, nextAngle);

            var currentDir = EquipmentOwner.transform.right;
            var angleDelta = Vector2.Angle(currentDir, aimDirection.normalized);
            EquipmentOwner.RuntimeData.CanFire = angleDelta <= EquipmentOwner.RuntimeData.AimAngle;
        }
    }
}
