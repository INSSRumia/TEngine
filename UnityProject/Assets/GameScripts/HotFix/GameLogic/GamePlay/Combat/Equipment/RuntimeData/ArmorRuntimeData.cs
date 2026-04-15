using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class ArmorRuntimeData : EquipmentRuntimeData
    {
        public int Hp { get; set; }
        public ArmorRuntimeData(EquipmentConfig config, ArmorLevelConfig levelConfig) : base(config, levelConfig)
        {
            if (levelConfig.Armor is ArmorAbsorbDamageAbilityConfig absorbDamageConfig)
            {
                Hp = absorbDamageConfig.Hp;
            }
        }
    }
}
