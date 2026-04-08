using TEngine;
using UnityEngine;

namespace GameLogic.Marble
{
    public static class MarbleFactory
    {
        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Prefabs/Marbles");

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
                IsAlive = true,
                Level = level,
                UpgradeExp = levelData.UpgradeExp,
                MaxHp = levelData.Hp,
                Hp = levelData.Hp,
                MaxShield = levelData.Shield,
                Shield = levelData.Shield,
                Defense = levelData.Defense,
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
            var marbleComponent = CreateMarbleInternal(runtimeData.ConfigId);
            marbleComponent.Init(runtimeData);
            AttachDefaultAbilities(marbleComponent);
            return marbleComponent;
        }

        private static Marble CreateMarbleInternal(string id)
        {
            var prefab = GameModule.Resource.LoadGameObject(_path + "\\" + id);
            var marble = GameObject.Instantiate(prefab);
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
            marbleComponent.AddAbility(new MarbleSyncScaleAbility());
            marbleComponent.AddAbility(new MarbleSyncMassAbility());
            marbleComponent.AddAbility(new MarbleAddDamageAbility());
            marbleComponent.AddAbility(new MarbleAddHealAbility());
            marbleComponent.AddAbility(new MarbleAddExpAbility());
            marbleComponent.AddAbility(new MarbleHandleDamageAbility());
            marbleComponent.AddAbility(new MarbleDeadAbility());
            marbleComponent.AddAbility(new MarbleLevelUpAbility());
            marbleComponent.AddAbility(new MarbleMovementAbility());
            marbleComponent.AddAbility(new MarbleRotationAbility());
        }
    }
}
