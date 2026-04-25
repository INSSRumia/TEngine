namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        public void AddBlackboardFlag(string flagId)
        {
            CurrentRun?.Blackboard?.AddFlag(flagId);
        }

        public void AddBlackboardItem(string itemId)
        {
            CurrentRun?.Blackboard?.AddItem(itemId);
        }

        public void AddBlackboardCounter(string counterId, int delta)
        {
            CurrentRun?.Blackboard?.AddCounterValue(counterId, delta);
        }

        public void SetBlackboardCounter(string counterId, int value)
        {
            CurrentRun?.Blackboard?.SetCounterValue(counterId, value);
        }

        public void InsertNodeNext(string nodeConfigId, string reason)
        {
            CurrentRun?.InsertNodeAtFront(nodeConfigId, true, CurrentRun?.GetCurrentNode()?.NodeConfigId, string.Empty, reason);
        }

        public void ScheduleNodeInsertionAfterNode(string triggerNodeConfigId, string nodeConfigId, string reason, int priority = 0)
        {
            CurrentRun?.ScheduleInsertionAfterNode(triggerNodeConfigId, nodeConfigId, reason, priority);
        }
    }
}
