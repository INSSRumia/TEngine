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

        public bool PrepareCurrentRandomEventNode()
        {
            var node = GetCurrentNode();
            var record = CurrentRun?.GetCurrentRecord();
            if (CurrentRun == null || node == null || record == null)
                return false;

            if (record.WasRandomEventSkipped || !string.IsNullOrWhiteSpace(record.ActualEventConfigId))
                return !record.WasRandomEventSkipped;

            var drawResult = CurrentRun.DrawRandomEvent();
            record.AddRouteDecisionLog(drawResult?.Summary);
            if (drawResult == null || !drawResult.IsSuccess)
            {
                record.WasRandomEventSkipped = true;
                record.Summary = drawResult?.Summary ?? "随机事件抽取失败，节点跳过。";
                CurrentRun.DebugLogs.Add($"[随机事件节点] {node.NodeConfigId} 未抽到事件。");
                return false;
            }

            record.ActualEventConfigId = drawResult.EventConfigId;
            record.RandomEventPoolConfigId = drawResult.RandomEventPoolConfigId;
            if (ExpeditionConfigBridge.ResolveEvent(record.ActualEventConfigId) == null)
            {
                record.WasRandomEventSkipped = true;
                record.Summary = $"随机事件 {record.ActualEventConfigId} 不存在，节点跳过。";
                record.AddRouteDecisionLog(record.Summary);
                CurrentRun.DebugLogs.Add($"[随机事件节点] {node.NodeConfigId} 抽到不存在的事件 {record.ActualEventConfigId}。");
                return false;
            }

            CurrentRun.DebugLogs.Add($"[随机事件节点] {node.NodeConfigId} 抽到事件 {record.ActualEventConfigId} pool={record.RandomEventPoolConfigId}");
            return true;
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
                case EnumExpeditionNodeType.RandomEvent:
                    ApplyRandomEventNodeResult(node, record);
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
            var eventConfig = ResolveNodeEventConfig(node, record);
            var option = eventConfig?.Options?.FirstOrDefault(item => item != null && item.OptionId == CurrentRun.PendingEventOptionId);
            if (option == null)
            {
                record.AddRouteDecisionLog($"节点 {node?.NodeConfigId} 没有收到合法选项输入。");
                return;
            }

            var context = new ExpeditionEffectExecutionContext(CurrentRun, _persistentData, record);
            ExpeditionEffectFactory.ExecuteEffects(option.LstEffect, context);
            record.ChosenOptionId = option.OptionId;
            record.RecordEffectSummary(context.AppliedMoneyDelta, context.SummaryLines);

            CurrentRun.Blackboard?.AddChosenOption(option.OptionId);
            CurrentRun.Blackboard?.AddCompletedEvent(eventConfig.EventConfigId);
        }

        private void ApplyRandomEventNodeResult(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            if (record.WasRandomEventSkipped)
            {
                record.RecordEffectSummary(0, new[] { "随机事件池为空，节点跳过。" });
                record.AddRouteDecisionLog("RandomEvent 节点未抽到事件，按默认出口推进。");
                return;
            }

            ApplyEventNodeResult(node, record);
        }

        private void ApplyCombatNodeResult(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            var result = CurrentRun.PendingCombatResult;
            if (result == null)
            {
                record.AddRouteDecisionLog($"节点 {node.NodeConfigId} 没有收到 Combat 结果。");
                return;
            }

            for (int i = 0; i < CurrentRun.MarbleSnapshots.Count; i++)
            {
                var snapshot = CurrentRun.MarbleSnapshots[i];
                if (!snapshot.HasValue)
                    continue;

                var marbleResult = result.LstMarbleResult.Find(item => item.HasValue && item.Value.MarbleInstId == snapshot.Value.MarbleInstId);
                if (!marbleResult.HasValue)
                    continue;

                CurrentRun.MarbleSnapshots[i] = marbleResult;
            }

            var combatConfig = ExpeditionConfigBridge.ResolveCombatEncounter(node.CombatEncounterConfigId);
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

            var lstInsertedEntry = CurrentRun.TriggerScheduledInsertions(record.NodeConfigId);
            if (lstInsertedEntry.Count == 0)
            {
                record.AddRouteDecisionLog("当前节点没有触发动态插入。");
                return;
            }

            record.RecordInsertedNodeIds(lstInsertedEntry.Select(entry => entry.NodeConfigId));
        }

        private void ApplyRouteDecision(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            if (CurrentRun == null || node == null || record == null)
                return;

            if (CurrentRun.EndReason != EnumExpeditionEndReason.None)
            {
                record.AddRouteDecisionLog($"远征已结束，跳过节点 {node.NodeConfigId} 的出口解析。");
                return;
            }

            var decision = ExpeditionRouteResolver.Resolve(CurrentRun, node, record);
            if (!string.IsNullOrWhiteSpace(decision?.Summary))
                record.AddRouteDecisionLog(decision.Summary);

            if (decision == null || string.IsNullOrWhiteSpace(decision.TargetNodeConfigId))
                return;

            var enqueuedNode = CurrentRun.EnqueueNode(
                decision.TargetNodeConfigId,
                false,
                node.NodeConfigId,
                decision.TransitionId,
                "route_transition");
            record.ResolvedTransitionId = decision.TransitionId;
            record.NextNodeConfigId = enqueuedNode?.NodeConfigId;
        }

        private ExpeditionEventConfig ResolveNodeEventConfig(ExpeditionRouteNodeConfig node, ExpeditionNodeRecord record)
        {
            var eventConfigId = !string.IsNullOrWhiteSpace(record?.ActualEventConfigId)
                ? record.ActualEventConfigId
                : node?.EventConfigId;
            return ExpeditionConfigBridge.ResolveEvent(eventConfigId);
        }
    }
}
