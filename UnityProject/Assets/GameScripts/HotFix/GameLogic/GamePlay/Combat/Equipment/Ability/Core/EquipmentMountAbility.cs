using System.Collections.Generic;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    // 装备挂载到角色身上
    public class EquipmentMountAbility : EquipmentAbility, IAbilityFixedUpdate
    {
        private static readonly Dictionary<EnumEquipmentSlot, Vector2> _slotConnectedAnchorMap = new Dictionary<EnumEquipmentSlot, Vector2>
        {
            { EnumEquipmentSlot.Top, new Vector2(0, 0.5f) },
            { EnumEquipmentSlot.Left, new Vector2(-0.5f, 0) },
            { EnumEquipmentSlot.Right, new Vector2(0.5f, 0) },
            { EnumEquipmentSlot.Bottom, new Vector2(0, -0.5f) },
            { EnumEquipmentSlot.Middle, new Vector2(0, 0) },
        };
        private List<HingeJoint2D> _joints = new List<HingeJoint2D>();

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
            EquipmentOwner.transform.SetParent(slotPoint, false);
            EquipmentOwner.transform.localPosition = Vector3.zero;
            EquipmentOwner.transform.localRotation = Quaternion.identity;

            BindJoint();
            EquipmentOwner.RuntimeData.IsEquipped = true;

            EquipmentOwner.OwnerMarble.RegisterEquipment(EquipmentOwner, Slot);
            Log.Info($"[EquipmentMountAbility] 装备 {EquipmentOwner.RuntimeData.ConfigId} 挂载到角色 {EquipmentOwner.OwnerMarble.RuntimeData.ConfigId}");
        }

        private void BindJoint()
        {
            if (EquipmentOwner.Rigidbody == null)
                return;

            var joints = EquipmentOwner.GetComponents<Joint2D>();
            foreach (var joint in joints)
            {
                if(joint is HingeJoint2D hingeJoint)
                {
                    _joints.Add(hingeJoint);
                }
            }
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var joint in _joints)
            {
                joint.connectedBody = EquipmentOwner.OwnerMarble.Rigidbody;
                joint.connectedAnchor = _slotConnectedAnchorMap[Slot];
            }
        }

    }
}
