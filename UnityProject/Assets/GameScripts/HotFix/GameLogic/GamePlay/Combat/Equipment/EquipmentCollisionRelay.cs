using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    /// <summary>
    /// 挂在子刚体节点上，把碰撞事件转发给根 Equipment。
    /// 适用于多刚体武器，避免子 Rigidbody2D 抢走根节点的碰撞回调。
    /// </summary>
    public sealed class EquipmentCollisionRelay : MonoBehaviour
    {
        private Equipment _owner;

        private void Awake()
        {
            _owner = GetComponentInParent<Equipment>();
            if (_owner == null)
            {
                Log.Error($"[装备碰撞转发器] 未在父节点找到装备: {name}");
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _owner?.HandleCollisionEnter2D(collision);
        }
    }
}
