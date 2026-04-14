using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileTrackTargetAbility : ProjectileTrackingAbility
    {
        public ProjectileTrackTargetAbility(float rotateSpeed) : base(rotateSpeed)
        {
        }

        protected override Vector2 ResolveDirection(float elapseSeconds)
        {
            var currentDirection = GetCurrentDirection();
            var target = Owner.CombatManager.GetTarget(Owner.RuntimeData.TargetMarbleInstId);
            if (target == null || target.RuntimeData == null || !target.RuntimeData.IsAlive)
                return currentDirection;

            var targetDirection = ((Vector2)(target.transform.position - Owner.transform.position)).normalized;
            return RotateTowards(currentDirection, targetDirection, RotateSpeed * elapseSeconds);
        }
    }
}
