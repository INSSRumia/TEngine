namespace GameLogic
{
    public abstract class RuntimeData
    {
        public string ConfigId { get; set; }
        public int InstId { get; set; }
        public int DamageAddition { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public int HealAddition { get; set; }
        public float HealMultiplier { get; set; } = 1f;
        public int ShieldAddition { get; set; }
        public float ShieldMultiplier { get; set; } = 1f;
    }
}
