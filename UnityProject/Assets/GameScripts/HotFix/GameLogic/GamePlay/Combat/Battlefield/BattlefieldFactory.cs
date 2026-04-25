using UnityEngine;
using TEngine;
using GameConfig.Gameplay.Combat;
namespace GameLogic.Gameplay.Combat
{
    public static class BattlefieldFactory
    {
        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Battlefield/");

        public static CombatBattlefieldConfig ResolveConfig(string battlefieldConfigId)
        {
            if (string.IsNullOrWhiteSpace(battlefieldConfigId))
                return null;

            return ConfigSystem.Instance.Tables?.TbCombatBattlefield?.GetOrDefault(battlefieldConfigId);
        }

        public static Battlefield CreateBattlefield(string battlefieldConfigId, Transform parent)
        {
            var config = ResolveConfig(battlefieldConfigId);
            if (config == null)
            {
                Log.Error($"[战场工厂] 未找到战场配置: {battlefieldConfigId}");
                return null;
            }

            var location = _path + config.BattlefieldConfigId;
            var gameObject = GameModule.Resource.LoadGameObject(location, parent);
            if (gameObject == null)
            {
                Log.Error($"[战场工厂] 未找到战场预制体: {location}");
                return null;
            }

            gameObject.name = config.BattlefieldConfigId;
            var battlefield = gameObject.GetComponent<Battlefield>();
            if (battlefield == null)
            {
                Log.Error($"[战场工厂] 战场预制体缺少 Battlefield 组件: {location}");
                Object.Destroy(gameObject);
                return null;
            }

            return battlefield;
        }
    }
}