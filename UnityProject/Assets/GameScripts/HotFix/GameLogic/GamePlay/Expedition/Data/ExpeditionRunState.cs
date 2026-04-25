using System;
using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;
using TEngine;

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
        private static readonly Random _random = new Random();
        private const string BaseRandomEventPoolSourceType = "expedition";
        private const string EnvironmentRandomEventPoolSourceType = "environment";

        public string ExpeditionInstId;
        public string ExpeditionConfigId;
        public string CurrentEnvironmentConfigId;
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
        public List<ExpeditionActiveRandomEventPoolState> LstActiveRandomEventPool = new List<ExpeditionActiveRandomEventPoolState>();
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

        public void InitializeRandomEventPools(ExpeditionTable.ExpeditionConfig expedition)
        {
            LstActiveRandomEventPool.Clear();
            ActivateRandomEventPools(
                expedition?.LstRandomEventPoolConfigId,
                BaseRandomEventPoolSourceType,
                expedition?.ExpeditionConfigId,
                true);
            ChangeEnvironment(expedition?.InitialEnvironmentConfigId);
        }

        public bool ChangeEnvironment(string environmentConfigId)
        {
            RemoveRandomEventPoolsBySource(EnvironmentRandomEventPoolSourceType);
            CurrentEnvironmentConfigId = environmentConfigId ?? string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentEnvironmentConfigId))
            {
                DebugLogs.Add("[环境] 当前环境为空，仅保留远征基础随机事件池。");
                return true;
            }

            var environment = ExpeditionConfigBridge.ResolveEnvironment(CurrentEnvironmentConfigId);
            if (environment == null)
            {
                DebugLogs.Add($"[环境] 未找到环境配置 environmentConfigId={CurrentEnvironmentConfigId}");
                Log.Warning($"[远征] 未找到环境配置。environmentConfigId:{CurrentEnvironmentConfigId}");
                return false;
            }

            ActivateRandomEventPools(
                environment.LstRandomEventPoolConfigId,
                EnvironmentRandomEventPoolSourceType,
                CurrentEnvironmentConfigId,
                true);
            DebugLogs.Add($"[环境] 当前环境切换为 {CurrentEnvironmentConfigId}");
            return true;
        }

        public ExpeditionRandomEventDrawResult DrawRandomEvent()
        {
            var lstValidPool = LstActiveRandomEventPool?
                .Where(pool => pool != null && pool.GetTotalWeight() > 0)
                .ToList() ?? new List<ExpeditionActiveRandomEventPoolState>();
            var totalWeight = lstValidPool.Sum(pool => pool.GetTotalWeight());
            if (totalWeight <= 0)
            {
                return new ExpeditionRandomEventDrawResult
                {
                    IsSuccess = false,
                    Summary = "当前没有可抽取的随机事件，跳过 RandomEvent 节点。",
                };
            }

            var globalWeight = _random.Next(totalWeight);
            var cursor = 0;
            foreach (var pool in lstValidPool)
            {
                var poolWeight = pool.GetTotalWeight();
                if (globalWeight >= cursor + poolWeight)
                {
                    cursor += poolWeight;
                    continue;
                }

                return DrawRandomEventFromPool(pool, globalWeight - cursor);
            }

            return new ExpeditionRandomEventDrawResult
            {
                IsSuccess = false,
                Summary = "随机事件权重定位失败，跳过 RandomEvent 节点。",
            };
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

        private void ActivateRandomEventPools(IEnumerable<string> lstPoolConfigId, string sourceType, string sourceConfigId, bool preserveExistingState)
        {
            if (lstPoolConfigId == null)
                return;

            foreach (var poolConfigId in lstPoolConfigId)
            {
                if (string.IsNullOrWhiteSpace(poolConfigId))
                    continue;

                if (preserveExistingState && HasActiveRandomEventPool(poolConfigId, sourceType, sourceConfigId))
                    continue;

                var poolConfig = ExpeditionConfigBridge.ResolveRandomEventPool(poolConfigId);
                if (poolConfig == null)
                {
                    DebugLogs.Add($"[随机事件池] 未找到池配置 poolConfigId={poolConfigId}");
                    Log.Warning($"[远征] 未找到随机事件池配置。poolConfigId:{poolConfigId}");
                    continue;
                }

                var poolState = new ExpeditionActiveRandomEventPoolState
                {
                    PoolRuntimeInstId = Guid.NewGuid().ToString("N"),
                    RandomEventPoolConfigId = poolConfig.RandomEventPoolConfigId,
                    SourceType = sourceType ?? string.Empty,
                    SourceConfigId = sourceConfigId ?? string.Empty,
                    LstRemainingEntry = poolConfig.LstEvent?
                        .Where(entry => entry != null)
                        .Select(entry => new ExpeditionRandomEventPoolEntryState(entry))
                        .ToList() ?? new List<ExpeditionRandomEventPoolEntryState>(),
                };
                LstActiveRandomEventPool.Add(poolState);
                DebugLogs.Add($"[随机事件池] 激活 {poolState.RandomEventPoolConfigId} source={poolState.SourceType}:{poolState.SourceConfigId} entries={poolState.LstRemainingEntry.Count}");
            }
        }

        private void RemoveRandomEventPoolsBySource(string sourceType)
        {
            if (LstActiveRandomEventPool == null || LstActiveRandomEventPool.Count == 0)
                return;

            var removedCount = LstActiveRandomEventPool.RemoveAll(pool => pool != null && pool.SourceType == sourceType);
            if (removedCount > 0)
                DebugLogs.Add($"[随机事件池] 移除来源 {sourceType} 的池数量:{removedCount}");
        }

        private bool HasActiveRandomEventPool(string poolConfigId, string sourceType, string sourceConfigId)
        {
            return LstActiveRandomEventPool.Any(pool =>
                pool != null
                && pool.RandomEventPoolConfigId == poolConfigId
                && pool.SourceType == (sourceType ?? string.Empty)
                && pool.SourceConfigId == (sourceConfigId ?? string.Empty));
        }

        private static ExpeditionRandomEventDrawResult DrawRandomEventFromPool(ExpeditionActiveRandomEventPoolState pool, int localWeight)
        {
            var cursor = 0;
            for (int i = 0; i < pool.LstRemainingEntry.Count; i++)
            {
                var entry = pool.LstRemainingEntry[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.EventConfigId) || entry.Weight <= 0)
                    continue;

                if (localWeight >= cursor + entry.Weight)
                {
                    cursor += entry.Weight;
                    continue;
                }

                pool.LstRemainingEntry.RemoveAt(i);
                return new ExpeditionRandomEventDrawResult
                {
                    IsSuccess = true,
                    EventConfigId = entry.EventConfigId,
                    RandomEventPoolConfigId = pool.RandomEventPoolConfigId,
                    Summary = $"从随机事件池 {pool.RandomEventPoolConfigId} 抽取事件 {entry.EventConfigId}。",
                };
            }

            return new ExpeditionRandomEventDrawResult
            {
                IsSuccess = false,
                Summary = $"随机事件池 {pool.RandomEventPoolConfigId} 没有命中有效条目。",
            };
        }
    }

    [Serializable]
    public class ExpeditionRandomEventPoolEntryState
    {
        public string EventConfigId;
        public int Weight;

        public ExpeditionRandomEventPoolEntryState()
        {
        }

        public ExpeditionRandomEventPoolEntryState(ExpeditionTable.ExpeditionRandomEventPoolEntryConfig config)
        {
            EventConfigId = config?.EventConfigId ?? string.Empty;
            Weight = config?.Weight ?? 0;
        }
    }

    [Serializable]
    public class ExpeditionActiveRandomEventPoolState
    {
        public string PoolRuntimeInstId;
        public string RandomEventPoolConfigId;
        public string SourceType;
        public string SourceConfigId;
        public List<ExpeditionRandomEventPoolEntryState> LstRemainingEntry = new List<ExpeditionRandomEventPoolEntryState>();

        public int GetTotalWeight()
        {
            return LstRemainingEntry?
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.EventConfigId) && entry.Weight > 0)
                .Sum(entry => entry.Weight) ?? 0;
        }
    }

    public class ExpeditionRandomEventDrawResult
    {
        public bool IsSuccess;
        public string EventConfigId;
        public string RandomEventPoolConfigId;
        public string Summary;
    }
}
