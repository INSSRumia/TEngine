using Cysharp.Threading.Tasks;
using UnityEngine;
using GameConfig.Gameplay.Combat;


namespace GameLogic.Gameplay.Combat.Equipment
{
    /// <summary>
    /// 弓射击能力。
    /// 负责把弓的发射参数转换为一个或多个 Projectile 创建请求，
    /// 但不直接处理发射物生命周期与命中逻辑，后续链路统一交给 ProjectileFactory 和 Projectile Ability。
    /// </summary>
    public class BowFireAbility : WeaponFireAbility
    {
        private const EnumBowShootType DefaultShootType = EnumBowShootType.Sequential;

        private BowEquipment _bowOwner;
        private Marble.Marble _sourceMarble;

        public string ProjectileConfigId {get; private set;}
        public int ProjectileLevel {get; private set;}
        public float ArrowInterval {get; private set;}
        public int ArrowCount {get; private set;}
        public float ArrowAngleStep {get; private set;}
        public EnumBowShootType ShootType {get; private set;}
        public BowFireAbility(BowFireAbilityConfig config)
        {
            ProjectileConfigId = config.ProjectileConfigId;
            ProjectileLevel = config.ProjectileLevel;
            ArrowInterval = config.ArrowInterval;
            ArrowCount = config.ArrowCount;
            ArrowAngleStep = config.ArrowAngleStep;
            ShootType = config.ShootType;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            if (_owner is BowEquipment bowEquipment)
                _bowOwner = bowEquipment;
            _sourceMarble = _bowOwner?.OwnerMarble;
        }

        protected override bool CanFire()
        {
            if (_bowOwner.RuntimeData.IsBroken)
                return false;
            return _bowOwner.RuntimeData.CanFire;
        }

        protected override async void DoFire()
        {
            if (_bowOwner.RuntimeData.IsBroken || _sourceMarble == null)
                return;

            var cooldownAbility = _bowOwner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return;

            if (string.IsNullOrEmpty(ProjectileConfigId))
                return;

            var spawnPosition = _bowOwner.transform.position;
            var forward = _bowOwner.transform.right;

            var target = _sourceMarble.CombatManager?.GetTarget(Owner.OwnerMarble.RuntimeData.State.TargetMarbleInstId);

            if (ShootType == EnumBowShootType.Spread)
            {
                // 散射模式以当前朝向为中心，左右交替铺开角度。
                var centerIndex = 0;
                for (var i = 0; i < ArrowCount; i++)
                {
                    var offsetIndex = i == 0 ? 0 : (i % 2 == 1 ? centerIndex + 1 : -(centerIndex + 1));
                    if (i % 2 == 0 && i > 0)
                        centerIndex++;
                    var angle = offsetIndex * ArrowAngleStep;
                    var rotation = Quaternion.Euler(0f, 0f, angle) * Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(forward.y, forward.x));
                    int damage = Owner.GetAbility<WeaponCalculateDamageAbility>().CalculateDamage();
                    ProjectileFactory.CreateProjectile(
                        ProjectileConfigId, ProjectileLevel,
                        _sourceMarble, target, default,
                        spawnPosition, rotation, damage);
                }
            }
            else
            {
                if (ShootType != DefaultShootType)
                    return;

                // 顺序发射模式会在多支箭之间等待 ArrowInterval，期间弓位置和朝向允许继续变化。
                for (var i = 0; i < ArrowCount; i++)
                {
                    if(Owner.RuntimeData.IsBroken)
                        break;

                    spawnPosition = Owner.transform.position;
                    forward = Owner.transform.right;

                    var angle = Mathf.Rad2Deg * Mathf.Atan2(forward.y, forward.x);
                    var rotation = Quaternion.Euler(0f, 0f, angle);
                    int damage = Owner.GetAbility<WeaponCalculateDamageAbility>().CalculateDamage();

                    ProjectileFactory.CreateProjectile(
                        ProjectileConfigId, ProjectileLevel,
                        _sourceMarble, target, default,
                        spawnPosition, rotation, damage);
                    await UniTask.Delay(System.TimeSpan.FromSeconds(ArrowInterval));
                }
            }
        }
    }
}
