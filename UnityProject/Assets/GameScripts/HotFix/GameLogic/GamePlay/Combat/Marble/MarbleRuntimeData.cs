using Sirenix.OdinInspector;
using UnityEngine;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    /// <summary>
    /// Marble 运行时数据。
    /// </summary>
    [System.Serializable]
    public partial class MarbleRuntimeData : RuntimeData
    {
        [ShowInInspector]
        public int Camp { get; set; }
        [ShowInInspector]
        public bool IsAlive { get; set; }

        #region 生命值和护盾
        [ShowInInspector]
        public int Hp { get; set; }
        [ShowInInspector]
        public int MaxHp { get; set; }
        [ShowInInspector]
        public int MaxHpAddition { get; set; }
        [ShowInInspector]
        public float MaxHpMultiplier { get; set; } = 1f;

        // 治疗附加
        [ShowInInspector]
        public int HealAddition { get; set; }
        // 治疗倍率
        [ShowInInspector]
        public float HealMultiplier { get; set; } = 1f;

        // 易伤附加
        [ShowInInspector]
        public int DamageAddition { get; set; }
        // 易伤倍率
        [ShowInInspector]
        public float DamageMultiplier { get; set; } = 1f;

        [ShowInInspector]
        public int Shield { get; set; }
        [ShowInInspector]
        public int MaxShield { get; set; }

        // 护盾附加
        [ShowInInspector]
        public int ShieldHealAddition { get; set; }
        // 护盾倍率
        [ShowInInspector]
        public float ShieldHealMultiplier { get; set; } = 1f;
        #endregion

        #region 攻击
        [ShowInInspector]
        public int Attack { get; set; }
        [ShowInInspector]
        public int AttackAddition { get; set; }
        [ShowInInspector]
        public float AttackMultiplier { get; set; } = 1f;
        #endregion

        #region 防御
        [ShowInInspector]
        public int Defense { get; set; }
        [ShowInInspector]
        public int DefenseAddition { get; set; }
        [ShowInInspector]
        public float DefenseMultiplier { get; set; } = 1f;
        #endregion


        [ShowInInspector]
        public int Level { get; set; }
        [ShowInInspector]
        public int Exp { get; set; }
        [ShowInInspector]
        public int UpgradeExp { get; set; }

        [ShowInInspector]
        public float Scale { get; set; }
        [ShowInInspector]
        public float Mass { get; set; }
        [ShowInInspector]
        public float Acceleration { get; set; }
        [ShowInInspector]
        public float TargetVelocity { get; set; }
        [ShowInInspector]
        public float AngularAcceleration { get; set; }
        [ShowInInspector]
        public float TargetAngularVelocity { get; set; }
        [ShowInInspector]
        public UnityEngine.Vector2 TargetDirection { get; set; }

        [ShowInInspector]
        public PriorityValueManager<Vector2> TargetDirectionManager { get; } = new PriorityValueManager<Vector2>(new DirectionCombineStrategy());

        [ShowInInspector]
        public int TargetMarbleInstId { get; set; }

        public MarbleRuntimeData(string configId, int instId) : base(configId, instId)
        {
        }

    }
}
