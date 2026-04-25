using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowStateFinished : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            var owner = Owner(fsm);
            owner.SetPhase(EnumExpeditionFlowPhase.Finished);
            owner.ReturnToEntry();
        }
    }
}
