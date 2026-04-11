using GameLogic.Marble;
using GameLogic.GamePlay.Common;

namespace GameLogic.Equipment
{
    public class WeaponCooldownAbility : EquipmentAbility<WeaponEquipment>, IAbilityUpdate
    {
        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null)
                return;

            if (EquipmentOwner.RuntimeData.CooldownRemaining <= 0f)
                return;

            EquipmentOwner.RuntimeData.CooldownRemaining -= elapseSeconds;
            if (EquipmentOwner.RuntimeData.CooldownRemaining < 0f)
                EquipmentOwner.RuntimeData.CooldownRemaining = 0f;
        }

        public bool TryConsumeCooldown()
        {
            if (EquipmentOwner == null || EquipmentOwner.RuntimeData == null)
                return false;

            if (EquipmentOwner.RuntimeData.CooldownRemaining > 0f)
                return false;

            EquipmentOwner.RuntimeData.CooldownRemaining = EquipmentOwner.RuntimeData.Cooldown;
            return true;
        }
    }
}
