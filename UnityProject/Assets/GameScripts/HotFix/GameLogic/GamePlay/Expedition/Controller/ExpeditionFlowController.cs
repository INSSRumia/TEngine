using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController : Singleton<ExpeditionFlowController>
    {
        private const string FsmName = "MinimalExpeditionFlow";

        private readonly ExpeditionPersistentDataStore _persistentData = new ExpeditionPersistentDataStore();

        public ExpeditionPersistentDataStore PersistentData => _persistentData;

        public ExpeditionRunState CurrentRun { get; private set; }

        public IFsm<ExpeditionFlowController> Fsm { get; private set; }

        public bool IsFlowRunning => CurrentRun != null && Fsm != null;
    }
}
