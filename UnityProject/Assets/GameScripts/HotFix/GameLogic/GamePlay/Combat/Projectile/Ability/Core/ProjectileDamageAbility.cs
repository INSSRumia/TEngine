using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileDamageAbility : ProjectileAbility
    {
        public int MaxPiercingCount { get; private set; }
        public int SourceMarble { get; private set; }
        public bool IsDamageByVelocity { get; private set; }
        public float VelocityDamageFactor { get; private set; }
        public ProjectileDamageAbility(ProjectileDamageConfig config, int sourceMarble)
        {
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

        public void HandleCollider(Collider2D other)
        {
            var target = other.GetComponentInParent<ASC>();
            if(target == null)
                return;

            int targetCamp = Owner.RuntimeData.SourceCamp;
            IReceiveDamage targetReceiveDamage = null;
            int targetMarbleInstId = -1;
            Rigidbody2D targetRigidbody = null;
            switch(target)
            {
                case Marble.Marble marble:
                    targetCamp = marble.RuntimeData.Camp;
                    targetReceiveDamage = marble.GetAbility<IReceiveDamage>();
                    targetMarbleInstId = marble.RuntimeData.InstId;
                    targetRigidbody = marble.Rigidbody;
                    break;
                case Equipment.Equipment equipment:
                    targetCamp = equipment.OwnerMarble.RuntimeData.Camp;
                    targetReceiveDamage = equipment.GetAbility<IReceiveDamage>();
                    targetMarbleInstId = equipment.OwnerMarble.RuntimeData.InstId;
                    targetRigidbody = equipment.Rigidbody;
                    break;
                default:
                    return;
            }

            if(targetCamp == Owner.RuntimeData.SourceCamp)
                return;

            if(targetReceiveDamage == null)
                return;

            int damage = Owner.RuntimeData.Damage;
            if(IsDamageByVelocity)
            {
                Vector2 velocity = Owner.Rigidbody.velocity;
                float relativeVelocity = velocity.magnitude - Vector2.Dot(velocity.normalized, targetRigidbody.velocity);
                damage = Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * damage);
            }

            // // 发射物动量
            // Vector2 p = Owner.Rigidbody.mass * Owner.Rigidbody.velocity;

            // // 将发射物的动量完全传递给目标
            // targetRigidbody.AddForce(p, ForceMode2D.Impulse);

            targetReceiveDamage.ReceiveDamage(damage, null);
            Owner.RuntimeData.TryMarkHit(targetMarbleInstId);
            
            Owner.RuntimeData.RemainPiercingCount--;
            if (Owner.RuntimeData.RemainPiercingCount < 0)
                ProjectileFactory.Recycle(Owner);
        }
    }
}
