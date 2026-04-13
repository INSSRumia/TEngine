using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    // 装备损坏
    public class EquipmentBrokenAbility : EquipmentAbility
    {
        public void Execute()
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null)
                return;

            EquipmentOwner.RuntimeData.IsBroken = true;
            Log.Info($"[EquipmentBrokenAbility] 装备 {EquipmentOwner.RuntimeData.ConfigId} 损坏");

            HandleBroken();
        }

        private void HandleBroken()
        {
            EquipmentOwner.RemoveAllAbilities();

            if(EquipmentOwner.Rigidbody != null)
            {
                EquipmentOwner.Rigidbody.drag = 5f;
                EquipmentOwner.Rigidbody.angularDrag = 5f;
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
