using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleDashAbility : TimedMarbleAbility, IAbilityFixedUpdate
    {
        public float TargetSpeed { get; set; }
        public float Acceleration { get; set; }
        public bool LockDirectionOnActivate { get; set; } = true;

        private Vector2 _lockedDirection;

        public MarbleDashAbility(MarbleDashAbilityConfig config)
        {
            Priority = config.Priority;
            CombineType = (EnumCombineType)config.CombineType;
            TargetSpeed = config.TargetSpeed;
            Acceleration = config.Acceleration;
            LockDirectionOnActivate = config.LockDirectionOnActivate;
            InitializeTiming(AbilityTimingFactory.CreateTiming(config.Timing));
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false || !IsActive)
                return;

            Vector2 direction = ResolveDashDirection();
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Owner.RuntimeData.TargetDirectionManager.Add(new PriorityValue<Vector2>(InstId, direction, Priority, CombineType));
            Owner.RuntimeData.TargetVelocityManager.Add(new PriorityValue<float>(InstId, TargetSpeed, Priority, CombineType));
            Owner.RuntimeData.AccelerationManager.Add(new PriorityValue<float>(InstId, Acceleration, Priority, CombineType));
        }

        protected override void OnTimingActivated()
        {
            base.OnTimingActivated();
            if (LockDirectionOnActivate)
            {
                _lockedDirection = ResolveTargetDirection();
            }
        }

        public bool TryDash()
        {
            return TryActivateTiming();
        }

        private Vector2 ResolveDashDirection()
        {
            if (LockDirectionOnActivate && _lockedDirection.sqrMagnitude > 0.0001f)
                return _lockedDirection;

            return ResolveTargetDirection();
        }

        private Vector2 ResolveTargetDirection()
        {
            if (Owner == null)
                return default;

            Marble target = Owner.CombatManager?.GetTarget(Owner.RuntimeData.TargetMarbleInstId);
            if (target != null)
            {
                Vector2 dirToTarget = target.transform.position - Owner.transform.position;
                if (dirToTarget.sqrMagnitude > 0.0001f)
                    return dirToTarget.normalized;
            }

            Vector2 currentVelocity = Owner.Rigidbody != null ? Owner.Rigidbody.velocity : default;
            if (currentVelocity.sqrMagnitude > 0.0001f)
                return currentVelocity.normalized;

            Vector2 currentTargetDirection = Owner.RuntimeData.TargetDirection;
            if (currentTargetDirection.sqrMagnitude > 0.0001f)
                return currentTargetDirection.normalized;

            return default;
        }
    }
}
