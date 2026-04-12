using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileDamageAbility : ProjectileAbility
    {
        public int MaxPiercingCount { get; set; }
        public int SourceMarble { get; set; }

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
            switch(target)
            {
                case Marble.Marble marble:
                    targetCamp = marble.RuntimeData.Camp;
                    targetReceiveDamage = marble.GetAbility<IReceiveDamage>();
                    targetMarbleInstId = marble.RuntimeData.InstId;
                    break;
                case Equipment.Equipment equipment:
                    targetCamp = equipment.OwnerMarble.RuntimeData.Camp;
                    targetReceiveDamage = equipment.GetAbility<IReceiveDamage>();
                    targetMarbleInstId = equipment.OwnerMarble.RuntimeData.InstId;
                    break;
                default:
                    return;
            }

            if(targetCamp == Owner.RuntimeData.SourceCamp)
                return;

            if(targetReceiveDamage == null)
                return;

            // TODO: 计算伤害

            targetReceiveDamage.ReceiveDamage(Owner.RuntimeData.Damage, null);
            Owner.RuntimeData.TryMarkHit(targetMarbleInstId);
            
            Owner.RuntimeData.RemainPiercingCount--;
            if (Owner.RuntimeData.RemainPiercingCount < 0)
                Owner.Despawn();
        }
    }
}
