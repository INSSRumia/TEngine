using UnityEngine;
using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleRotationAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public float TargetAngularSpeed { get; set; } = 360f;

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            Owner.RuntimeData.TargetAngularVelocityManager.Add(new PriorityValue<float>(
                InstId, TargetAngularSpeed, Priority, CombineType));

            var targetAngSpd = Owner.RuntimeData.TargetAngularVelocityManager.GetCombinedValue();
            var curAngSpd = Owner.Rigidbody.angularVelocity;

            // 方向对齐：当前旋转方向与目标方向相反时，取反目标速度
            if (targetAngSpd * curAngSpd < 0)
                targetAngSpd = -targetAngSpd;

            // 达到目标转速则停止施加扭矩
            if (Mathf.Abs(curAngSpd) >= Mathf.Abs(targetAngSpd))
                return;

            var sign = targetAngSpd > 0 ? 1 : -1;
            var angAcc = Owner.RuntimeData.AngularAccelerationManager.GetCombinedValue();
            var torque = sign * angAcc * Owner.Rigidbody.inertia;
            Owner.Rigidbody.AddTorque(torque, ForceMode2D.Force);
        }
    }
}
