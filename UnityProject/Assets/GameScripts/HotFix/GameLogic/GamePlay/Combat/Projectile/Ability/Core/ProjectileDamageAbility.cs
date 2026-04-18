using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileDamageAbility : ProjectileAbility, IProjectileHitHandler
    {
        public int MaxPiercingCount { get; private set; }
        public int SourceMarble { get; private set; }
        public bool IsDamageByVelocity { get; private set; }
        public float VelocityDamageFactor { get; private set; }
        public ProjectileDamageAbility(ProjectileDamageConfig config, int sourceMarble)
        {
            Priority = config?.Priority ?? 0;
            MaxPiercingCount = config?.PiercingCount ?? 0;
            SourceMarble = sourceMarble;
            IsDamageByVelocity = config != null && config.IsDamageByVelocity;
            VelocityDamageFactor = config?.VelocityDamageFactor ?? 0f;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Owner.RuntimeData.RemainPiercingCount = MaxPiercingCount;
        }

        public void HandleHit(ProjectileHitContext hitContext)
        {
            if (Owner?.RuntimeData == null || hitContext?.TargetReceiveDamage == null)
                return;

            int damage = Owner.RuntimeData.Damage;
            if (IsDamageByVelocity)
            {
                Vector2 velocity = Owner.Rigidbody != null ? Owner.Rigidbody.velocity : Vector2.zero;
                Vector2 targetVelocity = hitContext.TargetRigidbody != null ? hitContext.TargetRigidbody.velocity : Vector2.zero;
                float relativeVelocity = velocity.sqrMagnitude > 0.0001f
                    ? velocity.magnitude - Vector2.Dot(velocity.normalized, targetVelocity)
                    : 0f;
                damage = Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * damage);
            }

            hitContext.TargetReceiveDamage.ReceiveDamage(damage, null);
            Owner.RuntimeData.RemainPiercingCount--;
            if (Owner.RuntimeData.RemainPiercingCount < 0)
                ProjectileFactory.Recycle(Owner);
        }
    }
}
