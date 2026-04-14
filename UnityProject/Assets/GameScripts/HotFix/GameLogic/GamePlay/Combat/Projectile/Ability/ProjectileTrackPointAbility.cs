using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileTrackPointAbility : ProjectileTrackingAbility
    {
        public ProjectileTrackPointAbility(float rotateSpeed) : base(rotateSpeed)
        {
        }

        protected override Vector2 ResolveDirection(float elapseSeconds)
        {
            var currentDirection = GetCurrentDirection();

            var targetDirection = (Owner.RuntimeData.TargetPoint - (Vector2)Owner.transform.position).normalized;
            return RotateTowards(currentDirection, targetDirection, RotateSpeed * elapseSeconds);
        }
    }
}
