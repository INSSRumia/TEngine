namespace GameLogic.Gameplay.Combat
{
    public class ProjectileAbility : Ability<Projectile>
    {
        public ProjectileAbility() : base(ProjectileFactory.GetNextInstAbilityId) { }
    }
}
