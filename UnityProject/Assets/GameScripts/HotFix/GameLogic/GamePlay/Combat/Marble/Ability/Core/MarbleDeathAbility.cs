using GameLogic.Gameplay.Combat.Equipment;
using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleDeathAbility : MarbleAbility
    {
        public MarbleDeathAbility(MarbleDeathAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public void Execute()
        {
            if (!Owner.RuntimeData.State.IsAlive || Owner.RuntimeData.State.Hp > 0)
                return;
            Log.Info($"[弹珠死亡能力] 弹珠 {Owner.RuntimeData.ConfigId} 死亡");

            var lstBeforeDeathAbilities = ListPool<IBeforeDeath>.Get();
            Owner.GetAbilities<IBeforeDeath>(ref lstBeforeDeathAbilities);
            foreach (var ability in lstBeforeDeathAbilities)
                ability.BeforeDeath();
            ListPool<IBeforeDeath>.Release(lstBeforeDeathAbilities);

            if(Owner.RuntimeData.State.Hp > 0)
                return;

            HandleDeath();

            Owner.RuntimeData.State.IsAlive = false;
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

            var sortingGroup = Owner.GetComponentInChildren<SortingGroup>();
            if (sortingGroup != null)
                sortingGroup.sortingLayerName = "Death";

            var sprite = Owner.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                var color = sprite.color;
                color.r *= 0.5f;
                color.g *= 0.5f;
                color.b *= 0.5f;
                sprite.color = color;
                sprite.transform.localScale *= 0.8f;            
            }

        }
    }
}
