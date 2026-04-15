using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileMoveAbility : ProjectileAbility, IAbilityFixedUpdate
    {
        public float MoveSpeed { get; private set; }
        public ProjectileMoveAbility(ProjectileMoveConfig config)
        {
            MoveSpeed = config.Speed;
        }
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            var targetDir = Owner.RuntimeData.TargetDirection;
            if (targetDir.sqrMagnitude < 0.001f)
                return;

            Owner.Rigidbody.velocity = targetDir * MoveSpeed;

            // var angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            // Owner.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
