using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    // 装备挂载到角色身上
    public class EquipmentMountAbility : EquipmentAbility
    {
        public override void OnAdd()
        {
            MountToOwner();
        }

        public void MountToOwner()
        {
            if (EquipmentOwner == null || EquipmentOwner.OwnerMarble == null)
                return;

            var slotPoint = EquipmentOwner.OwnerMarble.GetEquipmentSlotPoint(EquipmentOwner.RuntimeData.Slot);
            EquipmentOwner.transform.SetParent(slotPoint, false);
            EquipmentOwner.transform.localPosition = Vector3.zero;
            EquipmentOwner.transform.localRotation = Quaternion.identity;

            BindJoint();
            EquipmentOwner.RuntimeData.IsEquipped = true;
            Log.Info($"[EquipmentMountAbility] 装备 {EquipmentOwner.RuntimeData.ConfigId} 挂载到角色 {EquipmentOwner.OwnerMarble.RuntimeData.ConfigId}");
        }

        private void BindJoint()
        {
            if (EquipmentOwner.Rigidbody == null)
                return;

            var joints = EquipmentOwner.GetComponents<Joint2D>();
            foreach (var joint in joints)
            {
                joint.connectedBody = EquipmentOwner.OwnerMarble.Rigidbody;
            }
        }
    }
}
