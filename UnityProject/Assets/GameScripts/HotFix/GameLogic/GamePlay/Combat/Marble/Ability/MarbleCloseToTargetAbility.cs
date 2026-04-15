using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleCloseToTargetAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public float CloseDistance { get; set; }
        public float TargetSpeed { get; set; }
        public float Acceleration { get; set; }

        public MarbleCloseToTargetAbility(MarbleCloseToTargetAbilityConfig config)
        {
            Priority = config.Priority;
            CombineType = (EnumCombineType)config.CombineType;
            CloseDistance = config.CloseDistance;
            TargetSpeed = config.TargetSpeed;
            Acceleration = config.Acceleration;
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false)
                return;

            Marble target = Owner.CombatManager?.GetTarget(Owner.RuntimeData.TargetMarbleInstId);
            if(target == null)
                return;

            float sqrDistance = (Owner.transform.position - target.transform.position).sqrMagnitude;

            if(sqrDistance <= CloseDistance * CloseDistance)
            {
                return;
            }

            Vector2 direction = (target.transform.position - Owner.transform.position).normalized;
            Owner.RuntimeData.TargetDirectionManager.Add(new PriorityValue<Vector2>(InstId, direction, Priority, CombineType));
            Owner.RuntimeData.TargetVelocityManager.Add(new PriorityValue<float>(InstId, TargetSpeed, Priority, CombineType));
            Owner.RuntimeData.AccelerationManager.Add(new PriorityValue<float>(InstId, Acceleration, Priority, CombineType));
        }
    }
}
