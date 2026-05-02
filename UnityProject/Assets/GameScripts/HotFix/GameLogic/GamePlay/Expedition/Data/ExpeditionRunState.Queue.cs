using System;
using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed partial class ExpeditionRunState
    {
        // 把当前正在执行的队列条目解析成统一运行时节点。
        public ExpeditionRuntimeNode GetCurrentNode()
        {
            return CurrentNodeEntry == null ? null : BuildRuntimeNode(CurrentNodeEntry);
        }

        // 从静态 Route 中按 node_config_id 查配置节点。
        public ExpeditionTable.ExpeditionRouteNodeConfig GetNode(string nodeConfigId)
        {
            return string.IsNullOrWhiteSpace(nodeConfigId)
                ? null
                : LstRouteConfig?.FirstOrDefault(node => node != null && node.NodeConfigId == nodeConfigId);
        }

        // 取当前节点对应的记录。进入节点时会创建，结算时会不断往里补信息。
        public ExpeditionNodeRecord GetCurrentRecord()
        {
            if (CurrentNodeEntry == null)
            {
                return null;
            }

            return LstNodeRecord.LastOrDefault(record => record != null && record.QueueEntryInstId == CurrentNodeEntry.QueueEntryInstId);
        }

        // 从待执行队列取出下一个节点，设为当前节点，并创建一条对应的 NodeRecord。
        public ExpeditionNodeRecord EnterNextPendingNode()
        {
            if (LstPendingNodeQueue == null || LstPendingNodeQueue.Count == 0)
            {
                CurrentNodeEntry = null;
                return null;
            }

            var queueBeforeEnter = DescribeQueue();
            CurrentNodeEntry = LstPendingNodeQueue[0];
            LstPendingNodeQueue.RemoveAt(0);

            var node = GetCurrentNode();
            if (node == null)
            {
                DebugTrace.RecordQueue(
                    $"缺失节点 nodeConfigId={CurrentNodeEntry.NodeConfigId}",
                    Phase,
                    CurrentNodeEntry.NodeConfigId,
                    CurrentNodeEntry.QueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Error);
                CurrentNodeEntry = null;
                EndReason = EnumExpeditionEndReason.Defeat;
                return null;
            }

            var record = new ExpeditionNodeRecord
            {
                QueueEntryInstId = CurrentNodeEntry.QueueEntryInstId,
                NodeConfigId = node.NodeConfigId,
                DisplayNodeLabel = node.DisplayNodeLabel,
                NodeType = node.NodeType,
                Status = EnumExpeditionNodeProcessStatus.Entered,
                EntryOrder = ++EnteredNodeCount,
                RoutePolicy = node.RoutePolicy.ToString(),
                SourceNodeConfigId = CurrentNodeEntry.SourceNodeConfigId,
                SourceTransitionId = CurrentNodeEntry.SourceTransitionId,
                EnqueueReason = CurrentNodeEntry.Reason,
                WasDynamicallyInserted = CurrentNodeEntry.IsDynamic,
                IsTemporaryRuntimeNode = CurrentNodeEntry.IsTemporaryRuntimeNode,
                QueueBeforeEnter = queueBeforeEnter,
                QueueAfterEnter = DescribeQueue(),
                BlackboardBefore = Blackboard?.ToDebugString() ?? string.Empty,
            };
            if (CurrentNodeEntry.IsTemporaryRuntimeNode)
            {
                record.ActualEventConfigId = CurrentNodeEntry.EventConfigId;
                record.ActualCombatEncounterConfigId = CurrentNodeEntry.CombatEncounterConfigId;
                record.AddRouteDecisionLog($"临时节点入队来源: {CurrentNodeEntry.DebugLabel}");
            }

            LstNodeRecord.Add(record);
            DebugTrace.RecordQueue(
                $"进入节点 queue={record.QueueAfterEnter}",
                Phase,
                record.NodeConfigId,
                record.QueueEntryInstId);
            return record;
        }

        // 当前节点结算完成后清空指针，等待下一次 EnterNextPendingNode。
        public void ClearCurrentNode()
        {
            CurrentNodeEntry = null;
        }

        // 当前是否还有待执行节点。没有的话，流程通常会进入结算。
        public bool HasPendingNodes()
        {
            return LstPendingNodeQueue != null && LstPendingNodeQueue.Count > 0;
        }

        // 把一个静态节点追加到队尾。主线路由解析通常用这个。
        public ExpeditionPendingNodeEntry EnqueueNode(
            string nodeConfigId,
            bool isDynamic,
            string sourceNodeConfigId,
            string sourceTransitionId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(nodeConfigId))
            {
                return null;
            }

            var entry = CreatePendingNodeEntry(nodeConfigId, isDynamic, sourceNodeConfigId, sourceTransitionId, reason);
            LstPendingNodeQueue.Add(entry);
            return entry;
        }

        // 把一个静态节点插到队首。立即插队逻辑通常用这个。
        public ExpeditionPendingNodeEntry InsertNodeAtFront(
            string nodeConfigId,
            bool isDynamic,
            string sourceNodeConfigId,
            string sourceTransitionId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(nodeConfigId))
            {
                return null;
            }

            var entry = CreatePendingNodeEntry(nodeConfigId, isDynamic, sourceNodeConfigId, sourceTransitionId, reason);
            LstPendingNodeQueue.Insert(0, entry);
            return entry;
        }

        // 用当前快照判断玩家队伍是否全灭。远征失败判断依赖这个结果。
        public bool AreAllPlayerMarblesDead()
        {
            var aliveCount = LstMarbleSnapshot.Count(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0);
            return aliveCount <= 0;
        }

        // 把当前待执行队列格式化成可读文本，便于日志和调试界面输出。
        public string DescribeQueue()
        {
            if (LstPendingNodeQueue == null || LstPendingNodeQueue.Count == 0)
            {
                return "<空>";
            }

            return string.Join(" -> ", LstPendingNodeQueue.Select(entry =>
            {
                if (entry == null)
                {
                    return "<空节点>";
                }

                var suffix = entry.IsDynamic ? "[动态]" : string.Empty;
                var label = entry.IsTemporaryRuntimeNode && !string.IsNullOrWhiteSpace(entry.DebugLabel)
                    ? entry.DebugLabel
                    : entry.NodeConfigId;
                return $"{label}{suffix}";
            }));
        }

        // 创建普通静态节点的队列条目。
        private static ExpeditionPendingNodeEntry CreatePendingNodeEntry(
            string nodeConfigId,
            bool isDynamic,
            string sourceNodeConfigId,
            string sourceTransitionId,
            string reason)
        {
            return new ExpeditionPendingNodeEntry
            {
                QueueEntryInstId = Guid.NewGuid().ToString("N"),
                NodeConfigId = nodeConfigId,
                IsDynamic = isDynamic,
                IsTemporaryRuntimeNode = false,
                NodeType = ExpeditionTable.EnumExpeditionNodeType.None,
                EventConfigId = string.Empty,
                CombatEncounterConfigId = string.Empty,
                SourceNodeConfigId = sourceNodeConfigId,
                SourceTransitionId = sourceTransitionId,
                SourcePendingInsertInstId = string.Empty,
                Reason = reason,
                DebugLabel = nodeConfigId,
            };
        }
    }
}
