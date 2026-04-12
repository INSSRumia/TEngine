using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileNoTrackingAbility : ProjectileTrackingAbility
    {
        protected override Vector2 ResolveDirection(float elapseSeconds)
        {
            return GetCurrentDirection();
        }
    }
}
