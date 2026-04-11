namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorRuntimeData : EquipmentRuntimeData
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Defense { get; set; }
        public ArmorRuntimeData(string configId, 
            int instId, 
            EquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int hp, 
            int maxHp, 
            int defense) : base(configId, instId, slot, isEquipped, isBroken)
        {
            SetData(configId, instId, slot, isEquipped, isBroken, hp, maxHp, defense);
        }
        public void SetData(
            string configId, 
            int instId, 
            EquipmentSlot slot, 
            bool isEquipped, 
            bool isBroken, 
            int hp, 
            int maxHp, 
            int defense)
        {
            base.SetData(configId, instId, slot, isEquipped, isBroken);
            Hp = hp;
            MaxHp = maxHp;
            Defense = defense;
        }
    }
}
