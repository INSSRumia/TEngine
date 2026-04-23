using System.Collections.Generic;
using System.Linq;
using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowController : Singleton<ExpeditionFlowController>
    {
        #region 常量/字段/属性

        private const string FsmName = "MinimalExpeditionFlow";

        private readonly ExpeditionPersistentDataStore _persistentData = new ExpeditionPersistentDataStore();

        public ExpeditionPersistentDataStore PersistentData => _persistentData;
        public ExpeditionRunState CurrentRun { get; private set; }
        public IFsm<ExpeditionFlowController> Fsm { get; private set; }

        public bool IsFlowRunning => CurrentRun != null && Fsm != null;

        #endregion

        #region 生命周期与入口

        protected override void OnInit()
        {
            _persistentData.EnsureInitialized();
            _ = ExpeditionCombatSessionController.Instance;
        }

        public void OpenEntryUi()
        {
            _persistentData.EnsureInitialized();
            GameModule.UI.ShowUIAsync<global::GameLogic.ExpeditionMainUI>();
        }

        public bool StartMinimalExpedition()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
            {
                return false;
            }

            _persistentData.EnsureInitialized();
            DestroyFsmIfNeeded();

            CurrentRun = ExpeditionConfigBridge.CreateConfiguredRun(_persistentData.Marbles, ExpeditionConstants.MinimalExpeditionId);
            if (CurrentRun == null)
            {
                Log.Warning("[ExpeditionFlowController] StartMinimalExpedition aborted because expedition config could not be resolved.");
                return false;
            }

            GameModule.UI.CloseUI<global::GameLogic.ExpeditionMainUI>();
            Fsm = GameModule.Fsm.CreateFsm(FsmName, this,
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

        #endregion

        #region UI 交互输入

        public ExpeditionTable.ExpeditionEventConfig GetCurrentEventNode()
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
            {
                return;
            }

            var eventNode = GetCurrentEventNode();
            if (!eventNode?.Options?.Any(option => option != null && option.OptionId == optionId) ?? true)
            {
                return;
            }

            CurrentRun.PendingEventOptionId = optionId;
        }

        public void SubmitCombatResult(CombatSessionResult result)
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.InCombat || result == null)
            {
                return;
            }

            CurrentRun.PendingCombatResult = result;
        }

        public void AcknowledgeSettlement()
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.Settling)
            {
                return;
            }

            CurrentRun.IsSettlementAcknowledged = true;
        }

        #endregion

        #region FSM/节点推进

        public void SetPhase(EnumExpeditionFlowPhase phase)
        {
            if (CurrentRun != null)
            {
                CurrentRun.Phase = phase;
            }
        }

        public bool HasPendingEventChoice()
        {
            return CurrentRun != null && !string.IsNullOrEmpty(CurrentRun.PendingEventOptionId);
        }

        public bool HasPendingCombatResult()
        {
            return CurrentRun?.PendingCombatResult != null;
        }

        public ExpeditionTable.ExpeditionRouteNodeConfig GetCurrentNode()
        {
            return CurrentRun?.GetCurrentNode();
        }

        public ExpeditionNodeRecord EnterCurrentNode()
        {
            var record = CurrentRun?.GetCurrentRecord();
            if (record != null)
            {
                record.Status = EnumExpeditionNodeProcessStatus.Entered;
            }

            return record;
        }

        public void ApplyCurrentNodeResult()
        {
            var node = GetCurrentNode();
            var record = CurrentRun?.GetCurrentRecord();
            if (node == null || record == null)
            {
                return;
            }

            switch (node.NodeType)
            {
                case ExpeditionTable.EnumExpeditionNodeType.Event:
                    ApplyEventNodeResult(node, record);
                    break;
                case ExpeditionTable.EnumExpeditionNodeType.Combat:
                    ApplyCombatNodeResult(record);
                    break;
            }

            record.Status = EnumExpeditionNodeProcessStatus.Resolved;
            CurrentRun.PendingEventOptionId = null;
            CurrentRun.PendingCombatResult = null;

            if (CurrentRun.AreAllPlayerMarblesDead())
            {
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
            }

            if (CurrentRun.EndReason == EnumExpeditionEndReason.None)
            {
                CurrentRun.CurrentNodeIndex++;
                if (CurrentRun.CurrentNodeIndex >= CurrentRun.Route.Count)
                {
                    CurrentRun.EndReason = EnumExpeditionEndReason.Victory;
                }
            }
        }

        public bool ShouldEnterSettlement()
        {
            return CurrentRun == null
                   || CurrentRun.EndReason != EnumExpeditionEndReason.None
                   || CurrentRun.CurrentNodeIndex >= CurrentRun.Route.Count;
        }

        #endregion

        #region Combat 桥接

        public bool StartCombatDebug()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
            {
                return false;
            }

            var encounter = ExpeditionConfigBridge.ResolveDebugCombatEncounter(ExpeditionConstants.MinimalExpeditionId);
            if (encounter == null)
            {
                Log.Warning("[ExpeditionFlowController] StartCombatDebug aborted because no combat encounter config was found.");
                return false;
            }

            var request = new CombatSessionRequest
            {
                SessionId = "combat_debug_session",
                NodeId = "combat_debug_node",
                CombatId = encounter.CombatEncounterId,
                Title = encounter.Title,
                AlliedMarbles = new List<MarblePersistentData?>(_persistentData.Marbles),
                EnemyMarbles = new List<ExpeditionTable.ExpeditionEnemyMarbleConfig>(encounter.EnemyMarbles),
            };

            GameModule.UI.CloseUI<global::GameLogic.ExpeditionMainUI>();
            return ExpeditionCombatSessionController.Instance.StartSession(request, OnDebugCombatCompleted);
        }

        public CombatSessionRequest BuildCombatSessionRequest()
        {
            var node = GetCurrentNode();
            var combatConfig = node == null ? null : ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterId);
            if (combatConfig == null || CurrentRun == null)
            {
                return null;
            }

            return new CombatSessionRequest
            {
                SessionId = $"{CurrentRun.RunId}_{combatConfig.CombatEncounterId}",
                NodeId = node.NodeId,
                CombatId = combatConfig.CombatEncounterId,
                Title = combatConfig.Title,
                AlliedMarbles = new List<MarblePersistentData?>(CurrentRun.MarbleSnapshots),
                EnemyMarbles = new List<ExpeditionTable.ExpeditionEnemyMarbleConfig>(combatConfig.EnemyMarbles),
            };
        }

        public bool StartCurrentCombatSession()
        {
            var request = BuildCombatSessionRequest();
            if (request == null)
            {
                return false;
            }

            return ExpeditionCombatSessionController.Instance.StartSession(request, SubmitCombatResult);
        }

        #endregion

        #region 结果应用与结算

        public void SettleCurrentRun()
        {
            if (CurrentRun == null)
            {
                return;
            }

            for (int i = 0; i < CurrentRun.MarbleSnapshots.Count; i++)
            {
                if (!CurrentRun.MarbleSnapshots[i].HasValue)
                {
                    continue;
                }

                var snapshot = CurrentRun.MarbleSnapshots[i].Value;
                _persistentData.SetMarble(snapshot);
            }

            _persistentData.Money += CurrentRun.TotalMoneyGained;
            CurrentRun.ResultSummary = BuildResultSummary(CurrentRun);
            _persistentData.LastResult = CurrentRun.ResultSummary;
        }

        private void ApplyEventNodeResult(ExpeditionTable.ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            var eventConfig = node == null ? null : ExpeditionConfigBridge.ResolveEvent(node.EventId);
            var option = eventConfig?.Options?.FirstOrDefault(item => item != null && item.OptionId == CurrentRun.PendingEventOptionId);
            if (option == null)
            {
                return;
            }

            var context = new ExpeditionEffectExecutionContext(CurrentRun, _persistentData, record);
            ExpeditionEffectFactory.ExecuteEffects(option.LstEffect, context);
            record.ChosenOptionId = option.OptionId;
            record.GainedMoney = context.AppliedMoneyDelta;
            record.EffectSummaries = context.SummaryLines.ToList();
            record.Summary = JoinSummaryLines(record.EffectSummaries);
        }

        private void ApplyCombatNodeResult(ExpeditionNodeRecord record)
        {
            var result = CurrentRun.PendingCombatResult;
            if (result == null)
            {
                return;
            }

            record.CombatResult = result;

            for (int i = 0; i < CurrentRun.MarbleSnapshots.Count; i++)
            {
                var snapshot = CurrentRun.MarbleSnapshots[i];
                if (!snapshot.HasValue)
                {
                    continue;
                }

                var marbleResult = result.MarbleResults.Find(item => item.HasValue && item.Value.PersistentId == snapshot.Value.PersistentId);
                if (!marbleResult.HasValue)
                {
                    continue;
                }

                CurrentRun.MarbleSnapshots[i] = marbleResult;
            }

            var node = GetCurrentNode();
            var combatConfig = node == null ? null : ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterId);
            var effectConfigs = result.IsVictory ? combatConfig?.LstVictoryEffect : combatConfig?.LstDefeatEffect;
            var context = new ExpeditionEffectExecutionContext(CurrentRun, _persistentData, record);
            ExpeditionEffectFactory.ExecuteEffects(effectConfigs, context);
            record.GainedMoney = context.AppliedMoneyDelta;
            record.EffectSummaries = context.SummaryLines.ToList();
            record.Summary = JoinSummaryLines(new[] { result.Summary }.Concat(record.EffectSummaries));

            if (!result.IsVictory)
            {
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
            }
        }

        private static string JoinSummaryLines(IEnumerable<string> summaries)
        {
            return string.Join("\n", summaries.Where(summary => !string.IsNullOrWhiteSpace(summary)));
        }

        private ExpeditionResultSummary BuildResultSummary(ExpeditionRunState runState)
        {
            return new ExpeditionResultSummary
            {
                ExpeditionId = runState.ExpeditionId,
                IsVictory = runState.EndReason == EnumExpeditionEndReason.Victory,
                EndReason = runState.EndReason,
                MoneyDelta = runState.TotalMoneyGained,
                MarbleSummaries = runState.MarbleSnapshots.Where(snapshot => snapshot.HasValue).Select(snapshot => new ExpeditionMarbleSummary
                {
                    PersistentId = snapshot.Value.PersistentId,
                    DisplayName = snapshot.Value.DisplayName,
                    CurrentHp = snapshot.Value.CurrentHp,
                    MaxHp = snapshot.Value.MaxHp,
                    Exp = snapshot.Value.Exp,
                    IsDead = snapshot.Value.IsDead,
                }).ToList(),
                NodeSummaries = runState.NodeRecords
                    .Where(record => !string.IsNullOrEmpty(record.Summary))
                    .Select(record => record.Summary)
                    .ToList(),
            };
        }

        #endregion

        #region 调试与清理

        private void OnDebugCombatCompleted(CombatSessionResult result)
        {
            Log.Info($"[ExpeditionFlowController] Combat debug complete. Victory:{result?.IsVictory}");
            OpenEntryUi();
        }

        internal void ReturnToEntry()
        {
            GameModule.UI.CloseUI<global::GameLogic.EventCardUI>();
            GameModule.UI.CloseUI<global::GameLogic.ExpeditionResultUI>();
            CurrentRun = null;
            OpenEntryUi();
        }

        private void DestroyFsmIfNeeded()
        {
            if (GameModule.Fsm.HasFsm<ExpeditionFlowController>(FsmName))
            {
                GameModule.Fsm.DestroyFsm<ExpeditionFlowController>(FsmName);
            }

            Fsm = null;
        }

        #endregion
    }
}
