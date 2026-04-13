using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowFireAbility : WeaponFireAbility
    {
        private BowEquipment _bowOwner;
        private Marble.Marble _sourceMarble;

        public override void OnAdd()
        {
            base.OnAdd();
            if (_owner is BowEquipment bowEquipment)
                _bowOwner = bowEquipment;
            _sourceMarble = _bowOwner?.OwnerMarble;
            if (_bowOwner?.RuntimeData != null)
                _fireInterval = _bowOwner.RuntimeData.ArrowInterval;
        }

        protected override bool CanFire()
        {
            if (_bowOwner == null || _bowOwner.RuntimeData == null)
                return false;
            return _bowOwner.RuntimeData.CanFire;
        }

        protected override async void DoFire()
        {
            if (_bowOwner == null || _bowOwner.RuntimeData == null || _sourceMarble == null)
                return;

            var cooldownAbility = _bowOwner.GetAbility<WeaponCooldownAbility>();
            if (cooldownAbility == null || !cooldownAbility.TryConsumeCooldown())
                return;

            var projectileConfigId = _bowOwner.RuntimeData.ProjectileConfigId;
            var projectileLevel = _bowOwner.RuntimeData.ProjectileLevel;
            if (string.IsNullOrEmpty(projectileConfigId))
                return;

            var spawnPosition = _bowOwner.transform.position;
            var forward = _bowOwner.transform.right;
            var arrowCount = Mathf.Max(1, _bowOwner.RuntimeData.ArrowCount);
            var angleStep = _bowOwner.RuntimeData.ArrowAngleStep;
            var shootType = _bowOwner.RuntimeData.ShootType;

            var target = _sourceMarble.CombatManager?.GetTarget(Owner.OwnerMarble.RuntimeData.TargetMarbleInstId);

            if (shootType == 1)
            {
                var centerIndex = 0;
                for (var i = 0; i < arrowCount; i++)
                {
                    var offsetIndex = i == 0 ? 0 : (i % 2 == 1 ? centerIndex + 1 : -(centerIndex + 1));
                    if (i % 2 == 0 && i > 0)
                        centerIndex++;
                    var angle = offsetIndex * angleStep;
                    var rotation = Quaternion.Euler(0f, 0f, angle) * Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(forward.y, forward.x));
                    int damage = Owner.GetAbility<WeaponCalculateDamageAbility>().CalculateDamage();
                    ProjectileFactory.CreateProjectile(
                        projectileConfigId, projectileLevel,
                        _sourceMarble, target, default,
                        spawnPosition, rotation, damage);
                }
            }
            else
            {
                for (var i = 0; i < arrowCount; i++)
                {
                    if(Owner.RuntimeData.IsBroken)
                        break;

                    spawnPosition = Owner.transform.position;
                    forward = Owner.transform.right;

                    var angle = Mathf.Rad2Deg * Mathf.Atan2(forward.y, forward.x);
                    var rotation = Quaternion.Euler(0f, 0f, angle);
                    int damage = Owner.GetAbility<WeaponCalculateDamageAbility>().CalculateDamage();

                    ProjectileFactory.CreateProjectile(
                        projectileConfigId, projectileLevel,
                        _sourceMarble, target, default,
                        spawnPosition, rotation, damage);
                    await UniTask.Delay(System.TimeSpan.FromSeconds(_bowOwner.RuntimeData.ArrowInterval));
                }
            }
        }
    }
}
