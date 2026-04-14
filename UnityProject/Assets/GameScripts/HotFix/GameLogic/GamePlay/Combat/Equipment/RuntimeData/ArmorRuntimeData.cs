namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorRuntimeData : EquipmentRuntimeData
    {
        public int Hp { get; set; }
        public ArmorRuntimeData(string configId, int level) : base(configId, level) { }
    }
}
