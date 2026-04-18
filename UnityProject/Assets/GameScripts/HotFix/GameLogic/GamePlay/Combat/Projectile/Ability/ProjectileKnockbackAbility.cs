using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileKnockbackAbility : ProjectileAbility, IProjectileHitHandler
    {
        private readonly float _force;

        public ProjectileKnockbackAbility(ProjectileKnockbackConfig config)
        {
            Priority = config?.Priority ?? 0;
            _force = config?.Force ?? 0f;
        }

        public void HandleHit(ProjectileHitContext context)
        {
            if (Owner == null || context?.TargetRigidbody == null || _force <= 0f)
                return;

            var direction = ResolveDirection(context);
            if (direction.sqrMagnitude < 0.0001f)
                return;

            context.TargetRigidbody.AddForce(direction * _force, ForceMode2D.Impulse);
        }

        private Vector2 ResolveDirection(ProjectileHitContext context)
        {
            if (Owner.Rigidbody != null)
            {
                var velocity = Owner.Rigidbody.velocity;
                if (velocity.sqrMagnitude > 0.0001f)
                    return velocity.normalized;
            }

            if (context.Target != null)
            {
                var fallbackDirection = (Vector2)(context.Target.transform.position - Owner.transform.position);
                if (fallbackDirection.sqrMagnitude > 0.0001f)
                    return fallbackDirection.normalized;
            }

            return Vector2.zero;
        }
    }
}
