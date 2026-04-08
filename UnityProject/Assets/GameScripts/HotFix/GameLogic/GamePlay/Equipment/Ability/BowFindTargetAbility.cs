using UnityEngine;

namespace GameLogic.Equipment
{
    public class BowFindTargetAbility : EquipmentAbility<BowRuntimeData>
    {
        public void SelectTarget(Vector2 aimDirection, int targetInstId = 0)
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null)
                return;

            EquipmentOwner.RuntimeData.TargetMarbleInstId = targetInstId;
            EquipmentOwner.RuntimeData.AimDirection = aimDirection;
        }
    }
}
