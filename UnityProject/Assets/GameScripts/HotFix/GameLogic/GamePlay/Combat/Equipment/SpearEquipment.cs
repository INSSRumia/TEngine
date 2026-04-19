using Sirenix.OdinInspector;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SpearEquipment : WeaponEquipment
    {
        public new SpearRuntimeData RuntimeData => base.RuntimeData as SpearRuntimeData;

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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollisionEnter2D(collision);
        }

        public override void HandleCollisionEnter2D(Collision2D collision)
        {
            GetAbility<SwordCollisionAttackAbility>()?.HandleCollision(collision);
        }

    }
}
