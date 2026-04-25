using System.Collections.Generic;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    // 装备挂载到角色身上
    public class EquipmentMountAbility : EquipmentAbility, IAbilityFixedUpdate
    {
        private static readonly Dictionary<EnumEquipmentSlot, Quaternion> _slotConnectedAnchorMap = new()
        {
            { EnumEquipmentSlot.Top, Quaternion.Euler(0, 0, 90) },
            { EnumEquipmentSlot.Left, Quaternion.Euler(0, 0, 180) },
            { EnumEquipmentSlot.Right, Quaternion.Euler(0, 0, 0) },
            { EnumEquipmentSlot.Bottom, Quaternion.Euler(0, 0, -90) },
            { EnumEquipmentSlot.Middle, Quaternion.identity },
        };
        private List<AnchoredJoint2D> _joints = new List<AnchoredJoint2D>();

        public EnumEquipmentSlot Slot {get; private set;}
        public EquipmentMountAbility(EquipmentMountAbilityConfig config, EnumEquipmentSlot slot)
        {
            Slot = slot;
        }
        public override void OnAdd()
        {
            MountToOwner();
        }

        public void MountToOwner()
        {
            if (EquipmentOwner == null || EquipmentOwner.OwnerMarble == null)
                return;

            var slotPoint = EquipmentOwner.OwnerMarble.GetEquipmentSlotPoint(Slot);
            EquipmentOwner.transform.SetParent(slotPoint, true);
            EquipmentOwner.transform.localPosition = Vector3.zero;
            EquipmentOwner.transform.localRotation = Quaternion.identity;

            BindJoint();
            EquipmentOwner.RuntimeData.IsEquipped = true;

            EquipmentOwner.OwnerMarble.RegisterEquipment(EquipmentOwner, Slot);
            Log.Info($"[装备挂载能力] 装备 {EquipmentOwner.RuntimeData.ConfigId} 挂载到角色 {EquipmentOwner.OwnerMarble.RuntimeData.ConfigId}");
        }

        private void BindJoint()
        {
            if (EquipmentOwner.Rigidbody == null)
                return;

            var joints = EquipmentOwner.GetComponents<Joint2D>();
            foreach (var joint in joints)
            {
                if(joint is AnchoredJoint2D anchorJoint)
                {
                    _joints.Add(anchorJoint);
                    anchorJoint.connectedBody = EquipmentOwner.OwnerMarble.Rigidbody;
                    // anchorJoint.connectedAnchor = _slotConnectedAnchorMap[Slot] * anchorJoint.connectedAnchor;
                }
            }
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            // foreach (var joint in _joints)
            // {
            //     joint.connectedBody = EquipmentOwner.OwnerMarble.Rigidbody;
            //     joint.connectedAnchor = _slotConnectedAnchorMap[Slot];
            // }
        }

    }
}
