using GameLogic.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleCloseToTargetAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public float CloseDistance { get; set; } = 25f;
        public EnumCombineType CombineType { get; set; } = EnumCombineType.Combine;

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false)
                return;

            Marble target = Owner.CombatManager?.GetTarget(Owner.RuntimeData.TargetMarbleInstId);
            if(target == null)
                return;

            float sqrDistance = (Owner.transform.position - target.transform.position).sqrMagnitude;

            if(sqrDistance <= CloseDistance * CloseDistance)
            {
                return;
            }

            Vector2 direction = (target.transform.position - Owner.transform.position).normalized;
            Owner.RuntimeData.TargetDirectionManager.Add(new PriorityValue<Vector2>(InstId, direction, Priority, CombineType));
        }
    }
}
