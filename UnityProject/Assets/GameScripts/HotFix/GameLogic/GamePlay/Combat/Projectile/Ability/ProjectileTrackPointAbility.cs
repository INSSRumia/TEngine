using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileTrackPointAbility : ProjectileTrackingAbility
    {
        public ProjectileTrackPointAbility(ProjectileTrackPointConfig config) : base(config.AngularSpeed)
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
