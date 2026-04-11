using GameConfig.GameConfig;
using GameLogic.GamePlay.Combat;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public static class EquipmentFactory
    {
        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Equipment/");
        public static ASC CreateEquipment(Marble.Marble ownerMarble, EquipmentConfig config, int level, EquipmentSlot slot)
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

            switch (levelConfig)
            {
                case ArmorLevelConfig armorConfig:
                {
                    var equipment = gameObject.GetComponent<ArmorEquipment>();
                    equipment.Init(ownerMarble, new ArmorRuntimeData
                    {
                        ConfigId = config.ConfigId,
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Hp = armorConfig.Hp,
                        MaxHp = armorConfig.Hp,
                        Defense = armorConfig.Defense,
                    });
                    AttachDefaultAbilities(equipment);
                    return equipment;
                }
                case BowLevelConfig bowConfig:
                {
                    var equipment = gameObject.GetComponent<BowEquipment>();
                    equipment.Init(ownerMarble, new BowRuntimeData
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
                    AttachDefaultAbilities(equipment);
                    return equipment;
                }
                case SwordLevelConfig swordConfig:
                {
                    var equipment = gameObject.GetComponent<SwordEquipment>();
                    equipment.Init(ownerMarble, new SwordRuntimeData
                    {
                        ConfigId = config.ConfigId,
                        Slot = slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Attack = swordConfig.Attack,
                        IsDamageByVelocity = swordConfig.IsDamageByVelocity,
                        Cooldown = swordConfig.Cooldown,
                    });
                    AttachDefaultAbilities(equipment);
                    return equipment;
                }
                default:
                    return null;
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
    }
}
