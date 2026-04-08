using UnityEngine;

namespace GameLogic.Equipment
{
    public class WeaponCollisionDamageAbility : EquipmentAbility<WeaponRuntimeData>
    {
        private const float VelocityDamageFactor = 1f;

        public int CalculateDamage(float relativeVelocity)
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.RuntimeData.IsBroken)
                return 0;

            var attack = EquipmentOwner.RuntimeData.Attack;
            if (!EquipmentOwner.RuntimeData.IsDamageByVelocity)
                return attack;
            

            return Mathf.Max(0, Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * attack));
        }
    }
}
