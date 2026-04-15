using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorAbsorbDamageAbility : EquipmentAbility, IReceiveDamage
    {
        private ArmorEquipment _owner;
        public int Defense {get; private set;}
        public int MaxHp {get; private set;}
        public ArmorAbsorbDamageAbility(ArmorAbsorbDamageAbilityConfig config)
        {
            Defense = config.Defense;
            MaxHp = config.Hp;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is ArmorEquipment armorEquipment)
                _owner = armorEquipment;
            _owner.RuntimeData.Hp = MaxHp;
        }
        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }
        public void ReceiveDamage(int damage, ASC source = null)
        {
            if (damage <= 0 || _owner.RuntimeData.IsBroken)
                return;

            damage -= Defense;
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
