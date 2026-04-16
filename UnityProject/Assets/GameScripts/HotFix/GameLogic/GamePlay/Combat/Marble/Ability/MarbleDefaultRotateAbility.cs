using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleDefaultRotateAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public float TargetAngularSpeed { get; set; }
        public float AngularAcceleration { get; set; }

        public MarbleDefaultRotateAbility(MarbleDefaultRotateAbilityConfig config)
        {
            Priority = config.Priority;
            CombineType = (EnumCombineType)config.CombineType;
            TargetAngularSpeed = config.TargetAngularSpeed;
            AngularAcceleration = config.AngularAcceleration;
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.State.IsAlive == false)
                return;
            // 方向对齐：当前旋转方向与目标方向相反时，取反目标速度
            var currentAngSpd = Owner.Rigidbody.angularVelocity;
            var targetAngSpd = TargetAngularSpeed;
            if (targetAngSpd * currentAngSpd < 0)
                targetAngSpd = -targetAngSpd;

            Owner.RuntimeData.Frame.TargetAngularVelocityManager.Add(new PriorityValue<float>(InstId, targetAngSpd, Priority, CombineType));
            Owner.RuntimeData.Frame.AngularAccelerationManager.Add(new PriorityValue<float>(InstId, AngularAcceleration, Priority, CombineType));
        }
    }
}
