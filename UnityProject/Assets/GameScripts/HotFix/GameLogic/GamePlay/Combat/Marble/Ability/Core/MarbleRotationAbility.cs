using UnityEngine;
using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleRotationAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public MarbleRotationAbility(GameConfig.Gameplay.Combat.MarbleRotationAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;
            
            var targetAngSpd = Owner.RuntimeData.Frame.TargetAngularVelocityManager.GetCombinedValue();
            var curAngSpd = Owner.Rigidbody.angularVelocity;

            // 达到目标转速则停止施加扭矩
            if (curAngSpd * targetAngSpd > 0 && Mathf.Abs(curAngSpd) >= Mathf.Abs(targetAngSpd))
                return;

            var sign = Mathf.Sign(targetAngSpd);
            var angAcc = Owner.RuntimeData.Frame.AngularAccelerationManager.GetCombinedValue();
            var torque = sign * angAcc * Owner.Rigidbody.inertia;
            Owner.Rigidbody.AddTorque(torque, ForceMode2D.Force);
        }
    }
}
