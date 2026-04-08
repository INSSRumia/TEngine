namespace GameLogic.Marble
{
    /// <summary>
    /// Marble 运行时数据。
    /// </summary>
    public partial class MarbleRuntimeData : RuntimeData
    {
        public int Camp { get; set; }
        public bool IsAlive { get; set; }

        public int Hp { get; set; }
        public int MaxHp { get; set; }

        public int Shield { get; set; }
        public int MaxShield { get; set; }

        public int PendingDamage { get; set; }
        public int PendingHeal { get; set; }

        public int Defense { get; set; }

        public int Level { get; set; }
        public int Exp { get; set; }
        public int UpgradeExp { get; set; }
        public float Scale { get; set; }
        public float Mass { get; set; }
        public float Acceleration { get; set; }
        public float TargetVelocity { get; set; }
        public float AngularAcceleration { get; set; }
        public float TargetAngularVelocity { get; set; }
        public UnityEngine.Vector2 TargetDirection { get; set; }
    }
}
