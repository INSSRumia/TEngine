using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleDeadAbility : Ability<Marble>
    {
        public void Execute()
        {
            if (Owner.RuntimeData.IsAlive || Owner.RuntimeData.Hp > 0)
                return;
            //TODO: 处理死亡逻辑
            GameEvent.Send(EventDef.Combat.MarbleDie, Owner);
            Owner.RuntimeData.IsAlive = false;
        }
    }
}
