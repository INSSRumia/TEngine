namespace GameLogic.Gameplay.Combat.Equipment
{
    public abstract class EquipmentAbility : Ability<Equipment>
    {
        public Equipment EquipmentOwner => base.Owner;
        public EquipmentAbility() : base(EquipmentFactory.GetNextInstId) { }
    }
}
