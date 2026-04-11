using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public abstract class EquipmentAbility<T> : Ability<T>
    {
        public T EquipmentOwner => Owner;
    }
}
