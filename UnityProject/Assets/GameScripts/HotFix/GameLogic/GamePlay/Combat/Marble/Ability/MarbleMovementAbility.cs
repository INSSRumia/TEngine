using UnityEngine;
using GameLogic.GamePlay.Combat;
using TEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleMovementAbility : Ability<Marble>, IAbilityFixedUpdate
    {
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            var targetDir = Owner.RuntimeData.TargetDirection;
            if (targetDir.sqrMagnitude < 0.001f)
                return;

            targetDir.Normalize();

            var targetSpd = Owner.RuntimeData.TargetVelocity;
            var curSpdAlongDir = Vector2.Dot(Owner.Rigidbody.velocity, targetDir);

            if (curSpdAlongDir >= targetSpd)
                return;

            var mass = Owner.Rigidbody.mass;
            var acc = Owner.RuntimeData.Acceleration;
            var force = targetDir * acc * mass;
            Owner.Rigidbody.AddForce(force, ForceMode2D.Force);
        }
    }
}
