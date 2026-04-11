using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleReceiveDamageAbility : Ability<Marble>, IReceiveDamage
    {
        public void ReceiveDamage(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.GetAbility<MarbleDamagePipelineAbility>()?.Execute(value, source);
        }
    }
}
