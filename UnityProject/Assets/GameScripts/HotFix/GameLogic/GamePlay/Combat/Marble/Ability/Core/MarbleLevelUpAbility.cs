using System.Collections.Generic;
using GameLogic.Gameplay.Combat;
using GameLogic.Gameplay.Combat.Equipment;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleLevelUpAbility : MarbleAbility
    {
        private GameConfig.Gameplay.Combat.MarbleLevelConfig _currentLevelConfig;

        public MarbleLevelUpAbility(GameConfig.Gameplay.Combat.MarbleLevelUpAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            if (Owner?.RuntimeData != null)
            {
                _currentLevelConfig = MarbleFactory.GetMarbleLevelConfig(Owner.RuntimeData.ConfigId, Owner.RuntimeData.Level);
            }
        }

        public void Resolve()
        {
            if (Owner == null || Owner.RuntimeData == null)
                return;
            if (!Owner.RuntimeData.State.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            var upgradeExp = runtimeData.Config.UpgradeExp;
            if (upgradeExp <= 0)
                return;

            var curExp = runtimeData.State.Exp;
            if (curExp < upgradeExp)
                return;

            var nextLevel = runtimeData.Level + 1;
            var nextLevelData = MarbleFactory.GetMarbleLevelConfig(runtimeData.ConfigId, nextLevel);
            if (nextLevelData == null)
            {
                runtimeData.Config.UpgradeExp = 0;
                return;
            }

            runtimeData.State.Exp = curExp - upgradeExp;
            runtimeData.Level = nextLevel;
            runtimeData.ApplyLevelConfig(nextLevelData);
            _currentLevelConfig = nextLevelData;

            Owner.GetAbility<MarbleSyncScaleAbility>()?.Sync();
            Owner.GetAbility<MarbleSyncMassAbility>()?.Sync();
            UpdateEquipmentOnLevelUp(nextLevel);
            UpdateOptionalAbilitiesOnLevelUp(nextLevel);
        }

        private void UpdateOptionalAbilitiesOnLevelUp(int newLevel)
        {
            if (Owner == null)
                return;

            Owner.ClearOptionalAbilities();

            var levelConfig = MarbleFactory.GetMarbleLevelConfig(Owner.RuntimeData.ConfigId, newLevel);
            if (levelConfig?.LstAbility == null)
                return;

            foreach (var config in levelConfig.LstAbility)
            {
                var ability = GameLogic.Gameplay.Combat.Marble.MarbleFactory.CreateAbilityFromConfig(config);
                if (ability != null)
                {
                    ability.Priority = config.Priority;
                    Owner.AddAbility(ability);
                }
            }
        }

        private void UpdateEquipmentOnLevelUp(int newLevel)
        {
            if (Owner == null)
                return;

            var levelConfig = MarbleFactory.GetMarbleLevelConfig(Owner.RuntimeData.ConfigId, newLevel);
            if (levelConfig == null || levelConfig.LstEquipment == null)
                return;

            var currentSlots = new HashSet<EnumEquipmentSlot>();
            foreach (EnumEquipmentSlot slot in System.Enum.GetValues(typeof(EnumEquipmentSlot)))
            {
                var equipment = Owner.GetEquipment(slot);
                if (equipment != null)
                {
                    currentSlots.Add(slot);
                }
            }

            foreach (var slot in currentSlots)
            {
                Owner.DestroyEquipment(slot);
            }

            foreach (var equipConfig in levelConfig.LstEquipment)
            {
                var configId = equipConfig.ConfigId;
                var level = equipConfig.Level;
                var slot = (EnumEquipmentSlot)equipConfig.Slot;

                var equipmentConfig = ConfigSystem.Instance.Tables.TbEquipment.Get(configId);
                if (equipmentConfig != null)
                {
                    EquipmentFactory.CreateEquipment(Owner, equipmentConfig, level, slot);
                }
            }
        }
    }
}
