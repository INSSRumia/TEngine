using System.Collections.Generic;
using UnityEngine;
using GameLogic.Gameplay.Combat.Equipment;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class Marble : GameLogic.GamePlay.Combat.ASC<MarbleRuntimeData>
    {
        [SerializeField] private List<Transform> _lstEquipmentPoints = new List<Transform>();
        private readonly Dictionary<EquipmentSlot, Transform> _slotPointMap = new Dictionary<EquipmentSlot, Transform>();
        private readonly Dictionary<EquipmentSlot, Equipment.Equipment> _equipmentMap = new Dictionary<EquipmentSlot, Equipment.Equipment>();
        public IReadOnlyDictionary<EquipmentSlot, Equipment.Equipment> EquipmentMap => _equipmentMap;

        protected override void Awake()
        {
            base.Awake();
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

        public Equipment.Equipment GetEquipment(EquipmentSlot slot)
        {
            return _equipmentMap.TryGetValue(slot, out var equipment) ? equipment : null;
        }


        public void RegisterEquipment(Equipment.Equipment equipment)
        {
            if (equipment == null || equipment.RuntimeData == null)
                return;

            var slot = equipment.RuntimeData.Slot;

            if (_equipmentMap.TryGetValue(slot, out var existingEquipment) && existingEquipment != null && existingEquipment != equipment)
            {
                DestroyEquipment(slot);
            }

            _equipmentMap[slot] = equipment;
        }

        public void UnregisterEquipment(EquipmentSlot slot)
        {
            _equipmentMap.Remove(slot);
        }

        public void DestroyEquipment(EquipmentSlot slot)
        {
            if (_equipmentMap.TryGetValue(slot, out var equipment) && equipment != null)
            {
                _equipmentMap.Remove(slot);
                EquipmentFactory.DestroyEquipment(equipment);
            }
        }

        public void DestroyAllEquipment()
        {
            foreach (var kvp in _equipmentMap)
            {
                if (kvp.Value != null)
                {
                    EquipmentFactory.DestroyEquipment(kvp.Value);
                }
            }
            _equipmentMap.Clear();
        }

        private void OnDrawGizmos()
        {
            if (RuntimeData == null)
                return;

#if UNITY_EDITOR
            var pos = transform.position;

            // 绘制 TargetDirection（Editor 预览方向的半透明射线）
            var targetDir = RuntimeData.TargetDirection;
            if (targetDir.sqrMagnitude > 0.001f)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
                Gizmos.DrawRay(pos, targetDir.normalized * 3f);
            }
#endif
        }

        private void OnDrawGizmosSelected()
        {
            if (RuntimeData == null)
                return;

#if UNITY_EDITOR
            var pos = transform.position;

            // 绘制 TargetDirection（选中时更清晰的实线）
            var targetDir = RuntimeData.TargetDirection;
            if (targetDir.sqrMagnitude > 0.001f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(pos, targetDir.normalized * 3f);
            }

            // 通过 TargetMarbleInstId 查询目标 Marble，并绘制连线
            var targetInstId = RuntimeData.TargetMarbleInstId;
            if (targetInstId > 0 && CombatManager != null)
            {
                var targetMarble = CombatManager.GetTarget(targetInstId);
                if (targetMarble != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(pos, targetMarble.transform.position);

                    // 绘制目标处的圆点标记
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(targetMarble.transform.position, 0.3f);
                }
            }
#endif
        }
    }
}
