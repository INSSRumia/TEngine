using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public sealed class ProjectileHitContext : MemoryObject
    {
        public Collider2D Collider { get; private set; }
        public ASC Target { get; private set; }
        public Rigidbody2D TargetRigidbody { get; private set; }
        public IReceiveDamage TargetReceiveDamage { get; private set; }
        public int TargetMarbleInstId { get; private set; }

        public void Reset(
            Collider2D collider,
            ASC target,
            Rigidbody2D targetRigidbody,
            IReceiveDamage targetReceiveDamage,
            int targetMarbleInstId)
        {
            Collider = collider;
            Target = target;
            TargetRigidbody = targetRigidbody;
            TargetReceiveDamage = targetReceiveDamage;
            TargetMarbleInstId = targetMarbleInstId;
        }

        public override void Clear()
        {
            Collider = null;
            Target = null;
            TargetRigidbody = null;
            TargetReceiveDamage = null;
            TargetMarbleInstId = 0;
        }

        public override void InitFromPool()
        {
            Clear();
        }

        public override void RecycleToPool()
        {
            Clear();
        }
    }
}
