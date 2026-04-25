using System.Collections.Generic;
using System.Linq;
using TEngine;
using GameConfig.Gameplay.Combat;

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
                CombatSessionInstId = "combat_debug_session",
                NodeConfigId = "combat_debug_node",
                CombatEncounterConfigId = encounter.CombatEncounterConfigId,
                Title = encounter.Title,
                LstAlliedMarble = _persistentData.LstMarbles
                    .Where(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0)
                    .ToList(),
                LstEnemyMarble = new List<MarbleSpawnConfig>(encounter.EnemyMarbles),
            };

            GameModule.UI.CloseUI<ExpeditionMainUI>();
            return ExpeditionCombatSessionController.Instance.StartSession(request, OnDebugCombatCompleted);
        }

        public CombatSessionRequest BuildCombatSessionRequest()
        {
            var node = GetCurrentNode();
            var combatConfig = node == null ? null : ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterConfigId);
            if (combatConfig == null || CurrentRun == null)
                return null;

            return new CombatSessionRequest
            {
                CombatSessionInstId = $"{CurrentRun.ExpeditionInstId}_{combatConfig.CombatEncounterConfigId}",
                NodeConfigId = node.NodeConfigId,
                CombatEncounterConfigId = combatConfig.CombatEncounterConfigId,
                Title = combatConfig.Title,
                LstAlliedMarble = CurrentRun.MarbleSnapshots
                    .Where(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0)
                    .ToList(),
                LstEnemyMarble = new List<MarbleSpawnConfig>(combatConfig.EnemyMarbles),
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
