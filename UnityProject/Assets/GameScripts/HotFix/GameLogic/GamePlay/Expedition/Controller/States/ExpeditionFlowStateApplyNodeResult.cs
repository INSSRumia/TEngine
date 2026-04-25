using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowStateApplyNodeResult : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            var owner = Owner(fsm);
            owner.SetPhase(EnumExpeditionFlowPhase.ApplyingNodeResult);
            owner.ApplyCurrentNodeResult();
        }

        protected override void OnUpdate(IFsm<ExpeditionFlowController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            if (Owner(fsm).ShouldEnterSettlement())
            {
                ChangeState<ExpeditionFlowStateSettlement>(fsm);
                return;
            }

            ChangeState<ExpeditionFlowStateEnterNode>(fsm);
        }
    }
}
