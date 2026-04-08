using UnityEngine;

namespace GameLogic.Equipment
{
    public class ArmorMountAbility : EquipmentAbility<ArmorRuntimeData>
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
