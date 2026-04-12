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
                    creator.AttachDefaultAbilities(equipment);
                    break;
                }
            }

            AttachOptionalAbilities(equipment, levelConfig);

            if (equipment != null)
            {
                ownerMarble.RegisterEquipment(equipment);
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

        private static void AttachOptionalAbilities(Equipment equipment, EquipmentLevelConfig levelConfig)
        {
            if (levelConfig?.LstAbility == null)
                return;

            foreach (var config in levelConfig.LstAbility)
            {
                var ability = CreateAbilityFromConfig(equipment, config);
                if (ability != null)
                {
                    ability.Priority = config.Priority;
                    equipment.AddAbility(ability);
                }
            }

        }

        private static List<IEquipmentAbilityCreatorForConfig> _lstEquipmentAbilityCreatorsForConfig = new List<IEquipmentAbilityCreatorForConfig>
        {
            new DefaultEquipmentAbilityCreatorForConfig(),
        };
        private static EquipmentAbility CreateAbilityFromConfig(Equipment equipment, EquipmentAbilityConfig config)
        {
            foreach (var creator in _lstEquipmentAbilityCreatorsForConfig)
            {
                var ability = creator.CreateAbility(equipment, config);
                if (ability != null)
                {
                    ability.Priority = creator.Priority;
                    equipment.AddAbility(ability);
                }
            }
            return null;
        }
    }

    public interface IEquipmentCreatorForConfig
    {
        int Priority { get; set; }
        EquipmentRuntimeData CreateEquipmentRuntimeData(EquipmentConfig config, EquipmentLevelConfig levelConfig, EnumEquipmentSlot slot);
        void AttachDefaultAbilities(Equipment equipment);
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
                    return new ArmorRuntimeData
                    (
                        configId: config.ConfigId,
                        instId: 0,
                        slot: slot,
                        isEquipped: true,
                        isBroken: false,
                        hp: armorConfig.Hp,
                        maxHp: armorConfig.Hp,
                        defense: armorConfig.Defense
                    );
                }
                case BowLevelConfig bowConfig:
                {
                    return new BowRuntimeData
                    (
                        configId: config.ConfigId,
                        instId: 0,
                        slot: slot,
                        isEquipped: true,
                        isBroken: false,
                        attack: bowConfig.Attack,
                        isDamageByVelocity: bowConfig.IsDamageByVelocity,
                        cooldown: bowConfig.Cooldown,
                        rotateSpeed: bowConfig.RotateSpeed,
                        shootType: bowConfig.ShootType,
                        arrowCount: bowConfig.ArrowCount,
                        arrowInterval: bowConfig.ArrowInterval,
                        arrowAngleStep: bowConfig.ArrowAngleStep,
                        aimAngle: bowConfig.AimAngle,
                        targetMarbleInstId: 0,
                        aimDirection: Vector2.zero,
                        canFire: false
                    );
                }
                case SwordLevelConfig swordConfig:
                {
                    return new SwordRuntimeData
                    (
                        configId: config.ConfigId,
                        instId: 0,
                        slot: slot,
                        isEquipped: true,
                        isBroken: false,
                        attack: swordConfig.Attack,
                        isDamageByVelocity: swordConfig.IsDamageByVelocity,
                        cooldown: swordConfig.Cooldown
                    );
                }
            }
            return null;
        }

        public void AttachDefaultAbilities(Equipment equipment)
        {
            switch (equipment)
            {
                case ArmorEquipment armorEquipment:
                {
                    armorEquipment.AddAbility(new EquipmentMountAbility());
                    armorEquipment.AddAbility(new EquipmentBrokenAbility());
                    break;
                }
                case BowEquipment bowEquipment:
                {
                    bowEquipment.AddAbility(new EquipmentMountAbility());
                    bowEquipment.AddAbility(new EquipmentBrokenAbility());
                    bowEquipment.AddAbility(new WeaponCooldownAbility());
                    bowEquipment.AddAbility(new WeaponCalculateDamageAbility());
                    bowEquipment.AddAbility(new BowAimAbility());
                    bowEquipment.AddAbility(new BowShootAbility());
                    break;
                }
                case SwordEquipment swordEquipment:
                {
                    swordEquipment.AddAbility(new EquipmentMountAbility());
                    swordEquipment.AddAbility(new EquipmentBrokenAbility());
                    swordEquipment.AddAbility(new WeaponCooldownAbility());
                    swordEquipment.AddAbility(new WeaponCalculateDamageAbility());
                    swordEquipment.AddAbility(new SwordCollisionAttackAbility());
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
                ArmorReduceDamageAbilityConfig => new ArmorReduceDamageAbility(),
                ArmorAbsorbDamageAbilityConfig => new ArmorAbsorbDamageAbility(),
                _ => null
            };
        }
    }
}
