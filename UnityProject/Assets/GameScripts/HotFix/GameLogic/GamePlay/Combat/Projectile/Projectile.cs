using TEngine;
using UnityEngine;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat
{
    public class Projectile : ASC<ProjectileRuntimeData>
    {
        private ICombatManager _combatManager;

        protected override void Awake()
        {
            base.Awake();
            _combatManager = GameLogic.Gameplay.Combat.CombatManager.Instance;
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (RuntimeData.IsFinishedLifetime)
            {
                ProjectileFactory.Recycle(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (RuntimeData == null)
                return;

            if (!TryCreateHitContext(other, out var hitContext))
                return;

            if (!RuntimeData.TryMarkHit(hitContext.TargetMarbleInstId))
            {
                MemoryPool.Dealloc(hitContext);
                return;
            }

            try
            {
                DispatchHitHandlers(hitContext);
            }
            finally
            {
                MemoryPool.Dealloc(hitContext);
            }
        }

        private bool TryCreateHitContext(Collider2D other, out ProjectileHitContext context)
        {
            context = null;
            if (other == null)
                return false;

            var target = other.GetComponentInParent<ASC>();
            if (target == null)
                return false;

            int targetCombatSide = RuntimeData.SourceCombatSide;
            IReceiveDamage targetReceiveDamage = null;
            int targetMarbleInstId = -1;
            Rigidbody2D targetRigidbody = null;
            switch (target)
            {
                case Marble.Marble marble:
                    targetCombatSide = marble.RuntimeData.CombatSide;
                    targetReceiveDamage = marble.GetAbility<IReceiveDamage>();
                    targetMarbleInstId = marble.RuntimeData.InstId;
                    targetRigidbody = marble.Rigidbody;
                    break;
                case Equipment.Equipment equipment:
                    targetCombatSide = equipment.OwnerMarble.RuntimeData.CombatSide;
                    targetReceiveDamage = equipment.GetAbility<IReceiveDamage>();
                    targetMarbleInstId = equipment.OwnerMarble.RuntimeData.InstId;
                    targetRigidbody = equipment.Rigidbody;
                    break;
                default:
                    return false;
            }

            if (targetCombatSide == RuntimeData.SourceCombatSide || targetReceiveDamage == null)
                return false;

            context = MemoryPool.Alloc<ProjectileHitContext>();
            context.Reset(other, target, targetRigidbody, targetReceiveDamage, targetMarbleInstId);
            return true;
        }

        private void DispatchHitHandlers(ProjectileHitContext hitContext)
        {
            var hitHandlers = ListPool<IProjectileHitHandler>.Get();
            try
            {
                GetAbilities(ref hitHandlers);
                foreach (var hitHandler in hitHandlers)
                {
                    hitHandler.HandleHit(hitContext);
                }
            }
            finally
            {
                ListPool<IProjectileHitHandler>.Release(hitHandlers);
            }
        }

        private void OnDrawGizmos()
        {
            if (RuntimeData == null)
                return;

            Gizmos.color = Color.red;
            var pos = transform.position;
            Gizmos.DrawLine(pos, pos + (Vector3)Rigidbody.velocity.normalized * 2f);
            Gizmos.DrawWireSphere(pos, 0.2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, 0.5f);

            Gizmos.color = Color.green;
            if (GetAbility<ProjectileTrackTargetAbility>() != null)
            {
                var target = _combatManager?.GetTarget(RuntimeData.TargetMarbleInstId);
                if (target != null)
                    Gizmos.DrawLine(pos, target.transform.position);
            }
            else if (GetAbility<ProjectileTrackPointAbility>() != null)
            {
                Gizmos.DrawWireSphere(RuntimeData.TargetPoint, 0.3f);
            }
        }
    }
}
