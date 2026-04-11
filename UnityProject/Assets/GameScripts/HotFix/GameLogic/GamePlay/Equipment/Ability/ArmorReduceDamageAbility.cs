using GameLogic.GamePlay.Common;
namespace GameLogic.Equipment
{
    public class ArmorReduceDamageAbility : EquipmentAbility<ArmorEquipment>, IReceiveDamage
    {
        public void ReceiveDamage(int damage, ASC source = null)
        {
            if (damage <= 0 || EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.OwnerMarble == null)
                return;

            damage -= EquipmentOwner.RuntimeData.Defense;
            if(damage <= 0)
                return;

            EquipmentOwner.OwnerMarble.GetAbility<IReceiveDamage>()?.ReceiveDamage(damage, source ?? EquipmentOwner);
        }
    }
}
