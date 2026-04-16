using GameConfig.Gameplay.Combat;
using GameLogic.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleKeepAwayFromTargetAbility : TimedMarbleAbility, IAbilityFixedUpdate
    {
        public float KeepAwayDistance { get; set; }
        public float TargetSpeed { get; set; }
        public float Acceleration { get; set; }

        public MarbleKeepAwayFromTargetAbility(MarbleKeepAwayFromTargetAbilityConfig config)
        {
            Priority = config.Priority;
            CombineType = (EnumCombineType)config.CombineType;
            KeepAwayDistance = config.KeepAwayDistance;
            TargetSpeed = config.TargetSpeed;
            Acceleration = config.Acceleration;
            InitializeTiming(AbilityTimingFactory.CreateTiming(config.Timing));
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.State.IsAlive == false || !IsActive)
                return;

            Marble target = Owner.CombatManager?.GetTarget(Owner.RuntimeData.State.TargetMarbleInstId);
            if (target == null)
                return;

            Vector2 offset = Owner.transform.position - target.transform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance > KeepAwayDistance * KeepAwayDistance || sqrDistance < 0.0001f)
                return;

            Vector2 direction = offset.normalized;
            Owner.RuntimeData.Frame.TargetDirectionManager.Add(new PriorityValue<Vector2>(InstId, direction, Priority, CombineType));
            Owner.RuntimeData.Frame.TargetVelocityManager.Add(new PriorityValue<float>(InstId, TargetSpeed, Priority, CombineType));
            Owner.RuntimeData.Frame.AccelerationManager.Add(new PriorityValue<float>(InstId, Acceleration, Priority, CombineType));
        }
    }
}
