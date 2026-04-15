using UnityEngine;

using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat
{
    public abstract class ProjectileTrackingAbility : ProjectileAbility, IAbilityFixedUpdate
    {
        public float RotateSpeed { get; private set; }
        protected ProjectileTrackingAbility(ProjectileTrackingConfig config, float rotateSpeed)
        {
            Priority = config.Priority;
            RotateSpeed = rotateSpeed;
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            var nextDirection = ResolveDirection(elapseSeconds);
            if (nextDirection.sqrMagnitude < 0.001f)
                return;

            Owner.RuntimeData.TargetDirection = nextDirection.normalized;
        }

        protected abstract Vector2 ResolveDirection(float elapseSeconds);

        protected Vector2 RotateTowards(Vector2 current, Vector2 target, float maxAngle)
        {
            if (current.sqrMagnitude < 0.0001f)
                return target.sqrMagnitude < 0.0001f ? Vector2.zero : target.normalized;

            if (target.sqrMagnitude < 0.0001f)
                return current.normalized;

            var currentAngle = Mathf.Atan2(current.y, current.x);
            var targetAngle = Mathf.Atan2(target.y, target.x);
            var angleDiff = Mathf.DeltaAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg);
            var clampedAngle = Mathf.Clamp(angleDiff, -maxAngle, maxAngle);
            var newAngle = currentAngle + clampedAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        }

        protected Vector2 GetCurrentDirection()
        {
            return Owner.transform.right;
        }
    }
}
