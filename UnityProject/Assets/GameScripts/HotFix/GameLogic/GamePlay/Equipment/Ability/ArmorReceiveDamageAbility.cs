using GameLogic.GamePlay.Common;
namespace GameLogic.Equipment
{
    public class ArmorReceiveDamageAbility : EquipmentAbility<ArmorRuntimeData>, IReceiveDamage
    {
        public void ReceiveDamage(int damage, ASC source = null)
        {
            if (damage <= 0 || EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.RuntimeData.IsBroken)
                return;

            damage -= EquipmentOwner.RuntimeData.Defense;
            if (damage <= 0)
                return;

            var remainHp = EquipmentOwner.RuntimeData.Hp - damage;
            if (remainHp > 0)
            {
                EquipmentOwner.RuntimeData.Hp = remainHp;
                return;
            }

            EquipmentOwner.RuntimeData.Hp = 0;
            EquipmentOwner.RuntimeData.IsBroken = true;
        }
    }
}
