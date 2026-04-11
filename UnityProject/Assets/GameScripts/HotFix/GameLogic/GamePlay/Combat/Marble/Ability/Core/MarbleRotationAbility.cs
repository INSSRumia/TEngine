using UnityEngine;
using GameLogic.GamePlay.Combat;
using TEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleRotationAbility : Ability<Marble>, IAbilityFixedUpdate
    {
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            Owner.RuntimeData.TargetAngularVelocity = 360;
            var targetAngSpd = Owner.RuntimeData.TargetAngularVelocity;
            var curAngSpd = Owner.Rigidbody.angularVelocity;

            // 方向对齐：当前旋转方向与目标方向相反时，取反目标速度
            if (targetAngSpd * curAngSpd < 0)
                targetAngSpd = -targetAngSpd;

            // 达到目标转速则停止施加扭矩
            if (Mathf.Abs(curAngSpd) >= Mathf.Abs(targetAngSpd)){
                // Log.Info($"[MarbleRotationAbility] 达到目标转速: {targetAngSpd}, 当前转速: {curAngSpd}");
                return;
            }

            var sign = targetAngSpd > 0 ? 1 : -1;
            var angAcc = Owner.RuntimeData.AngularAcceleration;
            var torque = sign * angAcc * Owner.Rigidbody.inertia;
            Owner.Rigidbody.AddTorque(torque, ForceMode2D.Force);
        }
    }
}
