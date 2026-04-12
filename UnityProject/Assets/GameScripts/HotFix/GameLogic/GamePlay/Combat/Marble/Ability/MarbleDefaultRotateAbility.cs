using GameLogic.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleDefaultRotateAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public float TargetAngularSpeed { get; set; }
        public float AngularAcceleration { get; set; }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false)
                return;

            Owner.RuntimeData.TargetAngularVelocityManager.Add(new PriorityValue<float>(InstId, TargetAngularSpeed, Priority, CombineType));
            Owner.RuntimeData.AngularAccelerationManager.Add(new PriorityValue<float>(InstId, AngularAcceleration, Priority, CombineType));
        }
    }
}
