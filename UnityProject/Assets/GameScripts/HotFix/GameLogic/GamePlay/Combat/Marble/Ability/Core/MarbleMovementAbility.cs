using UnityEngine;
using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleMovementAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public MarbleMovementAbility(GameConfig.Gameplay.Combat.MarbleMovementAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            Owner.RuntimeData.Frame.TargetDirection = Owner.RuntimeData.Frame.TargetDirectionManager.GetCombinedValue();
            var targetDir = Owner.RuntimeData.Frame.TargetDirection;
            if (targetDir.sqrMagnitude < 0.001f)
                return;

            var targetSpd = Owner.RuntimeData.Frame.TargetVelocityManager.GetCombinedValue();
            var curSpdAlongDir = Vector2.Dot(Owner.Rigidbody.velocity, targetDir);

            if (curSpdAlongDir >= targetSpd)
                return;

            var mass = Owner.Rigidbody.mass;
            var acc = Owner.RuntimeData.Frame.AccelerationManager.GetCombinedValue();
            var force = targetDir * (acc * mass);
            Owner.Rigidbody.AddForce(force, ForceMode2D.Force);
        }
    }
}
