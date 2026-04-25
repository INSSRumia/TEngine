using System.Linq;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        public ExpeditionEventConfig GetCurrentEventNode()
        {
            var node = CurrentRun?.GetCurrentNode();
            return node == null ? null : ExpeditionConfigBridge.ResolveEvent(node.EventId);
        }

        public ExpeditionResultSummary GetDisplayableResult()
        {
            return CurrentRun?.ResultSummary ?? _persistentData.LastResult;
        }

        public void SubmitEventChoice(string optionId)
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.WaitingEventChoice)
                return;

            var eventNode = GetCurrentEventNode();
            if (!eventNode?.Options?.Any(option => option != null && option.OptionId == optionId) ?? true)
                return;

            CurrentRun.PendingEventOptionId = optionId;
        }

        public void SubmitCombatResult(CombatSessionResult result)
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.InCombat || result == null)
                return;

            CurrentRun.PendingCombatResult = result;
        }

        public void AcknowledgeSettlement()
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.Settling)
                return;

            CurrentRun.IsSettlementAcknowledged = true;
        }
    }
}
