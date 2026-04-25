using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    // 装备损坏
    public class EquipmentBrokenAbility : EquipmentAbility
    {
        public EquipmentBrokenAbility(EquipmentBrokenAbilityConfig config)
        {
        }

        public void Execute()
        {
            EquipmentOwner.RuntimeData.IsBroken = true;
            Log.Info($"[装备损坏能力] 装备 {EquipmentOwner.RuntimeData.ConfigId} 损坏");

            HandleBroken();
        }

        private void HandleBroken()
        {
            EquipmentOwner.RemoveAllAbilities();
            var rigidbodies = EquipmentOwner.GetComponentsInChildren<Rigidbody2D>();
            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.drag = 2f;
                rigidbody.angularDrag = 2f;
            }

            foreach (var collider in EquipmentOwner.GetComponentsInChildren<Collider2D>())
                collider.enabled = false;
            foreach (var joint in EquipmentOwner.GetComponentsInChildren<Joint2D>())
                joint.enabled = false;

            EquipmentOwner.transform.localScale *= 0.8f;
            var sprite = EquipmentOwner.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
                sprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
