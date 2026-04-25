using System.Collections.Generic;
using System.Linq;
using TEngine;
using GameConfig.Gameplay.Expedition;

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
                Log.Warning("[远征流程控制器] StartMinimalExpedition 已中止，因为无法解析远征配置。");
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

        #region 黑板与动态插入能力

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

        public void InsertNodeNext(string nodeId, string reason)
        {
            CurrentRun?.InsertNodeAtFront(nodeId, true, CurrentRun?.GetCurrentNode()?.NodeId, string.Empty, reason);
        }

        public void ScheduleNodeInsertionAfterNode(string triggerNodeId, string nodeId, string reason, int priority = 0)
        {
            CurrentRun?.ScheduleInsertionAfterNode(triggerNodeId, nodeId, reason, priority);
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

        public ExpeditionRouteNodeConfig GetCurrentNode()
        {
            return CurrentRun?.GetCurrentNode();
        }

        public ExpeditionNodeRecord EnterCurrentNode()
        {
            return CurrentRun?.EnterNextPendingNode();
        }

        public void ApplyCurrentNodeResult()
        {
            var node = GetCurrentNode();
            var record = CurrentRun?.GetCurrentRecord();
            if (node == null || record == null)
            {
                return;
            }

            record.QueueBeforeRoute = CurrentRun.DescribeQueue();
            switch (node.NodeType)
            {
                case EnumExpeditionNodeType.Event:
                    ApplyEventNodeResult(node, record);
                    break;
                case EnumExpeditionNodeType.Combat:
                    ApplyCombatNodeResult(node, record);
                    break;
            }

            if (CurrentRun.AreAllPlayerMarblesDead())
            {
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
            }

            ApplyDynamicInsertions(record);
            ApplyRouteDecision(node, record);

            record.Status = EnumExpeditionNodeProcessStatus.Resolved;
            record.BlackboardAfter = CurrentRun.Blackboard?.ToDebugString() ?? string.Empty;
            record.QueueAfterRoute = CurrentRun.DescribeQueue();

            CurrentRun.PendingEventOptionId = null;
            CurrentRun.PendingCombatResult = null;
            CurrentRun.ClearCurrentNode();

            if (CurrentRun.EndReason == EnumExpeditionEndReason.None && !CurrentRun.HasPendingNodes())
            {
                CurrentRun.EndReason = EnumExpeditionEndReason.Victory;
            }
        }

        public bool ShouldEnterSettlement()
        {
            return CurrentRun == null
                   || CurrentRun.EndReason != EnumExpeditionEndReason.None
                   || !CurrentRun.HasPendingNodes();
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
                Log.Warning("[远征流程控制器] StartCombatDebug 已中止，因为未找到战斗遭遇配置。");
                return false;
            }

            var request = new CombatSessionRequest
            {
                SessionId = "combat_debug_session",
                NodeId = "combat_debug_node",
                CombatId = encounter.CombatEncounterId,
                Title = encounter.Title,
                AlliedMarbles = new List<MarblePersistentData?>(_persistentData.Marbles),
                EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>(encounter.EnemyMarbles),
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
                EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>(combatConfig.EnemyMarbles),
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
            CurrentRun.ResultSummary = ExpeditionResultSummary.BuildResultSummary(CurrentRun);
            _persistentData.LastResult = CurrentRun.ResultSummary;
        }

        private void ApplyEventNodeResult(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            var eventConfig = node == null ? null : ExpeditionConfigBridge.ResolveEvent(node.EventId);
            var option = eventConfig?.Options?.FirstOrDefault(item => item != null && item.OptionId == CurrentRun.PendingEventOptionId);
            if (option == null)
            {
                record.AddRouteDecisionLog($"节点 {node?.NodeId} 没有收到合法选项输入。");
                return;
            }

            var context = new ExpeditionEffectExecutionContext(CurrentRun, _persistentData, record);
            ExpeditionEffectFactory.ExecuteEffects(option.LstEffect, context);
            record.ChosenOptionId = option.OptionId;
            record.RecordEffectSummary(context.AppliedMoneyDelta, context.SummaryLines);

            CurrentRun.Blackboard?.AddChosenOption(option.OptionId);
            CurrentRun.Blackboard?.AddCompletedEvent(eventConfig.EventId);
        }

        private void ApplyCombatNodeResult(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            var result = CurrentRun.PendingCombatResult;
            if (result == null)
            {
                record.AddRouteDecisionLog($"节点 {node.NodeId} 没有收到 Combat 结果。");
                return;
            }

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

            var combatConfig = ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterId);
            var effectConfigs = result.IsVictory ? combatConfig?.LstVictoryEffect : combatConfig?.LstDefeatEffect;
            var context = new ExpeditionEffectExecutionContext(CurrentRun, _persistentData, record);
            ExpeditionEffectFactory.ExecuteEffects(effectConfigs, context);
            record.RecordCombatSummary(result, context.AppliedMoneyDelta, context.SummaryLines);

            if (!result.IsVictory)
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
        }

        private void ApplyDynamicInsertions(ExpeditionNodeRecord record)
        {
            if (CurrentRun == null || record == null)
                return;

            var insertedEntries = CurrentRun.TriggerScheduledInsertions(record.NodeId);
            if (insertedEntries.Count == 0)
            {
                record.AddRouteDecisionLog("当前节点没有触发动态插入。");
                return;
            }

            record.RecordInsertedNodeIds(insertedEntries.Select(entry => entry.NodeId));
        }

        private void ApplyRouteDecision(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            if (CurrentRun == null || node == null || record == null)
                return;

            if (CurrentRun.EndReason != EnumExpeditionEndReason.None)
            {
                record.AddRouteDecisionLog($"远征已结束，跳过节点 {node.NodeId} 的出口解析。");
                return;
            }

            var decision = ExpeditionRouteResolver.Resolve(CurrentRun, node, record);
            if (!string.IsNullOrWhiteSpace(decision?.Summary))
                record.AddRouteDecisionLog(decision.Summary);

            if (decision == null || string.IsNullOrWhiteSpace(decision.TargetNodeId))
                return;

            var enqueuedNode = CurrentRun.EnqueueNode(
                decision.TargetNodeId,
                false,
                node.NodeId,
                decision.TransitionId,
                "route_transition");
            record.ResolvedTransitionId = decision.TransitionId;
            record.NextNodeId = enqueuedNode?.NodeId;
        }


        #endregion

        #region 调试与清理

        private void OnDebugCombatCompleted(CombatSessionResult result)
        {
            Log.Info($"[远征流程控制器] 战斗调试完成。胜利:{result?.IsVictory}");
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
