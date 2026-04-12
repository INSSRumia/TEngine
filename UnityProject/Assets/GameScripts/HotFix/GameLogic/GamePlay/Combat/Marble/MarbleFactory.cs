using TEngine;
using GameLogic.Gameplay.Combat.Equipment;
using GameConfig.Gameplay.Combat;
using System.Collections.Generic;

namespace GameLogic.Gameplay.Combat.Marble
{
    public static partial class MarbleFactory
    {
        private static int _instIdCounter = 0;
        public static int GetNextInstId => _instIdCounter++;
        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Marbles/Marble");

        public static Marble CreateMarble(string id, int camp, int level = 0)
        {
            var levelData = GetMarbleLevelConfig(id, level);
            if (levelData == null)
            {
                return null;
            }

            var runtimeData = new MarbleRuntimeData(id, 0)
            {
                Camp = camp,
                IsAlive = true,
                Level = level,
                UpgradeExp = levelData.UpgradeExp,
                MaxHp = levelData.Hp,
                Hp = levelData.Hp,
                MaxShield = levelData.Shield,
                Shield = levelData.Shield,
                Defense = levelData.Defense,
                Attack = levelData.Attack,
                DamageMultiplier = 1f,
                HealMultiplier = 1f,
                ShieldHealMultiplier = 1f,
                AttackMultiplier = 1f,
                Scale = levelData.Scale,
                Mass = levelData.Mass,
            };

            return CreateMarble(runtimeData);
        }

        private static Marble CreateMarble(MarbleRuntimeData runtimeData)
        {
            var levelData = GetMarbleLevelConfig(runtimeData.ConfigId, runtimeData.Level);
            var marbleComponent = CreateMarbleInternal(runtimeData.ConfigId);
            marbleComponent.Init(runtimeData);
            AttachDefaultAbilities(marbleComponent);
            AttachOptionalAbilities(marbleComponent, levelData);
            AttachEquipment(marbleComponent, levelData);
            return marbleComponent;
        }

        private static Marble CreateMarbleInternal(string id)
        {
            var marble = GameModule.Resource.LoadGameObject(_path);
            return marble.GetComponent<Marble>();
        }

        public static MarbleLevelConfig GetMarbleLevelConfig(string id, int level)
        {
            var data = ConfigSystem.Instance.Tables.TbMarble.Get(id);
            var levelData = data.LstLevelConfig.Find(x => x.Level == level);
            if (levelData == null)
            {
                Log.Error($"Marble level data not found: {id} {level}");
                return null;
            }
            return levelData;
        }

        private static void AttachDefaultAbilities(Marble marbleComponent)
        {
            AttachCoreAbility(marbleComponent, new MarbleSyncScaleAbility());
            AttachCoreAbility(marbleComponent, new MarbleSyncMassAbility());
            AttachCoreAbility(marbleComponent, new MarbleDamagePipelineAbility());
            AttachCoreAbility(marbleComponent, new MarbleHealPipelineAbility());
            AttachCoreAbility(marbleComponent, new MarbleShieldHealPipelineAbility());
            AttachCoreAbility(marbleComponent, new MarbleReceiveDamageAbility());
            AttachCoreAbility(marbleComponent, new MarbleAddHealAbility());
            AttachCoreAbility(marbleComponent, new MarbleAddExpAbility());
            AttachCoreAbility(marbleComponent, new MarbleDeathAbility());
            AttachCoreAbility(marbleComponent, new MarbleLevelUpAbility());
            AttachCoreAbility(marbleComponent, new MarbleGetTargetAbility());
            AttachCoreAbility(marbleComponent, new MarbleMovementAbility());
            AttachCoreAbility(marbleComponent, new MarbleRotationAbility());
        }

        private static void AttachCoreAbility(Marble marble, Ability<Marble> ability)
        {
            ability.Category = AbilityCategory.Core;
            marble.AddAbility(ability);
        }

        private static void AttachOptionalAbilities(Marble marbleComponent, MarbleLevelConfig levelData)
        {
            if (levelData?.LstAbility == null)
                return;

            foreach (var config in levelData.LstAbility)
            {
                var ability = CreateAbilityFromConfig(config);
                if (ability != null)
                {
                    ability.Priority = config.Priority;
                    marbleComponent.AddAbility(ability);
                }
            }
        }

        private readonly static List<IMarbleAbilityCreatorForConfig> _lstAbilityCreatorsForConfig = new List<IMarbleAbilityCreatorForConfig>
        {
            new DefaultMarbleAbilityCreatorForConfig(),
        };
        public static void RegisterAbilityCreatorForConfig(IMarbleAbilityCreatorForConfig creator)
        {
            _lstAbilityCreatorsForConfig.Add(creator);
            // 降序排序
            _lstAbilityCreatorsForConfig.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public static Ability<Marble> CreateAbilityFromConfig(MarbleAbilityConfig config)
        {
            foreach (var creator in _lstAbilityCreatorsForConfig)
            {
                var ability = creator.CreateAbility(config);
                if (ability != null)
                {
                    return ability;
                }
            }
            Log.Error($"Marble ability creator for config not found: {config.GetType().Name}");
            return null;
        }

        private static void AttachEquipment(Marble marbleComponent, MarbleLevelConfig levelData)
        {
            if (marbleComponent == null || levelData?.LstEquipment == null)
                return;

            foreach (var config in levelData.LstEquipment)
            {
                var equipmentConfig = ConfigSystem.Instance.Tables.TbEquipment.Get(config.ConfigId);
                EquipmentFactory.CreateEquipment(marbleComponent, equipmentConfig, config.Level, (GameLogic.Gameplay.Combat.Equipment.EnumEquipmentSlot)config.Slot);
            }
        }
    }

    public interface IMarbleAbilityCreatorForConfig
    {
        int Priority { get; set; }
        Ability<Marble> CreateAbility(MarbleAbilityConfig config);
    }

    public class DefaultMarbleAbilityCreatorForConfig : IMarbleAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public Ability<Marble> CreateAbility(MarbleAbilityConfig config)
        {
            return config switch
            {
                MarbleCloseToTargetAbilityConfig closeConfig =>
                    new MarbleCloseToTargetAbility { CloseDistance = closeConfig.CloseDistance },
                _ => null
            };
        }
    }
}
