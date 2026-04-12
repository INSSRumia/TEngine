using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowAimAbility : EquipmentAbility, IAbilityUpdate
    {
        private BowEquipment _owner;
        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is BowEquipment bowEquipment)
                _owner = bowEquipment;
        }
        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }

        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_owner == null || _owner.RuntimeData == null)
                return;

            var targetMarble = _owner.OwnerMarble.CombatManager?.GetTarget(_owner.RuntimeData.TargetMarbleInstId);
            if (targetMarble == null)
                return;

            var aimDirection = (targetMarble.transform.position - _owner.transform.position).normalized;
            if (Mathf.Approximately(aimDirection.sqrMagnitude, 0f))
                return;

            var targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            var currentAngle = _owner.transform.eulerAngles.z;
            var nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _owner.RuntimeData.RotateSpeed * elapseSeconds);
            EquipmentOwner.transform.rotation = Quaternion.Euler(0f, 0f, nextAngle);

            var currentDir = _owner.transform.right;
            var angleDelta = Vector2.Angle(currentDir, aimDirection.normalized);
            _owner.RuntimeData.CanFire = angleDelta <= _owner.RuntimeData.AimAngle;
        }
    }
}
