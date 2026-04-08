namespace GameLogic.Marble
{
    public class MarbleLevelUpAbility : Ability<MarbleRuntimeData>
    {
        public override int Priority => 9700;

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
            runtimeData.Scale = nextLevelData.Scale;
            runtimeData.Mass = nextLevelData.Mass;
            runtimeData.TargetVelocity = nextLevelData.Speed;

            Owner.GetAbility<MarbleSyncScaleAbility>()?.Sync();
            Owner.GetAbility<MarbleSyncMassAbility>()?.Sync();
        }
    }
}
