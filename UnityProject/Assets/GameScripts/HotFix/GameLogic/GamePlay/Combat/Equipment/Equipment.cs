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

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if(RuntimeData.IsBroken)
            {
                GetAbility<EquipmentBrokenAbility>()?.Execute();
            }
        }
    }
}
