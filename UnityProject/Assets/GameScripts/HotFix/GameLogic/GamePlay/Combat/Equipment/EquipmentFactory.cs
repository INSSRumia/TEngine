using System.Collections.Generic;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public static partial class EquipmentFactory
    {
        private static int _instIdCounter = 0;
        public static int GetNextInstId => _instIdCounter++;

        private static int _instAbilityIdCounter = 0;
        public static int GetNextInstAbilityId => _instAbilityIdCounter++;

        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Equipment/");

        private static readonly List<IEquipmentCreatorForConfig> _lstEquipmentCreatorsForConfig = new List<IEquipmentCreatorForConfig>
        {
            new DefaultEquipmentCreatorForConfig(),
        };
        public static void RegisterEquipmentCreatorForConfig(IEquipmentCreatorForConfig creator)
        {
            _lstEquipmentCreatorsForConfig.Add(creator);
            // 降序排序
            _lstEquipmentCreatorsForConfig.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public static Equipment CreateEquipment(Marble.Marble ownerMarble, EquipmentConfig config, int level, EnumEquipmentSlot slot)
        {
            if (ownerMarble == null || config == null)
                return null;

            var levelConfig = config.LstLevelConfig.Find(x => x.Level == level);
            if (levelConfig == null)
            {
                Log.Error($"Equipment level config not found: {config.ConfigId} {level}");
                return null;
            }

            var gameObject = GameModule.Resource.LoadGameObject(_path + config.ConfigId);

            Equipment equipment = gameObject.GetComponent<Equipment>();
            if(equipment == null)
            {
                Log.Error($"Equipment 组件缺失: {config.ConfigId}");
                return null;
            }

            foreach (var creator in _lstEquipmentCreatorsForConfig)
            {
                var runtimeData = creator.CreateEquipmentRuntimeData(config, levelConfig, slot);
                if (runtimeData != null)
                {
                    equipment.Init(ownerMarble, runtimeData);
                    creator.AttachDefaultAbilities(equipment, levelConfig);
                    break;
                }
            }

            AttachOptionalAbilities(equipment, levelConfig);

            return equipment;
        }

        public static void DestroyEquipment(Equipment equipment)
        {
            if (equipment == null)
                return;

            Log.Info($"[EquipmentFactory] 销毁装备: {equipment.RuntimeData?.ConfigId}");

            if (equipment.gameObject != null)
            {
                Object.Destroy(equipment.gameObject);
            }
        }

        private static void AttachOptionalAbilities(Equipment equipment, EquipmentLevelConfig levelConfig)
        {
            if (levelConfig?.LstAbility == null)
                return;

            foreach (var config in levelConfig.LstAbility)
            {
                var ability = CreateAbilityFromConfig(equipment, config);
                if (ability == null)
                    continue;

                ability.Priority = config.Priority;
                equipment.AddAbility(ability);
            }
        }

        private static readonly List<IEquipmentAbilityCreatorForConfig> _lstAbilityCreatorsForConfig = new List<IEquipmentAbilityCreatorForConfig>
        {
            new DefaultEquipmentAbilityCreatorForConfig(),
        };

        public static void RegisterAbilityCreatorForConfig(IEquipmentAbilityCreatorForConfig creator)
        {
            _lstAbilityCreatorsForConfig.Add(creator);
            _lstAbilityCreatorsForConfig.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        private static EquipmentAbility CreateAbilityFromConfig(Equipment equipment, EquipmentAbilityConfig config)
        {
            foreach (var creator in _lstAbilityCreatorsForConfig)
            {
                var ability = creator.CreateAbility(equipment, config);
                if (ability != null)
                {
                    return ability;
                }
            }

            Log.Error($"Equipment ability creator for config not found: {config.GetType().Name}");
            return null;
        }
    }

    public interface IEquipmentCreatorForConfig
    {
        int Priority { get; set; }
        EquipmentRuntimeData CreateEquipmentRuntimeData(EquipmentConfig config, EquipmentLevelConfig levelConfig, EnumEquipmentSlot slot);
        void AttachDefaultAbilities(Equipment equipment, EquipmentLevelConfig levelConfig);
    }
    public class DefaultEquipmentCreatorForConfig : IEquipmentCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public EquipmentRuntimeData CreateEquipmentRuntimeData(EquipmentConfig config, EquipmentLevelConfig levelConfig, EnumEquipmentSlot slot)
        {
            switch (levelConfig)
            {
                case ArmorLevelConfig armorConfig:
                {
                    return new ArmorRuntimeData(config.ConfigId, levelConfig.Level)
                    {
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        // Hp = armorConfig.Armor.,
                    };
                }
                case BowLevelConfig bowConfig:
                {
                    return new BowRuntimeData(config.ConfigId, levelConfig.Level)
                    {
                        Slot = slot,
                        IsEquipped =  true,
                        IsBroken = false,
                        CanFire = false,
                    };
                }
                case SwordLevelConfig swordConfig:
                {
                    return new SwordRuntimeData(config.ConfigId, levelConfig.Level)
                    {
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                    };
                }
            }
            return null;
        }

        public void AttachDefaultAbilities(Equipment equipment, EquipmentLevelConfig levelConfig)
        {
            switch (equipment)
            {
                case ArmorEquipment armorEquipment:
                {
                    armorEquipment.AddAbility(new EquipmentMountAbility(equipment.RuntimeData.Slot));
                    armorEquipment.AddAbility(new EquipmentBrokenAbility());

                    var armorConfig = levelConfig as ArmorLevelConfig;
                    if (armorConfig == null)
                    {
                        Log.Error($"ArmorLevelConfig 配置错误: {levelConfig.Level}");
                        break;
                    }
                    switch(armorConfig.Armor)
                    {
                        case ArmorReduceDamageAbilityConfig c1 :
                            armorEquipment.AddAbility(new ArmorReduceDamageAbility(c1.Defense));
                            break;
                        case ArmorAbsorbDamageAbilityConfig c2 :
                            armorEquipment.AddAbility(new ArmorAbsorbDamageAbility(c2.Defense, c2.Hp));
                            break;
                    }

                    break;
                }
                case BowEquipment bowEquipment:
                {
                    bowEquipment.AddAbility(new EquipmentMountAbility(equipment.RuntimeData.Slot));
                    bowEquipment.AddAbility(new EquipmentBrokenAbility());
                    var bowConfig = levelConfig as BowLevelConfig;
                    if (bowConfig == null)
                    {
                        Log.Error($"BowLevelConfig 配置错误: {levelConfig.Level}");
                        break;
                    }
                    bowEquipment.AddAbility(new WeaponCooldownAbility(bowConfig.Cooldown.Cooldown));
                    bowEquipment.AddAbility(new WeaponCalculateDamageAbility(bowConfig.CalculateDamage.Attack));
                    bowEquipment.AddAbility(new BowAimAbility(bowConfig.BowAim.RotateSpeed, bowConfig.BowAim.RotateSpeed));
                    bowEquipment.AddAbility(new BowFireAbility(bowConfig.BowFire.ProjectileConfigId, bowConfig.BowFire.ProjectileLevel, bowConfig.BowFire.ArrowInterval, bowConfig.BowFire.ArrowCount, bowConfig.BowFire.ArrowAngleStep, bowConfig.BowFire.ShootType));
                    break;
                }
                case SwordEquipment swordEquipment:
                {
                    swordEquipment.AddAbility(new EquipmentMountAbility(equipment.RuntimeData.Slot));
                    swordEquipment.AddAbility(new EquipmentBrokenAbility());
                    var swordConfig = levelConfig as SwordLevelConfig;
                    if (swordConfig == null)
                    {
                        Log.Error($"SwordLevelConfig 配置错误: {levelConfig.Level}");
                        break;
                    }
                    swordEquipment.AddAbility(new WeaponCooldownAbility(swordConfig.Cooldown.Cooldown));
                    swordEquipment.AddAbility(new WeaponCalculateDamageAbility(swordConfig.CalculateDamage.Attack));
                    swordEquipment.AddAbility(new SwordCollisionAttackAbility(swordConfig.SwordCollisionAttack.IsDamageByVelocity, swordConfig.SwordCollisionAttack.VelocityDamageFactor));
                    break;
                }
                default:
                {
                    Log.Error($"Equipment 类型错误: {equipment.GetType().Name}");
                    break;
                }
            }
        }
    }

    public interface IEquipmentAbilityCreatorForConfig
    {
        int Priority { get; set; }
        EquipmentAbility CreateAbility(Equipment equipment, EquipmentAbilityConfig config);
    }
    public class DefaultEquipmentAbilityCreatorForConfig : IEquipmentAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public EquipmentAbility CreateAbility(Equipment equipment, EquipmentAbilityConfig config)
        {
            return config switch
            {
                _ => null
            };
        }
    }
}
