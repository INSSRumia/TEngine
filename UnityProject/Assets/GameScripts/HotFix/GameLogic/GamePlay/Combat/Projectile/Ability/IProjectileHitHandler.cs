namespace GameLogic.Gameplay.Combat
{
    public interface IProjectileHitHandler
    {
        void HandleHit(ProjectileHitContext context);
    }
}
