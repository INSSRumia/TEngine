using GameLogic.Gameplay.Combat.Equipment;
using GameLogic.Gameplay.Combat;
using TEngine;
using UnityEngine;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleDeathAbility : MarbleAbility
    {
        public void Execute()
        {
            if (!Owner.RuntimeData.IsAlive || Owner.RuntimeData.Hp > 0)
                return;
            Log.Info($"[MarbleDeathAbility]  marble {Owner.RuntimeData.ConfigId} 死亡");

            var lstBeforeDeathAbilities = ListPool<IBeforeDeath>.Get();
            Owner.GetAbilities<IBeforeDeath>(ref lstBeforeDeathAbilities);
            foreach (var ability in lstBeforeDeathAbilities)
                ability.BeforeDeath();
            ListPool<IBeforeDeath>.Release(lstBeforeDeathAbilities);

            if(Owner.RuntimeData.Hp > 0)
                return;

            HandleDeath();

            Owner.RuntimeData.IsAlive = false;
            GameEvent.Send(EventDef.Combat.MarbleDie, Owner);
        }

        private void HandleDeath()
        {
            foreach (var equipment in Owner.EquipmentMap){
                equipment.Value.RuntimeData.IsBroken = true;
            }
            Owner.RemoveAllAbilities();

            if(Owner.Rigidbody != null)
            {
                Owner.Rigidbody.drag = 5f;
                Owner.Rigidbody.angularDrag = 5f;
            }

            var collider = Owner.GetComponentInChildren<Collider2D>();
            if (collider != null)
                collider.enabled = false;

            var sprite = Owner.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                sprite.transform.localScale *= 0.8f;            
            }

        }
    }
}
