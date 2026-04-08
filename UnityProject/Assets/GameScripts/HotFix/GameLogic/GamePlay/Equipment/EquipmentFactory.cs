using GameConfig.GameConfig;
using GameLogic.GamePlay.Common;
using UnityEngine;

namespace GameLogic.Equipment
{
    public static class EquipmentFactory
    {
        public static ASC CreateEquipment(Marble.Marble ownerMarble, EquipmentConfig config)
        {
            if (ownerMarble == null || config == null)
                return null;

            switch (config)
            {
                case ArmorConfig armorConfig:
                {
                    var gameObject = new GameObject($"Equipment_{armorConfig.ConfigId}");
                    var equipment = gameObject.AddComponent<ArmorEquipment>();
                    equipment.Init(ownerMarble, new ArmorRuntimeData
                    {
                        ConfigId = armorConfig.ConfigId,
                        Slot = (EquipmentSlot)armorConfig.Slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Hp = armorConfig.Hp,
                        MaxHp = armorConfig.Hp,
                        Defense = armorConfig.Defense,
                    });
                    AttachDefaultAbilities(equipment);
                    return equipment;
                }
                case BowConfig bowConfig:
                {
                    var gameObject = new GameObject($"Equipment_{bowConfig.ConfigId}");
                    var equipment = gameObject.AddComponent<BowEquipment>();
                    equipment.Init(ownerMarble, new BowRuntimeData
                    {
                        ConfigId = bowConfig.ConfigId,
                        Slot = (EquipmentSlot)bowConfig.Slot,
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
                case WeaponConfig weaponConfig:
                {
                    var gameObject = new GameObject($"Equipment_{weaponConfig.ConfigId}");
                    var equipment = gameObject.AddComponent<WeaponEquipment>();
                    equipment.Init(ownerMarble, new WeaponRuntimeData
                    {
                        ConfigId = weaponConfig.ConfigId,
                        Slot = (EquipmentSlot)weaponConfig.Slot,
                        IsEquipped = true,
                        IsBroken = false,
                        Attack = weaponConfig.Attack,
                        IsDamageByVelocity = weaponConfig.IsDamageByVelocity,
                        Cooldown = weaponConfig.Cooldown,
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
            equipment.AddAbility(new ArmorMountAbility());
            equipment.AddAbility(new ArmorReduceDamageAbility());
            equipment.AddAbility(new ArmorReceiveDamageAbility());
        }

        private static void AttachDefaultAbilities(WeaponEquipment equipment)
        {
            equipment.AddAbility(new WeaponMountAbility());
            equipment.AddAbility(new WeaponCooldownAbility());
            equipment.AddAbility(new WeaponCollisionDamageAbility());
            equipment.AddAbility(new WeaponCollisionAttackAbility());
        }

        private static void AttachDefaultAbilities(BowEquipment equipment)
        {
            equipment.AddAbility(new BowMountAbility());
            equipment.AddAbility(new WeaponCooldownAbility());
            equipment.AddAbility(new BowFindTargetAbility());
            equipment.AddAbility(new BowAimAbility());
            equipment.AddAbility(new BowShootAbility());
        }
    }
}
