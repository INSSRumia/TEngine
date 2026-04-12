namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowEquipment : WeaponEquipment
    {
        public new BowRuntimeData RuntimeData => base.RuntimeData as BowRuntimeData;
    }
}
