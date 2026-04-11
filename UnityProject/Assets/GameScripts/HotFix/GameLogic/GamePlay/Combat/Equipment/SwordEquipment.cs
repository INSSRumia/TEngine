using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class SwordEquipment : WeaponEquipment
    {
        public new SwordRuntimeData RuntimeData => base.RuntimeData as SwordRuntimeData;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GetAbility<SwordCollisionAttackAbility>()?.HandleCollision(collision);
        }
    }
}
