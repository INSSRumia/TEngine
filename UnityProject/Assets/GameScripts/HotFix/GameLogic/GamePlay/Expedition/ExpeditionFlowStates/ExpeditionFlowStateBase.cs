using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public abstract class ExpeditionFlowStateBase : FsmState<ExpeditionFlowController>
    {
        protected ExpeditionFlowController Owner(IFsm<ExpeditionFlowController> fsm)
        {
            return fsm.Owner;
        }
    }
}
