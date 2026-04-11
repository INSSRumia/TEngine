using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowShootAbility : EquipmentAbility
    {
        private BowEquipment _owner;
        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is BowEquipment bowEquipment)
                _owner = bowEquipment;
            else
                Log.Error($"BowShootAbility 添加失败, 装备类型不匹配: {EquipmentOwner.GetType().Name}");
        }

        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }

        public bool TryBuildShot(out IReadOnlyList<Vector2> shotDirections)
        {
            shotDirections = null;
            if (_owner == null || _owner.RuntimeData == null || !_owner.RuntimeData.CanFire)
                return false;

            var cooldownAbility = _owner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return false;

            var result = new List<Vector2>();
            var forward = (Vector2)_owner.transform.right;
            var count = Mathf.Max(1, _owner.RuntimeData.ArrowCount);

            if (_owner.RuntimeData.ShootType == 1)
            {
                var centerIndex = 0;
                for (var i = 0; i < count; i++)
                {
                    var offsetIndex = i == 0 ? 0 : (i % 2 == 1 ? centerIndex + 1 : -(centerIndex + 1));
                    if (i % 2 == 0 && i > 0)
                        centerIndex++;
                    var angle = offsetIndex * _owner.RuntimeData.ArrowAngleStep;
                    result.Add(Quaternion.Euler(0f, 0f, angle) * forward);
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    result.Add(forward);
                }
            }

            shotDirections = result;
            return true;
        }
    }
}
