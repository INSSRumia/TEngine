using GameLogic.Marble;
using GameLogic.GamePlay.Common;

namespace GameLogic.Equipment
{
    public abstract class EquipmentAbility<T> : Ability<T>
    {
        public T EquipmentOwner => Owner;
    }
}
