using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponEquipment : Equipment
    {
        public new WeaponRuntimeData RuntimeData => base.RuntimeData as WeaponRuntimeData;
    }
}
