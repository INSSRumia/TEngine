using GameLogic.Marble;
using GameLogic.GamePlay.Common;
using UnityEngine;

namespace GameLogic.Equipment
{
    public class Equipment<TRuntimeData> : GameLogic.GamePlay.Common.ASC<TRuntimeData>
        where TRuntimeData : EquipmentRuntimeData
    {
        public Marble.Marble OwnerMarble { get; private set; }

        public void Init(Marble.Marble ownerMarble, TRuntimeData runtimeData)
        {
            OwnerMarble = ownerMarble;
            base.Init(runtimeData);
        }
    }

    public class Equipment : Equipment<EquipmentRuntimeData> { }
    public class ArmorEquipment : Equipment<ArmorRuntimeData> { }
    public class WeaponEquipment : Equipment<WeaponRuntimeData>
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            GetAbility<WeaponCollisionAttackAbility>()?.HandleCollision(collision);
        }
    }
    public class BowEquipment : Equipment<BowRuntimeData> { }
}
