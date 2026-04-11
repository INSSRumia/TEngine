using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class Equipment : ASC<EquipmentRuntimeData>
    {
        [ShowInInspector]
        public Marble.Marble OwnerMarble { get; private set; }

        public void Init(Marble.Marble ownerMarble, EquipmentRuntimeData runtimeData)
        {
            OwnerMarble = ownerMarble;
            base.Init(runtimeData);
        }
    }
}
