using GameLogic.Marble;
using GameLogic.GamePlay.Common;

namespace GameLogic.Equipment
{
    public abstract class EquipmentAbility<TRuntimeData> : GameLogic.GamePlay.Common.Ability<TRuntimeData>
        where TRuntimeData : EquipmentRuntimeData
    {
        public Equipment<TRuntimeData> EquipmentOwner => Owner as Equipment<TRuntimeData>;
    }
}
