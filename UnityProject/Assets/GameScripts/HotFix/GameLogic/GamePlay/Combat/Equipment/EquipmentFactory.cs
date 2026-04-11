using System.Collections.Generic;
using GameConfig;
using GameLogic.GamePlay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public static class EquipmentFactory
    {
        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Equipment/");

        public static Equipment CreateEquipment(Marble.Marble ownerMarble, EquipmentConfig config, int level, EquipmentSlot slot)
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

            Equipment equipment = null;
            switch (levelConfig)
            {
                case ArmorLevelConfig armorConfig:
                {
                    var armorEquipment = gameObject.GetComponent<ArmorEquipment>();
                    armorEquipment.Init(ownerMarble, new ArmorRuntimeData
                    {
                        ConfigId = config.ConfigId,
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Hp = armorConfig.Hp,
                        MaxHp = armorConfig.Hp,
                        Defense = armorConfig.Defense,
                    });
                    AttachDefaultAbilities(armorEquipment);
                    AttachOptionalAbilities(armorEquipment, armorConfig);
                    equipment = armorEquipment;
                    break;
                }
                case BowLevelConfig bowConfig:
                {
                    var bowEquipment = gameObject.GetComponent<BowEquipment>();
                    bowEquipment.Init(ownerMarble, new BowRuntimeData
                    {
                        ConfigId = config.ConfigId,
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Cooldown = bowConfig.Cooldown,
                        RotateSpeed = bowConfig.RotateSpeed,
                        ShootType = bowConfig.ShootType,
                        ArrowCount = bowConfig.ArrowCount,
                        ArrowInterval = bowConfig.ArrowInterval,
                        ArrowAngleStep = bowConfig.ArrowAngleStep,
                        AimAngle = bowConfig.AimAngle,
                    });
                    AttachDefaultAbilities(bowEquipment);
                    AttachOptionalAbilities(bowEquipment, bowConfig);
                    equipment = bowEquipment;
                    break;
                }
                case SwordLevelConfig swordConfig:
                {
                    var swordEquipment = gameObject.GetComponent<SwordEquipment>();
                    swordEquipment.Init(ownerMarble, new SwordRuntimeData
                    {
                        ConfigId = config.ConfigId,
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Attack = swordConfig.Attack,
                        IsDamageByVelocity = swordConfig.IsDamageByVelocity,
                        Cooldown = swordConfig.Cooldown,
                    });
                    AttachDefaultAbilities(swordEquipment);
                    AttachOptionalAbilities(swordEquipment, swordConfig);
                    equipment = swordEquipment;
                    break;
                }
            }

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

        private static void AttachDefaultAbilities(ArmorEquipment equipment)
        {
            equipment.AddAbility(new EquipmentMountAbility());
            // equipment.AddAbility(new ArmorReduceDamageAbility());
            // equipment.AddAbility(new ArmorAbsorbDamageAbility());
        }

        private static void AttachDefaultAbilities(SwordEquipment equipment)
        {
            equipment.AddAbility(new EquipmentMountAbility());
            equipment.AddAbility(new WeaponCooldownAbility());
            equipment.AddAbility(new WeaponCalculateDamageAbility());
            equipment.AddAbility(new SwordCollisionAttackAbility());
        }

        private static void AttachDefaultAbilities(BowEquipment equipment)
        {
            equipment.AddAbility(new EquipmentMountAbility());
            equipment.AddAbility(new WeaponCooldownAbility());
            equipment.AddAbility(new WeaponCalculateDamageAbility());
            equipment.AddAbility(new BowAimAbility());
            equipment.AddAbility(new BowShootAbility());
        }

        private static void AttachOptionalAbilities(ArmorEquipment equipment, ArmorLevelConfig levelConfig)
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

        private static void AttachOptionalAbilities(BowEquipment equipment, BowLevelConfig levelConfig)
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

        private static void AttachOptionalAbilities(SwordEquipment equipment, SwordLevelConfig levelConfig)
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

        private static List<IArmorAbilityCreatorForConfig> _lstArmorAbilityCreatorsForConfig = new List<IArmorAbilityCreatorForConfig>
        {
            new DefaultArmorAbilityCreatorForConfig(),
        };
        private static List<IBowAbilityCreatorForConfig> _lstBowAbilityCreatorsForConfig = new List<IBowAbilityCreatorForConfig>
        {
            new DefaultBowAbilityCreatorForConfig(),
        };
        private static List<ISwordAbilityCreatorForConfig> _lstSwordAbilityCreatorsForConfig = new List<ISwordAbilityCreatorForConfig>
        {
            new DefaultSwordAbilityCreatorForConfig(),
        };

        private static Ability<ArmorEquipment> CreateAbilityFromConfig(ArmorEquipment equipment, EquipmentAbilityConfig config)
        {
            foreach (var creator in _lstArmorAbilityCreatorsForConfig)
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

        private static Ability<BowEquipment> CreateAbilityFromConfig(BowEquipment equipment, EquipmentAbilityConfig config)
        {
            foreach (var creator in _lstBowAbilityCreatorsForConfig)
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

        private static Ability<SwordEquipment> CreateAbilityFromConfig(SwordEquipment equipment, EquipmentAbilityConfig config)
        {
            foreach (var creator in _lstSwordAbilityCreatorsForConfig)
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

    public interface IArmorAbilityCreatorForConfig
    {
        int Priority { get; set; }
        Ability<ArmorEquipment> CreateAbility(ArmorEquipment equipment, EquipmentAbilityConfig config);
    }
    public class DefaultArmorAbilityCreatorForConfig : IArmorAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public Ability<ArmorEquipment> CreateAbility(ArmorEquipment equipment, EquipmentAbilityConfig config)
        {
            return config switch
            {
                ArmorReduceDamageAbilityConfig => new ArmorReduceDamageAbility(),
                ArmorAbsorbDamageAbilityConfig => new ArmorAbsorbDamageAbility(),
                _ => null
            };
        }
    }
    public interface IBowAbilityCreatorForConfig
    {
        int Priority { get; set; }
        Ability<BowEquipment> CreateAbility(BowEquipment equipment, EquipmentAbilityConfig config);
    }
    public class DefaultBowAbilityCreatorForConfig : IBowAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public Ability<BowEquipment> CreateAbility(BowEquipment equipment, EquipmentAbilityConfig config)
        {
            return config switch
            {
                _ => null
            };
        }
    }
    public interface ISwordAbilityCreatorForConfig
    {
        int Priority { get; set; }
        Ability<SwordEquipment> CreateAbility(SwordEquipment equipment, EquipmentAbilityConfig config);
    }
    public class DefaultSwordAbilityCreatorForConfig : ISwordAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public Ability<SwordEquipment> CreateAbility(SwordEquipment equipment, EquipmentAbilityConfig config)
        {
            return config switch
            {
                _ => null
            };
        }
    }
}
