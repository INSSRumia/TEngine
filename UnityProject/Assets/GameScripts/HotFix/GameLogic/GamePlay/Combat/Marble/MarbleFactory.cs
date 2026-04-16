using System.Collections.Generic;
using GameConfig.Gameplay.Combat;
using GameLogic.Gameplay.Combat.Equipment;
using TEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    /// <summary>
    /// Marble 装配入口。
    /// 负责把 Luban 等级配置转换为运行时实体，并明确区分：
    /// 1. 固定骨架能力挂载
    /// 2. 配置驱动扩展能力挂载
    /// 3. 装备继续装配
    /// </summary>
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
            // 固定骨架能力先挂载，保证 Marble 的受伤、治疗、移动、升级等基础链路先完整建立。
            AttachDefaultAbilities(marbleComponent, levelData);
            // 装备装配放在 Marble 初始化之后进行，避免装备能力读取不到 OwnerMarble 的基础黑板。
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

            // 配置驱动扩展能力最后挂载，让玩法层能力建立在固定骨架之上。
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

            // lst_ability 只承载玩法扩展能力；固定骨架能力不应从这里重复声明。
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
