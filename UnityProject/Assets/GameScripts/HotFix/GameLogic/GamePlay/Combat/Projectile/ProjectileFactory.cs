using TEngine;
using UnityEngine;
using GameConfig.Gameplay.Combat;
namespace GameLogic.Gameplay.Combat
{
    public static class ProjectileFactory
    {
        private static int _instIdCounter = 0;
        public static int GetNextInstId => _instIdCounter++;

        private static readonly string _path = Utility.Path.GetRegularPath("Assets/AssetRaw/Actor/Prefabs/Projectiles/");

        public static Projectile Spawn(
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

            string path = _path + configId;
            var obj = GameModule.Resource.LoadGameObject(path);
            if (obj == null)
                return null;

            var instance = Object.Instantiate(obj, spawnPosition, rotation);
            var projectile = instance.GetComponent<Projectile>();

            if (projectile == null)
            {
                Log.Error($"[ProjectileFactory] Projectile component not found on prefab: {path}");
                Object.Destroy(instance);
                return null;
            }

            var runtimeData = new ProjectileRuntimeData(configId, GetNextInstId)
            {
                SourceCamp = sourceMarble.RuntimeData.Camp,
                SourceMarbleInstId = sourceMarble.RuntimeData.InstId,
                TargetMarbleInstId = targetMarble?.RuntimeData?.InstId ?? 0,
                TargetPoint = targetPoint,
                CurrentLifetime = 0f,
                Damage = damage,
                StartPosition = spawnPosition,
                TargetDirection = rotation * Vector2.up,
            };

            projectile.Init(runtimeData);

            AttachCoreAbilities(projectile, levelConfig);

            foreach (var abilityConfig in levelConfig.LstAbility)
            {
                var ability = CreateAbilityFromConfig(abilityConfig);
                if (ability != null)
                    projectile.AddAbility(ability);
            }

            return projectile;
        }

        public static void Despawn(Projectile projectile)
        {
            if (projectile == null)
                return;
            projectile.Despawn();
        }

        private static void AttachCoreAbilities(Projectile projectile, ProjectileLevelConfig levelConfig)
        {
            var moveAbility = new ProjectileMoveAbility();
            projectile.AddAbility(moveAbility);
            AttachTrackingAbility(projectile, levelConfig.TrackingAbility);

            var damageAbility = new ProjectileDamageAbility();
            damageAbility.MaxPiercingCount = levelConfig.DamageAbility?.PiercingCount ?? 0;
            damageAbility.SourceMarble = projectile.RuntimeData.SourceMarbleInstId;
            projectile.AddAbility(damageAbility);

            var lifetimeAbility = new ProjectileLifetimeAbility();
            lifetimeAbility.MaxLifetime = levelConfig.Lifetime?.MaxLifetime ?? 0f;
            projectile.AddAbility(lifetimeAbility);
        }

        private static void AttachTrackingAbility(Projectile projectile, ProjectileTrackConfig trackConfig)
        {
            ProjectileTrackingAbility trackingAbility =  (trackConfig.TrackingType) switch
            {
                EnumProjectileTrackingType.Target => new ProjectileTrackTargetAbility(),
                EnumProjectileTrackingType.Point => new ProjectileTrackPointAbility(),
                _ => new ProjectileNoTrackingAbility(),
            };

            trackingAbility.RotateSpeed = trackConfig.AngularSpeed;
            projectile.AddAbility(trackingAbility);
        }

        private static Ability<Projectile> CreateAbilityFromConfig(ProjectileAbilityConfig config)
        {
            return config switch
            {
                _ => null
            };
        }
    }
}
