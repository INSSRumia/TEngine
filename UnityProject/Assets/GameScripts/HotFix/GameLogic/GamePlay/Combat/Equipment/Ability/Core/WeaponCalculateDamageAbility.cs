using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponCalculateDamageAbility : EquipmentAbility<WeaponEquipment>
    {
        private const float VELOCITY_DAMAGE_FACTOR = 1f;

        public int CalculateDamage()
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.RuntimeData.IsBroken)
                return 0;

            bool isUseOwnerAttack = EquipmentOwner.RuntimeData.Attack == null;
            int attack = isUseOwnerAttack ? EquipmentOwner.OwnerMarble.RuntimeData.Attack : EquipmentOwner.RuntimeData.Attack.Value;
            int attackAddition = EquipmentOwner.OwnerMarble.RuntimeData.AttackAddition;
            float attackMultiplier = EquipmentOwner.OwnerMarble.RuntimeData.AttackMultiplier;
            attack = Mathf.RoundToInt((attack + attackAddition) * attackMultiplier);

            return Mathf.Max(0, Mathf.RoundToInt(attack));
        }
    }
}
