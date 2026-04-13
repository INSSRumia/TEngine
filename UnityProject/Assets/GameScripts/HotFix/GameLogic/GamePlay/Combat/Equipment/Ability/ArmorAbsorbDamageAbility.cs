namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorAbsorbDamageAbility : EquipmentAbility, IReceiveDamage
    {
        private ArmorEquipment _owner;
        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is ArmorEquipment armorEquipment)
                _owner = armorEquipment;
        }
        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }
        public void ReceiveDamage(int damage, ASC source = null)
        {
            if (damage <= 0 || _owner == null || _owner.RuntimeData == null || _owner.RuntimeData.IsBroken)
                return;

            damage -= _owner.RuntimeData.Defense;
            if (damage <= 0)
                return;

            var remainHp = _owner.RuntimeData.Hp - damage;
            if (remainHp > 0)
            {
                _owner.RuntimeData.Hp = remainHp;
                return;
            }

            _owner.RuntimeData.Hp = 0;
            _owner.RuntimeData.IsBroken = true;
        }
    }
}
