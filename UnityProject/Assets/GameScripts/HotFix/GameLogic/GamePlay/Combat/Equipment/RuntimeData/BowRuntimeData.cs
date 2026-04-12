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
        public BowRuntimeData(string configId, 
            int instId, 
            EnumEquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int? attack, 
            bool isDamageByVelocity, 
            float cooldown, 
            float rotateSpeed, 
            int shootType, 
            int arrowCount, 
            float arrowInterval, 
            float arrowAngleStep, 
            float aimAngle, 
            int targetMarbleInstId, 
            Vector2 aimDirection, 
            bool canFire) : base(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown)
        {
            SetData(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown, 0, rotateSpeed, shootType, arrowCount, arrowInterval, arrowAngleStep, aimAngle, targetMarbleInstId, aimDirection, canFire);
        }
        public void SetData(string configId, 
            int instId, 
            EnumEquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int? attack, 
            bool isDamageByVelocity, 
            float cooldown, 
            float cooldownRemaining,
            float rotateSpeed, 
            int shootType, 
            int arrowCount, 
            float arrowInterval, 
            float arrowAngleStep, 
            float aimAngle, 
            int targetMarbleInstId, 
            Vector2 aimDirection, 
            bool canFire)
        {
            base.SetData(configId, instId, slot, isEquipped, isBroken, attack, isDamageByVelocity, cooldown, cooldownRemaining);
            RotateSpeed = rotateSpeed;
            ShootType = shootType;
            ArrowCount = arrowCount;
            ArrowInterval = arrowInterval;
            ArrowAngleStep = arrowAngleStep;
            AimAngle = aimAngle;
            TargetMarbleInstId = targetMarbleInstId;
            AimDirection = aimDirection;
            CanFire = canFire;
        }
    }
}
