using UnityEngine;

namespace GameLogic.Equipment
{
    // 装备挂载到角色身上
    public class EquipmentMountAbility : EquipmentAbility<EquipmentRuntimeData>
    {
        public override void OnAdd()
        {
            MountToOwner();
        }

        private void MountToOwner()
        {
            if (EquipmentOwner == null || EquipmentOwner.OwnerMarble == null)
                return;

            var slotPoint = EquipmentOwner.OwnerMarble.GetEquipmentSlotPoint(EquipmentOwner.RuntimeData.Slot);
            EquipmentOwner.transform.SetParent(slotPoint, false);
            EquipmentOwner.transform.localPosition = Vector3.zero;
            EquipmentOwner.transform.localRotation = Quaternion.identity;

            BindJoint();
            EquipmentOwner.RuntimeData.IsEquipped = true;
        }

        private void BindJoint()
        {
            if (EquipmentOwner.Rigidbody == null || EquipmentOwner.OwnerMarble.Rigidbody == null)
                return;

            var joints = EquipmentOwner.GetComponents<Joint2D>();
            foreach (var joint in joints)
            {
                joint.connectedBody = EquipmentOwner.OwnerMarble.Rigidbody;
            }
        }
    }
}
