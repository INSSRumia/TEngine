using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowEquipment : WeaponEquipment
    {
        public new BowRuntimeData RuntimeData => base.RuntimeData as BowRuntimeData;
    }
}
