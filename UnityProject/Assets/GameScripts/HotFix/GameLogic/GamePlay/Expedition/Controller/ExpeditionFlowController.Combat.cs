using System.Collections.Generic;
using TEngine;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        public bool StartCombatDebug()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
                return false;

            var encounter = ExpeditionConfigBridge.ResolveDebugCombatEncounter(ExpeditionConstants.MinimalExpeditionId);
            if (encounter == null)
            {
                Log.Warning("[远征流程控制器] StartCombatDebug 已中止，因为未找到战斗遭遇配置。");
                return false;
            }

            var request = new CombatSessionRequest
            {
                SessionId = "combat_debug_session",
                NodeId = "combat_debug_node",
                CombatId = encounter.CombatEncounterId,
                Title = encounter.Title,
                LstAlliedMarble = new List<MarblePersistentData?>(_persistentData.LstMarbles),
                LstEnemyMarble = new List<ExpeditionEnemyMarbleConfig>(encounter.EnemyMarbles),
            };

            GameModule.UI.CloseUI<ExpeditionMainUI>();
            return ExpeditionCombatSessionController.Instance.StartSession(request, OnDebugCombatCompleted);
        }

        public CombatSessionRequest BuildCombatSessionRequest()
        {
            var node = GetCurrentNode();
            var combatConfig = node == null ? null : ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterId);
            if (combatConfig == null || CurrentRun == null)
                return null;

            return new CombatSessionRequest
            {
                SessionId = $"{CurrentRun.RunId}_{combatConfig.CombatEncounterId}",
                NodeId = node.NodeId,
                CombatId = combatConfig.CombatEncounterId,
                Title = combatConfig.Title,
                LstAlliedMarble = new List<MarblePersistentData?>(CurrentRun.MarbleSnapshots),
                LstEnemyMarble = new List<ExpeditionEnemyMarbleConfig>(combatConfig.EnemyMarbles),
            };
        }

        public bool StartCurrentCombatSession()
        {
            var request = BuildCombatSessionRequest();
            if (request == null)
                return false;

            return ExpeditionCombatSessionController.Instance.StartSession(request, SubmitCombatResult);
        }

        private void OnDebugCombatCompleted(CombatSessionResult result)
        {
            Log.Info($"[远征流程控制器] 战斗调试完成。胜利:{result?.IsVictory}");
            OpenEntryUi();
        }
    }
}
