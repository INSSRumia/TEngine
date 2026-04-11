using UnityEngine;

namespace GameLogic.Equipment
{
    public class WeaponCalculateDamageAbility : EquipmentAbility<WeaponRuntimeData>
    {
        private const float VelocityDamageFactor = 1f;

        public int CalculateDamage(float relativeVelocity)
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.RuntimeData.IsBroken)
                return 0;

            bool isUseOwnerAttack = EquipmentOwner.RuntimeData.Attack == null;
            int attack = isUseOwnerAttack ? EquipmentOwner.OwnerMarble.RuntimeData.Attack : EquipmentOwner.RuntimeData.Attack.Value;
            attack = Mathf.RoundToInt((attack + EquipmentOwner.OwnerMarble.RuntimeData.AttackAddition) * EquipmentOwner.OwnerMarble.RuntimeData.AttackMultiplier);

            return Mathf.Max(0, Mathf.RoundToInt(relativeVelocity * VelocityDamageFactor * attack));
        }
    }
}
