namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponEquipment : Equipment
    {
        public new WeaponRuntimeData RuntimeData => base.RuntimeData as WeaponRuntimeData;
    }
}
