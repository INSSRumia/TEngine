using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleCloseToTargetAbility : Ability<Marble>, IAbilityFixedUpdate
    {
        public float SquaredCloseDistance { get; set; } = 25f; // 距离目标小于这个值时，认为已经接近目标
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false)
                return;

            Marble target = Owner.CombatManager?.GetTarget(Owner.RuntimeData.TargetMarbleInstId);
            if(target == null)
                return;
            
            float sqrDistance = (Owner.transform.position - target.transform.position).sqrMagnitude;

            if(sqrDistance <= SquaredCloseDistance)
            {
                Owner.RuntimeData.TargetDirection = default;
                return;
            }

            Owner.RuntimeData.TargetDirection = (target.transform.position - Owner.transform.position).normalized;
        }

    }
}
