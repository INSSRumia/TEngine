namespace GameLogic.Equipment
{
    public class ArmorReduceDamageAbility : EquipmentAbility<ArmorRuntimeData>
    {
        public int ReduceDamage(int damage)
        {
            if (damage <= 0 || EquipmentOwner == null || EquipmentOwner.RuntimeData == null || EquipmentOwner.RuntimeData.IsBroken)
                return damage;

            if (EquipmentOwner.RuntimeData.Hp > 0)
                return damage;

            var reduceValue = EquipmentOwner.RuntimeData.Defense;
            return damage - reduceValue < 0 ? 0 : damage - reduceValue;
        }
    }
}
