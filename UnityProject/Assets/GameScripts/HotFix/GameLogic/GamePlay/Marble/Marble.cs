using System.Collections.Generic;
using UnityEngine;
using GameLogic.Equipment;

namespace GameLogic.Marble
{
    public class Marble : GameLogic.GamePlay.Common.ASC<MarbleRuntimeData>
    {
        private readonly Dictionary<EquipmentSlot, Transform> _slotPointMap = new Dictionary<EquipmentSlot, Transform>();

        public Transform GetEquipmentSlotPoint(EquipmentSlot slot)
        {
            if (_slotPointMap.TryGetValue(slot, out var point) && point != null)
                return point;

            point = CreateSlotPoint(slot);
            _slotPointMap[slot] = point;
            return point;
        }

        private Transform CreateSlotPoint(EquipmentSlot slot)
        {
            var slotObject = new GameObject($"EquipmentSlot_{slot}");
            var slotTransform = slotObject.transform;
            slotTransform.SetParent(transform, false);
            slotTransform.localPosition = GetSlotLocalPosition(slot);
            return slotTransform;
        }

        private static Vector3 GetSlotLocalPosition(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Top => new Vector3(0f, 0.6f, 0f),
                EquipmentSlot.Left => new Vector3(-0.6f, 0f, 0f),
                EquipmentSlot.Right => new Vector3(0.6f, 0f, 0f),
                EquipmentSlot.Bottom => new Vector3(0f, -0.6f, 0f),
                _ => Vector3.zero,
            };
        }
    }
}
