using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        public void SettleCurrentRun()
        {
            if (CurrentRun == null)
                return;

            for (int i = 0; i < CurrentRun.MarbleSnapshots.Count; i++)
            {
                if (!CurrentRun.MarbleSnapshots[i].HasValue)
                    continue;

                var snapshot = CurrentRun.MarbleSnapshots[i].Value;
                _persistentData.SetMarble(snapshot);
            }

            _persistentData.Money += CurrentRun.TotalMoneyGained;
            CurrentRun.ResultSummary = ExpeditionResultSummary.BuildResultSummary(CurrentRun);
            _persistentData.LastResult = CurrentRun.ResultSummary;
        }

        public void ReturnToEntry()
        {
            GameModule.UI.CloseUI<EventCardUI>();
            GameModule.UI.CloseUI<ExpeditionResultUI>();
            CurrentRun = null;
            OpenEntryUi();
        }

        private void DestroyFsmIfNeeded()
        {
            if (GameModule.Fsm.HasFsm<ExpeditionFlowController>(FsmName))
                GameModule.Fsm.DestroyFsm<ExpeditionFlowController>(FsmName);

            Fsm = null;
        }
    }
}
