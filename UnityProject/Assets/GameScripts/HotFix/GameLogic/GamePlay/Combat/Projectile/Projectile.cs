using TEngine;
using UnityEngine;

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

        public void Despawn()
        {
            if (this == null || gameObject == null)
                return;
            RemoveAllAbilities();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (RuntimeData == null)
                return;

            GetAbility<ProjectileDamageAbility>()?.HandleCollider(other);
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
