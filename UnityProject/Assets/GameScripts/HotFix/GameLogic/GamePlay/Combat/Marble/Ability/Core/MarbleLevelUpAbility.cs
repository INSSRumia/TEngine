using System.Collections.Generic;
using GameLogic.GamePlay.Combat;
using GameLogic.Gameplay.Combat.Equipment;
using GameConfig.GameConfig;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleLevelUpAbility : Ability<Marble>
    {
        public void Resolve()
        {
            if (Owner == null || Owner.RuntimeData == null)
                return;
            if (!Owner.RuntimeData.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            var upgradeExp = runtimeData.UpgradeExp;
            if (upgradeExp <= 0)
                return;

            var curExp = runtimeData.Exp;
            if (curExp < upgradeExp)
                return;

            var nextLevel = runtimeData.Level + 1;
            var nextLevelData = MarbleFactory.GetMarbleLevelConfig(runtimeData.ConfigId, nextLevel);
            if (nextLevelData == null)
            {
                runtimeData.UpgradeExp = 0;
                return;
            }

            runtimeData.Exp = curExp - upgradeExp;
            runtimeData.Level = nextLevel;
            runtimeData.UpgradeExp = nextLevelData.UpgradeExp;

            runtimeData.MaxHp = nextLevelData.Hp;
            runtimeData.Hp = nextLevelData.Hp;
            runtimeData.MaxShield = nextLevelData.Shield;
            runtimeData.Shield = nextLevelData.Shield;
            runtimeData.Defense = nextLevelData.Defense;
            runtimeData.Attack = nextLevelData.Attack;
            runtimeData.Scale = nextLevelData.Scale;
            runtimeData.Mass = nextLevelData.Mass;
            runtimeData.TargetVelocity = nextLevelData.Speed;

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

            var currentSlots = new HashSet<EquipmentSlot>();
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
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
                var slot = (EquipmentSlot)equipConfig.Slot;

                var equipmentConfig = ConfigSystem.Instance.Tables.TbEquipment.Get(configId);
                if (equipmentConfig != null)
                {
                    EquipmentFactory.CreateEquipment(Owner, equipmentConfig, level, slot);
                }
            }
        }
    }
}
