using System;
using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionPendingNodeEntry
    {
        public string QueueEntryInstId;
        public string NodeConfigId;
        public bool IsDynamic;
        public string SourceNodeConfigId;
        public string SourceTransitionId;
        public string Reason;
    }

    [Serializable]
    public sealed class ExpeditionScheduledNodeInsertion
    {
        public string InsertionRequestInstId;
        public string TriggerNodeConfigId;
        public string NodeConfigId;
        public int Priority;
        public string Reason;
        public bool IsConsumed;
    }

    [Serializable]
    public sealed class ExpeditionRunState
    {
        public string ExpeditionInstId;
        public string ExpeditionConfigId;
        public EnumExpeditionFlowPhase Phase;
        public EnumExpeditionEndReason EndReason;
        public int TotalMoneyGained;
        public ExpeditionBlackboard Blackboard = new ExpeditionBlackboard();
        public List<MarblePersistentData?> MarbleSnapshots = new List<MarblePersistentData?>();
        public List<ExpeditionTable.ExpeditionRouteNodeConfig> Route = new List<ExpeditionTable.ExpeditionRouteNodeConfig>();
        public List<ExpeditionPendingNodeEntry> PendingNodeQueue = new List<ExpeditionPendingNodeEntry>();
        public ExpeditionPendingNodeEntry CurrentNodeEntry;
        public List<ExpeditionNodeRecord> NodeRecords = new List<ExpeditionNodeRecord>();
        public List<ExpeditionScheduledNodeInsertion> ScheduledInsertions = new List<ExpeditionScheduledNodeInsertion>();
        public List<string> DebugLogs = new List<string>();
        public string PendingEventOptionId;
        public CombatSessionResult PendingCombatResult;
        public bool IsSettlementAcknowledged;
        public ExpeditionResultSummary ResultSummary;
        public int EnteredNodeCount;

        public ExpeditionTable.ExpeditionRouteNodeConfig GetCurrentNode()
        {
            return CurrentNodeEntry == null ? null : GetNode(CurrentNodeEntry.NodeConfigId);
        }

        public ExpeditionTable.ExpeditionRouteNodeConfig GetNode(string nodeConfigId)
        {
            return string.IsNullOrWhiteSpace(nodeConfigId)
                ? null
                : Route?.FirstOrDefault(node => node != null && node.NodeConfigId == nodeConfigId);
        }

        public ExpeditionNodeRecord GetCurrentRecord()
        {
            if (CurrentNodeEntry == null)
            {
                return null;
            }

            return NodeRecords.LastOrDefault(record => record != null && record.QueueEntryInstId == CurrentNodeEntry.QueueEntryInstId);
        }

        public ExpeditionNodeRecord EnterNextPendingNode()
        {
            if (PendingNodeQueue == null || PendingNodeQueue.Count == 0)
            {
                CurrentNodeEntry = null;
                return null;
            }

            var queueBeforeEnter = DescribeQueue();
            CurrentNodeEntry = PendingNodeQueue[0];
            PendingNodeQueue.RemoveAt(0);

            var node = GetCurrentNode();
            if (node == null)
            {
                DebugLogs.Add($"[缺失节点] queueEntry={CurrentNodeEntry.QueueEntryInstId} nodeConfigId={CurrentNodeEntry.NodeConfigId}");
                CurrentNodeEntry = null;
                EndReason = EnumExpeditionEndReason.Defeat;
                return null;
            }

            var record = new ExpeditionNodeRecord
            {
                QueueEntryInstId = CurrentNodeEntry.QueueEntryInstId,
                NodeConfigId = node.NodeConfigId,
                NodeType = node.NodeType,
                Status = EnumExpeditionNodeProcessStatus.Entered,
                EntryOrder = ++EnteredNodeCount,
                RoutePolicy = node.RoutePolicy.ToString(),
                SourceNodeConfigId = CurrentNodeEntry.SourceNodeConfigId,
                SourceTransitionId = CurrentNodeEntry.SourceTransitionId,
                EnqueueReason = CurrentNodeEntry.Reason,
                WasDynamicallyInserted = CurrentNodeEntry.IsDynamic,
                QueueBeforeEnter = queueBeforeEnter,
                QueueAfterEnter = DescribeQueue(),
                BlackboardBefore = Blackboard?.ToDebugString() ?? string.Empty,
            };
            NodeRecords.Add(record);
            DebugLogs.Add($"[进入节点] {record.NodeConfigId} queue={record.QueueAfterEnter}");
            return record;
        }

        public void ClearCurrentNode()
        {
            CurrentNodeEntry = null;
        }

        public bool HasPendingNodes()
        {
            return PendingNodeQueue != null && PendingNodeQueue.Count > 0;
        }

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
            PendingNodeQueue.Add(entry);
            return entry;
        }

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
            PendingNodeQueue.Insert(0, entry);
            return entry;
        }

        public void ScheduleInsertionAfterNode(string triggerNodeConfigId, string nodeConfigId, string reason, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(triggerNodeConfigId) || string.IsNullOrWhiteSpace(nodeConfigId))
            {
                return;
            }

            ScheduledInsertions.Add(new ExpeditionScheduledNodeInsertion
            {
                InsertionRequestInstId = Guid.NewGuid().ToString("N"),
                TriggerNodeConfigId = triggerNodeConfigId,
                NodeConfigId = nodeConfigId,
                Priority = priority,
                Reason = reason,
                IsConsumed = false,
            });
        }

        public List<ExpeditionPendingNodeEntry> TriggerScheduledInsertions(string triggerNodeConfigId)
        {
            if (string.IsNullOrWhiteSpace(triggerNodeConfigId) || ScheduledInsertions == null || ScheduledInsertions.Count == 0)
            {
                return new List<ExpeditionPendingNodeEntry>();
            }

            var insertions = ScheduledInsertions
                .Where(item => item != null && !item.IsConsumed && item.TriggerNodeConfigId == triggerNodeConfigId)
                .OrderByDescending(item => item.Priority)
                .ToList();
            var insertedEntries = new List<ExpeditionPendingNodeEntry>(insertions.Count);
            for (int i = insertions.Count - 1; i >= 0; i--)
            {
                var insertion = insertions[i];
                insertion.IsConsumed = true;
                var entry = InsertNodeAtFront(
                    insertion.NodeConfigId,
                    true,
                    triggerNodeConfigId,
                    string.Empty,
                    insertion.Reason);
                if (entry != null)
                {
                    insertedEntries.Add(entry);
                }
            }

            insertedEntries.Reverse();
            return insertedEntries;
        }

        public bool AreAllPlayerMarblesDead()
        {
            var aliveCount = MarbleSnapshots.Count(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0);
            return aliveCount <= 0;
        }

        public string DescribeQueue()
        {
            if (PendingNodeQueue == null || PendingNodeQueue.Count == 0)
            {
                return "<空>";
            }

            return string.Join(" -> ", PendingNodeQueue.Select(entry =>
            {
                if (entry == null)
                {
                    return "<空节点>";
                }

                var suffix = entry.IsDynamic ? "[动态]" : string.Empty;
                return $"{entry.NodeConfigId}{suffix}";
            }));
        }

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
                SourceNodeConfigId = sourceNodeConfigId,
                SourceTransitionId = sourceTransitionId,
                Reason = reason,
            };
        }
    }
}
