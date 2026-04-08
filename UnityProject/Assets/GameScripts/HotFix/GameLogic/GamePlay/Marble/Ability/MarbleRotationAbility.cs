using UnityEngine;

namespace GameLogic.Marble
{
    public class MarbleRotationAbility : Ability<IRotation>
    {
        public override AbilityExecutionMode ExecutionMode => AbilityExecutionMode.FixedUpdate;

        public override void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            var targetAngSpd = Owner.RuntimeData.TargetAngularVelocity;
            var curAngSpd = Owner.Rigidbody.angularVelocity;

            if (targetAngSpd * curAngSpd < 0)
                targetAngSpd = -targetAngSpd;

            if (curAngSpd > targetAngSpd)
                return;

            var sign = targetAngSpd > 0 ? 1 : -1;
            var angAcc = Owner.RuntimeData.AngularAcceleration;
            var torque = sign * angAcc * Owner.Rigidbody.inertia;
            Owner.Rigidbody.AddTorque(torque, ForceMode2D.Force);
        }
    }
}
