namespace GameLogic.Marble
{
    public class MarbleAddDamageAbility : Ability<MarbleRuntimeData>
    {
        public void AddDamage(int value)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.RuntimeData.PendingDamage += value;
            Owner.GetAbility<MarbleHandleDamageAbility>()?.Resolve();
            Owner.GetAbility<MarbleDeadAbility>()?.Resolve();
        }
    }
}
