using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
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
                case ExpeditionTable.EnumExpeditionNodeType.RandomEvent:
                    if (owner.PrepareCurrentRandomEventNode())
                        ChangeState<ExpeditionFlowStateEvent>(fsm);
                    else
                        ChangeState<ExpeditionFlowStateApplyNodeResult>(fsm);
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
}
