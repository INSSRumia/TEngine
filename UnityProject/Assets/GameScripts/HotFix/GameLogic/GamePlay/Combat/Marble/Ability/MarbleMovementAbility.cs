using UnityEngine;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleMovementAbility : Ability<Marble>, IAbilityFixedUpdate
    {
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            var targetSpd = Owner.RuntimeData.TargetVelocity;
            var curSpd = Owner.Rigidbody.velocity.magnitude;
            if (curSpd > targetSpd)
                return;

            var targetDir = Owner.RuntimeData.TargetDirection;
            if (Mathf.Approximately(targetDir.sqrMagnitude, 0f))
                return;

            var mass = Owner.Rigidbody.mass;
            var acc = Owner.RuntimeData.Acceleration;
            var force = targetDir * acc * mass;
            Owner.Rigidbody.AddForce(force, ForceMode2D.Force);
        }
    }
}
