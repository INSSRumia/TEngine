using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleAddExpAbility : Ability<Marble>
    {
        public void AddExp(int value)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.RuntimeData.Exp += value;
            Owner.GetAbility<MarbleLevelUpAbility>()?.Resolve();
        }
    }
}
