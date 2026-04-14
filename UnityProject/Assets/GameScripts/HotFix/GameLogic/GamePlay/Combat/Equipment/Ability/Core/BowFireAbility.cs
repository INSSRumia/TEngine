using Cysharp.Threading.Tasks;
using UnityEngine;


namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowFireAbility : WeaponFireAbility
    {
        private BowEquipment _bowOwner;
        private Marble.Marble _sourceMarble;

        public string ProjectileConfigId {get; private set;}
        public int ProjectileLevel {get; private set;}
        public float ArrowInterval {get; private set;}
        public int ArrowCount {get; private set;}
        public float ArrowAngleStep {get; private set;}
        public int ShootType {get; private set;}
        public BowFireAbility(string projectileConfigId, int projectileLevel, float arrowInterval, int arrowCount, float arrowAngleStep, int shootType)
        {
            ProjectileConfigId = projectileConfigId;
            ProjectileLevel = projectileLevel;
            ArrowInterval = arrowInterval;
            ArrowCount = arrowCount;
            ArrowAngleStep = arrowAngleStep;
            ShootType = shootType;
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

            var target = _sourceMarble.CombatManager?.GetTarget(Owner.OwnerMarble.RuntimeData.TargetMarbleInstId);

            if (ShootType == 1)
            {
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
