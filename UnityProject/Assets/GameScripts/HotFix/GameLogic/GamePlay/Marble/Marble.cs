using System.Collections.Generic;
using UnityEngine;
using GameLogic.Equipment;

namespace GameLogic.Marble
{
    public class Marble : GameLogic.GamePlay.Common.ASC<MarbleRuntimeData>
    {
        [SerializeField] private List<Transform> _lstEquipmentPoints = new List<Transform>();
        private readonly Dictionary<EquipmentSlot, Transform> _slotPointMap = new Dictionary<EquipmentSlot, Transform>();

        private void Awake()
        {
            InitEquipmentSlotPoint();
        }
        private void InitEquipmentSlotPoint()
        {
            for(int i = 0; i < _lstEquipmentPoints.Count; i++)
            {
                _slotPointMap[(EquipmentSlot)i] = _lstEquipmentPoints[i];
            }
        }

        public Transform GetEquipmentSlotPoint(EquipmentSlot slot)
        {
            if (_slotPointMap.TryGetValue(slot, out var point) && point != null)
                return point;
            return null;
        }
    }
}
