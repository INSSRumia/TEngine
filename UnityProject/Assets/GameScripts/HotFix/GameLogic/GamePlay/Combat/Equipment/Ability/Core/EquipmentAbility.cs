using GameLogic.Gameplay.Combat.Marble;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public abstract class EquipmentAbility : Ability<Equipment>
    {
        public Equipment EquipmentOwner => base.Owner;
    }
}
