namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponRuntimeData : EquipmentRuntimeData
    {
        public float CooldownRemaining { get; set; }
        public WeaponRuntimeData(string configId, int level) : base(configId, level) { }
    }
}
