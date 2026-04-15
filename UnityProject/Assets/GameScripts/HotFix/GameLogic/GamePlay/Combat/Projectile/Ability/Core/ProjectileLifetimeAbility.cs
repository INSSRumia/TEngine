using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileLifetimeAbility : ProjectileAbility, IAbilityUpdate
    {
        public float MaxLifetime { get; private set; }
        public ProjectileLifetimeAbility(ProjectileLifetimeConfig config)
        {
            Priority = config?.Priority ?? 0;
            MaxLifetime = config?.MaxLifetime ?? 0f;
        }

        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (MaxLifetime <= 0f)
                return;

            Owner.RuntimeData.CurrentLifetime += elapseSeconds;
            if (Owner.RuntimeData.CurrentLifetime >= MaxLifetime)
            {
                Owner.RuntimeData.IsFinishedLifetime = true;
            }
        }
    }
}
