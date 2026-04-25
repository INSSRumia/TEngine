using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowStatePrepare : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            Owner(fsm).SetPhase(EnumExpeditionFlowPhase.Preparing);
        }

        protected override void OnUpdate(IFsm<ExpeditionFlowController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            ChangeState<ExpeditionFlowStateEnterNode>(fsm);
        }
    }
}
