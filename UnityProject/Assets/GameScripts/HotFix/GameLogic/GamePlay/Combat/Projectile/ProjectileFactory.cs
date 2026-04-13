using TEngine;
using UnityEngine;
using GameConfig.Gameplay.Combat;
namespace GameLogic.Gameplay.Combat
{
    public static class ProjectileFactory
    {
        private static int _instIdCounter = 0;
        public static int GetNextInstId => _instIdCounter++;

        private static int _instAbilityIdCounter = 0;
        public static int GetNextInstAbilityId => _instAbilityIdCounter++;

        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Projectiles/");

        public static Projectile CreateProjectile(
            string configId,
            int level,
            Marble.Marble sourceMarble,
            Marble.Marble targetMarble,
            Vector2 targetPoint,
            Vector2 spawnPosition,
            Quaternion rotation,
            int damage)
        {
            if (sourceMarble?.RuntimeData == null)
            {
                Log.Error("[ProjectileFactory] Source marble is null");
                return null;
            }

            var projectileConfig = ConfigSystem.Instance.Tables.TbProjectile.Get(configId);
            if (projectileConfig == null)
            {
                Log.Error($"[ProjectileFactory] Projectile config not found: {configId}");
                return null;
            }

            var levelConfig = projectileConfig.LstLevelConfig.Find(x => x.Level == level);
            if (levelConfig == null)
            {
                Log.Error($"[ProjectileFactory] Projectile level config not found: {configId} level {level}");
                return null;
            }

            var projectile = CreateProjectileInternal(configId);

            if (projectile == null)
            {
                Log.Error($"[ProjectileFactory] Projectile component not found on prefab: {configId}");
                return null;
            }

            projectile.transform.position = spawnPosition;
            projectile.transform.rotation = rotation;

            var runtimeData = CreateProjectileRuntimeData(configId, level, sourceMarble, targetMarble, targetPoint, spawnPosition, rotation, damage);

            projectile.Init(runtimeData);

            AttachDefaultAbilities(projectile, levelConfig);
            AttachOptionalAbilities(projectile, levelConfig);

            return projectile;
        }

        private static ProjectileRuntimeData CreateProjectileRuntimeData(string configId, int level, Marble.Marble sourceMarble, Marble.Marble targetMarble, Vector2 targetPoint, Vector2 spawnPosition, Quaternion rotation, int damage)
        {
            return new ProjectileRuntimeData(configId, level)
            {
                SourceCamp = sourceMarble.RuntimeData.Camp,
                SourceMarbleInstId = sourceMarble.RuntimeData.InstId,
                TargetMarbleInstId = targetMarble?.RuntimeData?.InstId ?? 0,
                TargetPoint = targetPoint,
                CurrentLifetime = 0f,
                Damage = damage,
                StartPosition = spawnPosition,
                TargetDirection = rotation * Vector2.right,
            };
        }

        private static Projectile CreateProjectileInternal(string configId)
        {
            var path = _path + configId;
            var obj = GameModule.Resource.LoadGameObject(path);
            if (obj == null)
            {
                Log.Error($"[ProjectileFactory] Projectile prefab not found: {path}");
                return null;
            }
            return obj.GetComponent<Projectile>();
        }

        public static void Recycle(Projectile projectile)
        {
            if (projectile == null)
                return;
            projectile.RemoveAllAbilities();
            GameObject.Destroy(projectile.gameObject);
        }

        private static void AttachDefaultAbilities(Projectile projectile, ProjectileLevelConfig levelConfig)
        {
            var moveAbility = new ProjectileMoveAbility();
            moveAbility.MoveSpeed = levelConfig.MoveAbility.Speed;
            AttachCoreAbility(projectile, moveAbility);

            var damageAbility = new ProjectileDamageAbility();
            damageAbility.MaxPiercingCount = levelConfig.DamageAbility?.PiercingCount ?? 0;
            damageAbility.SourceMarble = projectile.RuntimeData.SourceMarbleInstId;
            damageAbility.IsDamageByVelocity = levelConfig.DamageAbility?.IsDamageByVelocity ?? false;
            AttachCoreAbility(projectile, damageAbility);

            var lifetimeAbility = new ProjectileLifetimeAbility();
            lifetimeAbility.MaxLifetime = levelConfig.Lifetime?.MaxLifetime ?? 0f;
            AttachCoreAbility(projectile, lifetimeAbility);

            void AttachCoreAbility(Projectile projectile, ProjectileAbility ability)
            {
                ability.Category = AbilityCategory.Core;
                projectile.AddAbility(ability);
            }
        }

        private static void AttachOptionalAbilities(Projectile projectile, ProjectileLevelConfig levelConfig)
        {
            if(levelConfig?.LstAbility == null)
                return;

            foreach (var config in levelConfig.LstAbility)
            {
                var ability = CreateAbilityFromConfig(config);
                if (ability != null)
                {
                    ability.Priority = config.Priority;
                    projectile.AddAbility(ability);
                }
            }
        }


        private readonly static System.Collections.Generic.List<IProjectileAbilityCreatorForConfig> _lstAbilityCreatorsForConfig = new ()
        {
            new DefaultProjectileAbilityCreatorForConfig(),
        };

        public static void RegisterAbilityCreatorForConfig(IProjectileAbilityCreatorForConfig creator)
        {
            _lstAbilityCreatorsForConfig.Add(creator);
            _lstAbilityCreatorsForConfig.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public static ProjectileAbility CreateAbilityFromConfig(ProjectileAbilityConfig config)
        {
            foreach (var creator in _lstAbilityCreatorsForConfig)
            {
                var ability = creator.CreateAbility(config);
                if (ability != null)
                {
                    return ability;
                }
            }
            Log.Error($"Projectile ability creator for config not found: {config.GetType().Name}");
            return null;
        }
    }

    public interface IProjectileAbilityCreatorForConfig
    {
        int Priority { get; set; }
        ProjectileAbility CreateAbility(ProjectileAbilityConfig config);
    }

    public class DefaultProjectileAbilityCreatorForConfig : IProjectileAbilityCreatorForConfig
    {
        public int Priority { get; set; } = int.MinValue;
        public ProjectileAbility CreateAbility(ProjectileAbilityConfig config)
        {
            return config switch
            {
                ProjectileNoTrackingConfig _=> new ProjectileNoTrackingAbility(),
                ProjectileTrackTargetConfig _=> new ProjectileTrackTargetAbility(),
                ProjectileTrackPointConfig _=> new ProjectileTrackPointAbility(),
                _ => null
            };
        }
    }
}
