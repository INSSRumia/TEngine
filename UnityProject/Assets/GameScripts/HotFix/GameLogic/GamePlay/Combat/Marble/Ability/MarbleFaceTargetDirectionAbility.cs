using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleFaceTargetDirectionAbility : TimedMarbleAbility, IAbilityFixedUpdate
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;
        private const float MinAngleThreshold = 0.1f;
        private const float FullTurnAngle = 180f;

        public Vector2 TargetLocalDirection { get; set; } = Vector2.right;
        public float TargetAngularSpeed { get; set; }
        public float AngularAcceleration { get; set; }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (!IsActive)
                return;

            var target = Owner.CombatManager?.GetTarget(Owner.RuntimeData.TargetMarbleInstId);
            if (target == null)
                return;

            Vector2 directionToTarget = target.transform.position - Owner.transform.position;

            Vector2 facingDirection = ResolveFacingDirection();

            float signedAngle = Vector2.SignedAngle(facingDirection, directionToTarget.normalized);
            if (Mathf.Abs(signedAngle) < MinAngleThreshold)
                return;

            // float normalizedAngle = Mathf.Clamp01(Mathf.Abs(signedAngle) / FullTurnAngle);
            // float angularSpeed = Mathf.Lerp(0f, Mathf.Abs(TargetAngularSpeed), normalizedAngle) * Mathf.Sign(signedAngle);
            // float angularAcceleration = Mathf.Lerp(0f, Mathf.Abs(AngularAcceleration), normalizedAngle);
            float angularSpeed = signedAngle * AngularAcceleration;
            float angularAcceleration = AngularAcceleration;

            Owner.RuntimeData.TargetAngularVelocityManager.Add(new PriorityValue<float>(InstId, angularSpeed, Priority, CombineType));
            Owner.RuntimeData.AngularAccelerationManager.Add(new PriorityValue<float>(InstId, angularAcceleration, Priority, CombineType));
        }

        private Vector2 ResolveFacingDirection()
        {
            Vector2 localDirection = TargetLocalDirection;
            if (localDirection.sqrMagnitude < MinDirectionSqrMagnitude)
                localDirection = Vector2.right;

            return Owner.transform.TransformDirection(localDirection.normalized);
        }
    }
}
