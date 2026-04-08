namespace GameLogic.Marble
{
    public class MarbleDeadAbility : Ability<MarbleRuntimeData>
    {
        public override int Priority => 9800;

        public void Resolve()
        {
            if (Owner.RuntimeData.IsAlive || Owner.RuntimeData.Hp > 0)
                return;
            //TODO: 处理死亡逻辑
            GameEvent.Send(EventDef.Combat.MarbleDie, Owner as Marble);
            Owner.RuntimeData.IsAlive = false;
        }
    }
}
