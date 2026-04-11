using GameLogic.Marble;
using GameLogic.GamePlay.Common;
using UnityEngine;

namespace GameLogic.Equipment
{
    public class Equipment : ASC<EquipmentRuntimeData>
    {
        public Marble.Marble OwnerMarble { get; private set; }

        public void Init(Marble.Marble ownerMarble, EquipmentRuntimeData runtimeData)
        {
            OwnerMarble = ownerMarble;
            base.Init(runtimeData);
        }
    }


    public class ArmorEquipment : Equipment 
    {
        public new ArmorRuntimeData RuntimeData => base.RuntimeData as ArmorRuntimeData;
    }

    public class WeaponEquipment : Equipment
    {
        public new WeaponRuntimeData RuntimeData => base.RuntimeData as WeaponRuntimeData;
    }

    public class SwordEquipment : WeaponEquipment
    {
        public new SwordRuntimeData RuntimeData => base.RuntimeData as SwordRuntimeData;
    }
    public class BowEquipment : WeaponEquipment
    {
        public new BowRuntimeData RuntimeData => base.RuntimeData as BowRuntimeData;
    }
}
