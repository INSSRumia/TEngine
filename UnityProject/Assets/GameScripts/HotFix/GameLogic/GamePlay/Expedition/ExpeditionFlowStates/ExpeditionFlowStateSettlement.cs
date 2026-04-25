using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowStateSettlement : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            var owner = Owner(fsm);
            owner.SetPhase(EnumExpeditionFlowPhase.Settling);
            owner.SettleCurrentRun();
            GameModule.UI.ShowUIAsync<global::GameLogic.ExpeditionResultUI>();
        }

        protected override void OnUpdate(IFsm<ExpeditionFlowController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            if (Owner(fsm).CurrentRun != null && Owner(fsm).CurrentRun.IsSettlementAcknowledged)
            {
                ChangeState<ExpeditionFlowStateFinished>(fsm);
            }
        }
    }
}
