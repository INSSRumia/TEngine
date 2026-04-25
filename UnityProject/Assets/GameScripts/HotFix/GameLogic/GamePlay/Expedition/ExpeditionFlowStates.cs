using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public abstract class ExpeditionFlowStateBase : FsmState<ExpeditionFlowController>
    {
        protected ExpeditionFlowController Owner(IFsm<ExpeditionFlowController> fsm)
        {
            return fsm.Owner;
        }
    }

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

    public sealed class ExpeditionFlowStateEnterNode : ExpeditionFlowStateBase
    {
        protected override void OnEnter(IFsm<ExpeditionFlowController> fsm)
        {
            var owner = Owner(fsm);
            owner.SetPhase(EnumExpeditionFlowPhase.EnteringNode);
            owner.EnterCurrentNode();
        }

        protected override void OnUpdate(IFsm<ExpeditionFlowController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            var owner = Owner(fsm);
            var node = owner.GetCurrentNode();
            if (node == null)
            {
                ChangeState<ExpeditionFlowStateSettlement>(fsm);
                return;
            }

            switch (node.NodeType)
            {
                case ExpeditionTable.EnumExpeditionNodeType.Event:
                    ChangeState<ExpeditionFlowStateEvent>(fsm);
                    break;
                case ExpeditionTable.EnumExpeditionNodeType.Combat:
                    ChangeState<ExpeditionFlowStateCombat>(fsm);
                    break;
                default:
                    ChangeState<ExpeditionFlowStateSettlement>(fsm);
                    break;
            }
        }
    }

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
