namespace GameLogic.Gameplay.Combat
{
    public class ProjectileLifetimeAbility : ProjectileAbility, IAbilityUpdate
    {
        public float MaxLifetime { get; private set; }
        public ProjectileLifetimeAbility(float maxLifetime)
        {
            MaxLifetime = maxLifetime;
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
