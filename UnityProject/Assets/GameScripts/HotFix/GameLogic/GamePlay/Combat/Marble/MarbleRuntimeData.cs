using Sirenix.OdinInspector;
using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    /// <summary>
    /// Marble 能力系统共享访问的运行时黑板根对象。
    /// 这里显式拆分为 State / Config / Frame 三个区域，
    /// 分别承载持久状态、配置投影与逐帧临时求解数据，避免不同语义的数据继续平铺混杂。
    /// </summary>
    [System.Serializable]
    public partial class MarbleRuntimeData : RuntimeData
    {
        [ShowInInspector]
        public int Camp { get; set; }
        [ShowInInspector]
        public MarbleStateData State { get; } = new MarbleStateData();
        [ShowInInspector]
        public MarbleConfigData Config { get; } = new MarbleConfigData();
        [ShowInInspector]
        public MarbleFrameData Frame { get; } = new MarbleFrameData();

        public MarbleRuntimeData(MarbleConfig config, MarbleLevelConfig levelConfig) : base(config.ConfigId, levelConfig.Level, MarbleFactory.GetNextInstId)
        {
            State.IsAlive = true;
            Level = levelConfig.Level;
            Config.UpgradeExp = levelConfig.UpgradeExp;
            State.MaxHp = levelConfig.Hp;
            State.Hp = levelConfig.Hp;
            State.MaxShield = levelConfig.Shield;
            State.Shield = levelConfig.Shield;
            Config.Defense = levelConfig.Defense;
            Config.Attack = levelConfig.Attack;
            Config.DamageMultiplier = 1f;
            Config.HealMultiplier = 1f;
            Config.ShieldHealMultiplier = 1f;
            Config.AttackMultiplier = 1f;
            Config.MaxHpMultiplier = 1f;
            Config.DefenseMultiplier = 1f;
            Config.Scale = levelConfig.Scale;
            Config.Mass = levelConfig.Mass;
        }

        [System.Serializable]
        public class MarbleStateData
        {
            /// <summary>
            /// 长生命周期状态区。
            /// 这里存放会被结算流程直接改写的状态，例如生命、护盾、经验、目标与存活状态。
            /// </summary>
            [ShowInInspector]
            public bool IsAlive { get; set; }
            [ShowInInspector]
            public int Hp { get; set; }
            [ShowInInspector]
            public int MaxHp { get; set; }
            [ShowInInspector]
            public int Shield { get; set; }
            [ShowInInspector]
            public int MaxShield { get; set; }
            [ShowInInspector]
            public int Exp { get; set; }
            [ShowInInspector]
            public int TargetMarbleInstId { get; set; }
        }

        [System.Serializable]
        public class MarbleConfigData
        {
            /// <summary>
            /// 配置投影区。
            /// 这里存放由等级配置初始化、并可能被长期 buff/能力修正的属性，
            /// 供伤害、治疗、成长、移动等能力统一读取。
            /// </summary>
            [ShowInInspector]
            public int MaxHpAddition { get; set; }
            [ShowInInspector]
            public float MaxHpMultiplier { get; set; } = 1f;
            [ShowInInspector]
            public int HealAddition { get; set; }
            [ShowInInspector]
            public float HealMultiplier { get; set; } = 1f;
            [ShowInInspector]
            public int DamageAddition { get; set; }
            [ShowInInspector]
            public float DamageMultiplier { get; set; } = 1f;
            [ShowInInspector]
            public int ShieldHealAddition { get; set; }
            [ShowInInspector]
            public float ShieldHealMultiplier { get; set; } = 1f;
            [ShowInInspector]
            public int Attack { get; set; }
            [ShowInInspector]
            public int AttackAddition { get; set; }
            [ShowInInspector]
            public float AttackMultiplier { get; set; } = 1f;
            [ShowInInspector]
            public int Defense { get; set; }
            [ShowInInspector]
            public int DefenseAddition { get; set; }
            [ShowInInspector]
            public float DefenseMultiplier { get; set; } = 1f;
            [ShowInInspector]
            public int UpgradeExp { get; set; }
            [ShowInInspector]
            public float Scale { get; set; }
            [ShowInInspector]
            public float Mass { get; set; }
        }

        [System.Serializable]
        public class MarbleFrameData
        {
            /// <summary>
            /// 帧级临时黑板。
            /// 这里的值通常在每个 FixedUpdate 内被多个行为能力写入，再由移动/旋转能力统一消费。
            /// 不要把需要跨帧持久保存的状态写到这一层。
            /// </summary>
            [ShowInInspector]
            public PriorityValueManager<float> AccelerationManager { get; } = new PriorityValueManager<float>(new ScalarMaxCombineStrategy());
            [ShowInInspector]
            public PriorityValueManager<float> TargetVelocityManager { get; } = new PriorityValueManager<float>(new ScalarMaxCombineStrategy());
            [ShowInInspector]
            public PriorityValueManager<float> AngularAccelerationManager { get; } = new PriorityValueManager<float>(new ScalarMaxCombineStrategy());
            [ShowInInspector]
            public PriorityValueManager<float> TargetAngularVelocityManager { get; } = new PriorityValueManager<float>(new ScalarSumCombineStrategy());
            [ShowInInspector]
            public Vector2 TargetDirection { get; set; }
            [ShowInInspector]
            public PriorityValueManager<Vector2> TargetDirectionManager { get; } = new PriorityValueManager<Vector2>(new DirectionCombineStrategy());
        }
    }
}
