using Sirenix.OdinInspector;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SpearEquipment : SwordEquipment
    {
        [ShowInInspector]
        public SliderJoint2D SliderJoint { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SliderJoint = GetComponentInChildren<SliderJoint2D>(true);
            if (SliderJoint == null)
            {
                Log.Error($"[SpearEquipment] 未找到子节点 SliderJoint2D: {name}");
            }
        }

    }
}
