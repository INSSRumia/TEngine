using Sirenix.OdinInspector;
using UnityEngine;
using GameConfig.Gameplay.Combat;

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
