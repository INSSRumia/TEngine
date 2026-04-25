using System.Linq;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public partial class ExpeditionFlowController
    {
        public void SetPhase(EnumExpeditionFlowPhase phase)
        {
            if (CurrentRun != null)
                CurrentRun.Phase = phase;
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
                return;

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
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;

            ApplyDynamicInsertions(record);
            ApplyRouteDecision(node, record);

            record.Status = EnumExpeditionNodeProcessStatus.Resolved;
            record.BlackboardAfter = CurrentRun.Blackboard?.ToDebugString() ?? string.Empty;
            record.QueueAfterRoute = CurrentRun.DescribeQueue();

            CurrentRun.PendingEventOptionId = null;
            CurrentRun.PendingCombatResult = null;
            CurrentRun.ClearCurrentNode();

            if (CurrentRun.EndReason == EnumExpeditionEndReason.None && !CurrentRun.HasPendingNodes())
                CurrentRun.EndReason = EnumExpeditionEndReason.Victory;
        }

        public bool ShouldEnterSettlement()
        {
            return CurrentRun == null
                || CurrentRun.EndReason != EnumExpeditionEndReason.None
                || !CurrentRun.HasPendingNodes();
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
                    continue;

                var marbleResult = result.LstMarbleResult.Find(item => item.HasValue && item.Value.PersistentId == snapshot.Value.PersistentId);
                if (!marbleResult.HasValue)
                    continue;

                CurrentRun.MarbleSnapshots[i] = marbleResult;
            }

            var combatConfig = ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterId);
            var lstEffectConfig = result.IsVictory ? combatConfig?.LstVictoryEffect : combatConfig?.LstDefeatEffect;
            var context = new ExpeditionEffectExecutionContext(CurrentRun, _persistentData, record);
            ExpeditionEffectFactory.ExecuteEffects(lstEffectConfig, context);
            record.RecordCombatSummary(result, context.AppliedMoneyDelta, context.SummaryLines);

            if (!result.IsVictory)
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
        }

        private void ApplyDynamicInsertions(ExpeditionNodeRecord record)
        {
            if (CurrentRun == null || record == null)
                return;

            var lstInsertedEntry = CurrentRun.TriggerScheduledInsertions(record.NodeId);
            if (lstInsertedEntry.Count == 0)
            {
                record.AddRouteDecisionLog("当前节点没有触发动态插入。");
                return;
            }

            record.RecordInsertedNodeIds(lstInsertedEntry.Select(entry => entry.NodeId));
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
    }
}
