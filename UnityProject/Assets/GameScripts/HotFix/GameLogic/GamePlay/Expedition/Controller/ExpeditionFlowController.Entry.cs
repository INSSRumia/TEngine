using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        protected override void OnInit()
        {
            _persistentData.EnsureInitialized();
            _ = ExpeditionCombatSessionController.Instance;
        }

        public void OpenEntryUi()
        {
            _persistentData.EnsureInitialized();
            GameModule.UI.ShowUIAsync<ExpeditionMainUI>();
        }

        public bool StartMinimalExpedition()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
                return false;

            _persistentData.EnsureInitialized();
            DestroyFsmIfNeeded();

            CurrentRun = ExpeditionConfigBridge.CreateConfiguredRun(_persistentData.LstMarbles, ExpeditionConstants.MinimalExpeditionId);
            if (CurrentRun == null)
            {
                Log.Warning("[远征流程控制器] StartMinimalExpedition 已中止，因为无法解析远征配置。");
                return false;
            }

            GameModule.UI.CloseUI<ExpeditionMainUI>();
            Fsm = GameModule.Fsm.CreateFsm(
                FsmName,
                this,
                new ExpeditionFlowStatePrepare(),
                new ExpeditionFlowStateEnterNode(),
                new ExpeditionFlowStateEvent(),
                new ExpeditionFlowStateCombat(),
                new ExpeditionFlowStateApplyNodeResult(),
                new ExpeditionFlowStateSettlement(),
                new ExpeditionFlowStateFinished());
            Fsm.Start<ExpeditionFlowStatePrepare>();
            return true;
        }
    }
}
