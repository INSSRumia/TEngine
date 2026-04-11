using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class BowRuntimeData : WeaponRuntimeData
    {
        public float RotateSpeed { get; set; }
        public int ShootType { get; set; }
        public int ArrowCount { get; set; }
        public float ArrowInterval { get; set; }
        public float ArrowAngleStep { get; set; }
        public float AimAngle { get; set; }
        public int TargetMarbleInstId { get; set; }
        public Vector2 AimDirection { get; set; }
        public bool CanFire { get; set; }
    }
}
