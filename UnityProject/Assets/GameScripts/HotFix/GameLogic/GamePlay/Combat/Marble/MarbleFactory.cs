using TEngine;
using UnityEngine;
using GameLogic.GamePlay.Combat;
using GameLogic.Gameplay.Combat.Equipment;

namespace GameLogic.Gameplay.Combat.Marble
{
    public static class MarbleFactory
    {
        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Marbles/Marble");

        public static Marble CreateMarble(string id, int camp, int level = 0)
        {
            var levelData = GetMarbleLevelConfig(id, level);
            if (levelData == null)
            {
                return null;
            }

            var runtimeData = new MarbleRuntimeData
            {
                ConfigId = id,
                InstId = 0,
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
                TargetVelocity = levelData.Speed,
                Acceleration = levelData.Speed * 10,
                AngularAcceleration = 360f,
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

        public static GameConfig.GameConfig.MarbleLevelConfig GetMarbleLevelConfig(string id, int level)
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
            AttachCoreAbility(marbleComponent, new MarbleDeadAbility());
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

        private static void AttachOptionalAbilities(Marble marbleComponent, GameConfig.GameConfig.MarbleLevelConfig levelData)
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

        public static Ability<Marble> CreateAbilityFromConfig(GameConfig.GameConfig.MarbleAbilityConfig config)
        {
            return config switch
            {
                GameConfig.GameConfig.MarbleCloseToTargetAbilityConfig closeConfig =>
                    new MarbleCloseToTargetAbility { CloseDistance = closeConfig.CloseDistance },
                _ => null
            };
        }

        private static void AttachEquipment(Marble marbleComponent, GameConfig.GameConfig.MarbleLevelConfig levelData)
        {
            if (marbleComponent == null || levelData?.LstEquipment == null)
                return;

            foreach (var config in levelData.LstEquipment)
            {
                var equipmentConfig = ConfigSystem.Instance.Tables.TbEquipment.Get(config.ConfigId);
                EquipmentFactory.CreateEquipment(marbleComponent, equipmentConfig, config.Level, (EquipmentSlot)config.Slot);
            }
        }
    }
}
