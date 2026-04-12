namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorEquipment : Equipment 
    {
        public new ArmorRuntimeData RuntimeData => base.RuntimeData as ArmorRuntimeData;
    }
}
