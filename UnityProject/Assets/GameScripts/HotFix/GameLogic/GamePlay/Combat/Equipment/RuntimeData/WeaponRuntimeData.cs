namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponRuntimeData : EquipmentRuntimeData
    {
        public int? Attack { get; set; }
        public bool IsDamageByVelocity { get; set; }
        public float Cooldown { get; set; }
        public float CooldownRemaining { get; set; }
    }
}
