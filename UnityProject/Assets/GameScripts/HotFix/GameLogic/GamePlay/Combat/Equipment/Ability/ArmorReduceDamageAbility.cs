using GameLogic.GamePlay.Combat;
namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorReduceDamageAbility : EquipmentAbility, IReceiveDamage
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
            if (damage <= 0 || _owner == null || _owner.RuntimeData == null || _owner.OwnerMarble == null)
                return;

            damage -= _owner.RuntimeData.Defense;
            if(damage <= 0)
                return;

            _owner.OwnerMarble.GetAbility<IReceiveDamage>()?.ReceiveDamage(damage, source ?? _owner);
        }
    }
}
