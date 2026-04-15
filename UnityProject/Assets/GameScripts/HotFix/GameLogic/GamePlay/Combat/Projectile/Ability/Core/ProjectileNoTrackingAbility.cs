using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileNoTrackingAbility : ProjectileTrackingAbility
    {
        public ProjectileNoTrackingAbility(ProjectileNoTrackingConfig config) : base(config, 0f)
        {
        }

        protected override Vector2 ResolveDirection(float elapseSeconds)
        {
            return GetCurrentDirection();
        }
    }
}
