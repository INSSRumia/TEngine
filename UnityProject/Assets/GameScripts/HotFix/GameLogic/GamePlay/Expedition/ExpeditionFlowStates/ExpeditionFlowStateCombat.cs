using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowStateCombat : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            var owner = Owner(fsm);
            owner.SetPhase(EnumExpeditionFlowPhase.InCombat);
            if (!owner.StartCurrentCombatSession())
            {
                owner.SubmitCombatResult(new CombatSessionResult
                {
                    IsVictory = false,
                    Summary = "Combat 会话启动失败，远征按失败结算。"
                });
            }
        }

        protected override void OnUpdate(IFsm<ExpeditionFlowController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            if (Owner(fsm).HasPendingCombatResult())
            {
                ChangeState<ExpeditionFlowStateApplyNodeResult>(fsm);
            }
        }

        protected override void OnLeave(IFsm<ExpeditionFlowController> fsm, bool isShutdown)
        {
            ExpeditionCombatSessionController.Instance.ClearSession();
        }
    }
}
