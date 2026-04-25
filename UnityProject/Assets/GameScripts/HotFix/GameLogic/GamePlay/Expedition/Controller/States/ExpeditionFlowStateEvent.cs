using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowStateEvent : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            Owner(fsm).SetPhase(EnumExpeditionFlowPhase.WaitingEventChoice);
            GameModule.UI.ShowUIAsync<global::GameLogic.EventCardUI>();
        }

        protected override void OnUpdate(IFsm<ExpeditionFlowController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            if (Owner(fsm).HasPendingEventChoice())
            {
                ChangeState<ExpeditionFlowStateApplyNodeResult>(fsm);
            }
        }

        protected override void OnLeave(IFsm<ExpeditionFlowController> fsm, bool isShutdown)
        {
            GameModule.UI.CloseUI<global::GameLogic.EventCardUI>();
        }
    }
}
