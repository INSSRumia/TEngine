using UnityEngine;
using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleMovementAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            Owner.RuntimeData.TargetDirection = Owner.RuntimeData.TargetDirectionManager.GetCombinedValue();
            var targetDir = Owner.RuntimeData.TargetDirection;
            if (targetDir.sqrMagnitude < 0.001f)
                return;

            var targetSpd = Owner.RuntimeData.TargetVelocityManager.GetCombinedValue();
            var curSpdAlongDir = Vector2.Dot(Owner.Rigidbody.velocity, targetDir);

            if (curSpdAlongDir >= targetSpd)
                return;

            var mass = Owner.Rigidbody.mass;
            var acc = Owner.RuntimeData.AccelerationManager.GetCombinedValue();
            var force = targetDir * (acc * mass);
            Owner.Rigidbody.AddForce(force, ForceMode2D.Force);
        }
    }
}
