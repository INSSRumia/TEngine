using UnityEngine;

namespace GameLogic.Marble
{
    public class MoveAbility : MarbleAbility
    {
        public override void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (!IsOwnerValid() || Owner.RuntimeData.Rigidbody == null)
                return;
            if (!Owner.RuntimeData.IsAlive)
                return;
            
            var targetSpd = Owner.RuntimeData.TargetSpeed;
            var curSpd = Owner.RuntimeData.Rigidbody.velocity.magnitude;
            if (curSpd > targetSpd) return;
            
            var targetDir = Owner.RuntimeData.TargetDirection;
            if (targetDir.sqrMagnitude < 0.01f) return;
            
            var mass = Owner.RuntimeData.Rigidbody.mass;
            var acc = Owner.RuntimeData.Acceleration;
            var force = targetDir * acc * mass;
            
            Owner.RuntimeData.Rigidbody.AddForce(force, ForceMode2D.Force);
        }
    }
}
