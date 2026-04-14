using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileNoTrackingAbility : ProjectileTrackingAbility
    {
        public ProjectileNoTrackingAbility() : base(0f)
        {
        }

        protected override Vector2 ResolveDirection(float elapseSeconds)
        {
            return GetCurrentDirection();
        }
    }
}
