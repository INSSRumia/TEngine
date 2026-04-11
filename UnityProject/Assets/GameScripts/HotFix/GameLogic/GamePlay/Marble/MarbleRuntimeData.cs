namespace GameLogic.Marble
{
    /// <summary>
    /// Marble 运行时数据。
    /// </summary>
    public partial class MarbleRuntimeData : RuntimeData
    {
        public int Camp { get; set; }
        public bool IsAlive { get; set; }

        #region 生命值和护盾
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int MaxHpAddition { get; set; }
        public float MaxHpMultiplier { get; set; } = 1f;

        // 治疗附加
        public int HealAddition { get; set; }
        // 治疗倍率
        public float HealMultiplier { get; set; } = 1f;

        // 易伤附加
        public int DamageAddition { get; set; }
        // 易伤倍率
        public float DamageMultiplier { get; set; } = 1f;

        public int Shield { get; set; }
        public int MaxShield { get; set; }

        // 护盾附加
        public int ShieldHealAddition { get; set; }
        // 护盾倍率
        public float ShieldHealMultiplier { get; set; } = 1f;
        #endregion

        #region 攻击
        public int Attack { get; set; }
        public int AttackAddition { get; set; }
        public float AttackMultiplier { get; set; } = 1f;
        #endregion

        #region 防御
        public int Defense { get; set; }
        public int DefenseAddition { get; set; }
        public float DefenseMultiplier { get; set; } = 1f;
        #endregion


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

        public int TargetMarbleInstId { get; set; }
    }
}
