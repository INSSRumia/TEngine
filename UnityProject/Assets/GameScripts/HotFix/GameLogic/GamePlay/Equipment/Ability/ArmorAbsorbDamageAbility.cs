namespace GameLogic.Equipment
{
    public class ArmorAbsorbDamageAbility : EquipmentAbility<ArmorRuntimeData>
    {
        public int AbsorbDamage(int damage)
        {
            if (damage <= 0 || EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.RuntimeData.IsBroken)
                return damage;

            if (EquipmentOwner.RuntimeData.Hp <= 0)
                return damage;

            var remainHp = EquipmentOwner.RuntimeData.Hp - damage;
            if (remainHp > 0)
            {
                EquipmentOwner.RuntimeData.Hp = remainHp;
                return 0;
            }

            EquipmentOwner.RuntimeData.Hp = 0;
            EquipmentOwner.RuntimeData.IsBroken = true;
            return -remainHp;
        }
    }
}
