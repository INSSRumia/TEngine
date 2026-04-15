using System.Collections.Generic;
using GameConfig.Gameplay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public static partial class EquipmentFactory
    {
        private static readonly List<IEquipmentCreatorForConfig> _lstCreatorsForConfig = new List<IEquipmentCreatorForConfig>
        {
            new DefaultEquipmentCreatorForConfig(),
        };

        private static readonly List<IEquipmentAbilityCreatorForConfig> _lstAbilityCreatorsForConfig = new List<IEquipmentAbilityCreatorForConfig>
        {
            new DefaultEquipmentAbilityCreatorForConfig(),
        };

        private static int _instIdCounter = 0;
        public static int GetNextInstId => _instIdCounter++;

        private static int _instAbilityIdCounter = 0;
        public static int GetNextInstAbilityId => _instAbilityIdCounter++;

        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Equipment/");

        public static void RegisterEquipmentCreatorForConfig(IEquipmentCreatorForConfig creator)
        {
            _lstCreatorsForConfig.Add(creator);
            _lstCreatorsForConfig.Sort((a, b) => b.Priority.CompareTo(a.Priority));
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

            var equipment = gameObject.GetComponent<Equipment>();
            if (equipment == null)
            {
                Log.Error($"Equipment 组件缺失: {config.ConfigId}");
                return null;
            }

            foreach (var creator in _lstCreatorsForConfig)
            {
                var runtimeData = creator.CreateEquipmentRuntimeData(config, levelConfig, slot);
                if (runtimeData != null)
                {
                    equipment.Init(ownerMarble, runtimeData);
                    creator.AttachDefaultAbilities(equipment, levelConfig);
                    AttachConfigAbilities(equipment, levelConfig);
                    break;
                }
            }

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

        private static void AttachConfigAbilities(Equipment equipment, EquipmentLevelConfig levelConfig)
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
                case ArmorLevelConfig:
                {
                    return new ArmorRuntimeData(config.ConfigId, levelConfig.Level)
                    {
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        // Hp = armorConfig.Armor.,
                    };
                }
                case BowLevelConfig:
                {
                    return new BowRuntimeData(config.ConfigId, levelConfig.Level)
                    {
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        CanFire = false,
                    };
                }
                case SwordLevelConfig:
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
                    AttachArmorDefaultAbilities(armorEquipment, levelConfig as ArmorLevelConfig);
                    break;
                case BowEquipment bowEquipment:
                    AttachBowDefaultAbilities(bowEquipment, levelConfig as BowLevelConfig);
                    break;
                case SwordEquipment swordEquipment:
                    AttachSwordDefaultAbilities(swordEquipment, levelConfig as SwordLevelConfig);
                    break;
                default:
                {
                    Log.Error($"Equipment 类型错误: {equipment.GetType().Name}");
                    break;
                }
            }
        }

        private static void AttachArmorDefaultAbilities(ArmorEquipment equipment, ArmorLevelConfig config)
        {
            if (config == null)
            {
                Log.Error("ArmorLevelConfig 配置错误");
                return;
            }

            AttachEquipmentCoreAbilities(equipment);

            switch (config.Armor)
            {
                case ArmorReduceDamageAbilityConfig reduceDamageConfig:
                    AttachCoreAbility(equipment, new ArmorReduceDamageAbility(reduceDamageConfig.Defense));
                    break;
                case ArmorAbsorbDamageAbilityConfig absorbDamageConfig:
                    AttachCoreAbility(equipment, new ArmorAbsorbDamageAbility(absorbDamageConfig.Defense, absorbDamageConfig.Hp));
                    break;
            }
        }

        private static void AttachBowDefaultAbilities(BowEquipment equipment, BowLevelConfig config)
        {
            if (config == null)
            {
                Log.Error("BowLevelConfig 配置错误");
                return;
            }

            AttachEquipmentCoreAbilities(equipment);
            AttachWeaponCoreAbilities(equipment, config);
            AttachCoreAbility(equipment, new BowAimAbility(config.BowAim.RotateSpeed, config.BowAim.AimAngle));
            AttachCoreAbility(equipment, new BowFireAbility(
                config.BowFire.ProjectileConfigId,
                config.BowFire.ProjectileLevel,
                config.BowFire.ArrowInterval,
                config.BowFire.ArrowCount,
                config.BowFire.ArrowAngleStep,
                config.BowFire.ShootType));
        }

        private static void AttachSwordDefaultAbilities(SwordEquipment equipment, SwordLevelConfig config)
        {
            if (config == null)
            {
                Log.Error("SwordLevelConfig 配置错误");
                return;
            }

            AttachEquipmentCoreAbilities(equipment);
            AttachWeaponCoreAbilities(equipment, config);
            AttachCoreAbility(equipment, new SwordCollisionAttackAbility(
                config.SwordCollisionAttack.IsDamageByVelocity,
                config.SwordCollisionAttack.VelocityDamageFactor));
        }

        private static void AttachEquipmentCoreAbilities(Equipment equipment)
        {
            AttachCoreAbility(equipment, new EquipmentMountAbility(equipment.RuntimeData.Slot));
            AttachCoreAbility(equipment, new EquipmentBrokenAbility());
        }

        private static void AttachWeaponCoreAbilities(WeaponEquipment equipment, WeaponLevelConfig config)
        {
            AttachCoreAbility(equipment, new WeaponCooldownAbility(config.Cooldown.Cooldown));
            AttachCoreAbility(equipment, new WeaponCalculateDamageAbility(config.CalculateDamage.Attack));
        }

        private static void AttachCoreAbility(Equipment equipment, EquipmentAbility ability)
        {
            ability.Category = AbilityCategory.Core;
            equipment.AddAbility(ability);
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
