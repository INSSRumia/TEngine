using System.Collections.Generic;
using GameConfig.Gameplay.Combat;
using GameLogic.Gameplay.Combat.Equipment;
using TEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public static partial class MarbleFactory
    {
        private static readonly List<IMarbleAbilityCreatorForConfig> _lstAbilityCreatorsForConfig = new List<IMarbleAbilityCreatorForConfig>
        {
            new DefaultMarbleAbilityCreatorForConfig(),
        };

        private static int _instIdCounter = 1;
        private static int _instAbilityIdCounter = 1;
        public static int GetNextInstId => _instIdCounter++;
        public static int GetNextInstAbilityId => _instAbilityIdCounter++;

        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Marbles/Marble");

        public static Marble CreateMarble(string id, int camp, int level = 0)
        {
            var config = ConfigSystem.Instance.Tables.TbMarble.Get(id);
            var levelData = GetMarbleLevelConfig(id, level);
            if (config == null || levelData == null)
            {
                return null;
            }

            var runtimeData = new MarbleRuntimeData(config, levelData) { Camp = camp };

            return CreateMarble(runtimeData);
        }

        private static Marble CreateMarble(MarbleRuntimeData runtimeData)
        {
            var levelData = GetMarbleLevelConfig(runtimeData.ConfigId, runtimeData.Level);
            var marbleComponent = CreateMarbleInternal(runtimeData.ConfigId);
            marbleComponent.Init(runtimeData);
            AttachDefaultAbilities(marbleComponent, levelData);
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

        private static void AttachDefaultAbilities(Marble marbleComponent, MarbleLevelConfig levelData)
        {
            AttachCoreAbility(marbleComponent, new MarbleSyncScaleAbility(levelData.SyncScale));
            AttachCoreAbility(marbleComponent, new MarbleSyncMassAbility(levelData.SyncMass));
            AttachCoreAbility(marbleComponent, new MarbleDamagePipelineAbility(levelData.DamagePipeline));
            AttachCoreAbility(marbleComponent, new MarbleHealPipelineAbility(levelData.HealPipeline));
            AttachCoreAbility(marbleComponent, new MarbleShieldHealPipelineAbility(levelData.ShieldHealPipeline));
            AttachCoreAbility(marbleComponent, new MarbleReceiveDamageAbility(levelData.ReceiveDamage));
            AttachCoreAbility(marbleComponent, new MarbleAddHealAbility(levelData.AddHeal));
            AttachCoreAbility(marbleComponent, new MarbleAddExpAbility(levelData.AddExp));
            AttachCoreAbility(marbleComponent, new MarbleDeathAbility(levelData.Death));
            AttachCoreAbility(marbleComponent, new MarbleLevelUpAbility(levelData.LevelUp));
            AttachCoreAbility(marbleComponent, new MarbleGetTargetAbility(levelData.GetTarget));
            AttachCoreAbility(marbleComponent, new MarbleMovementAbility(levelData.Movement));
            AttachCoreAbility(marbleComponent, new MarbleRotationAbility(levelData.Rotation));

            AttachConfigAbilities(marbleComponent, levelData);
        }

        private static void AttachCoreAbility(Marble marble, MarbleAbility ability)
        {
            ability.Category = AbilityCategory.Core;
            marble.AddAbility(ability);
        }

        private static void AttachConfigAbilities(Marble marbleComponent, MarbleLevelConfig levelData)
        {
            if (levelData?.LstAbility == null)
                return;

            foreach (var config in levelData.LstAbility)
            {
                var ability = CreateAbilityFromConfig(config);
                if (ability == null)
                    continue;

                ability.Priority = config.Priority;
                marbleComponent.AddAbility(ability);
            }
        }

        public static void RegisterAbilityCreatorForConfig(IMarbleAbilityCreatorForConfig creator)
        {
            _lstAbilityCreatorsForConfig.Add(creator);
            _lstAbilityCreatorsForConfig.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public static MarbleAbility CreateAbilityFromConfig(MarbleAbilityConfig config)
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
                EquipmentFactory.CreateEquipment(marbleComponent, equipmentConfig, config.Level, (Equipment.EnumEquipmentSlot)config.Slot);
            }
        }
    }

    public interface IMarbleAbilityCreatorForConfig
    {
        int Priority { get; set; }
        MarbleAbility CreateAbility(MarbleAbilityConfig config);
    }

    public class DefaultMarbleAbilityCreatorForConfig : IMarbleAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;

        public MarbleAbility CreateAbility(MarbleAbilityConfig config)
        {
            return config switch
            {
                MarbleCloseToTargetAbilityConfig closeConfig => new MarbleCloseToTargetAbility(closeConfig),
                MarbleDefaultRotateAbilityConfig defaultRotateConfig => new MarbleDefaultRotateAbility(defaultRotateConfig),
                MarbleDashAbilityConfig dashConfig => new MarbleDashAbility(dashConfig),
                MarbleFaceTargetDirectionAbilityConfig faceTargetDirectionConfig => new MarbleFaceTargetDirectionAbility(faceTargetDirectionConfig),
                _ => null
            };
        }
    }
}
