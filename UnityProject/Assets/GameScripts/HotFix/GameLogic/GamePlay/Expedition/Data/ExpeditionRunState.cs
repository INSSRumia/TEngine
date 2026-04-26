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
        public bool IsTemporaryRuntimeNode;
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        public string EventConfigId;
        public string CombatEncounterConfigId;
        public string SourceNodeConfigId;
        public string SourceTransitionId;
        public string SourcePendingInsertInstId;
        public string Reason;
        public string DebugLabel;
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
    public class ExpeditionRuntimeNode
    {
        public string NodeConfigId;
        public string DisplayNodeLabel;
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        public bool IsTemporaryRuntimeNode;
        public string EventConfigId;
        public string CombatEncounterConfigId;
        public string RoutePolicy;
        public ExpeditionTable.ExpeditionRouteNodeConfig StaticNodeConfig;
    }

    [Serializable]
    public class ExpeditionPendingInsertNodeEntry
    {
        public string PendingInsertInstId;
        public int RemainingPassedNodeCount;
        public int CreateOrder;
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        public string EventConfigId;
        public string CombatEncounterConfigId;
        public string SourceNodeConfigId;
        public string SourceQueueEntryInstId;
        public string Reason;
        public string DebugLabel;
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
        public List<MarblePersistentData?> LstMarbleSnapshot = new ();
        public List<ExpeditionTable.ExpeditionRouteNodeConfig> Route = new ();
        public List<ExpeditionPendingNodeEntry> LstPendingNodeQueue = new ();
        public ExpeditionPendingNodeEntry CurrentNodeEntry;
        public List<ExpeditionNodeRecord> LstNodeRecord = new ();
        public List<ExpeditionScheduledNodeInsertion> LstScheduledInsertion = new ();
        public List<ExpeditionPendingInsertNodeEntry> LstPendingInsertNode = new ();
        public List<ExpeditionActiveRandomEventPoolState> LstActiveRandomEventPool = new ();
        public List<string> DebugLogs = new ();
        public string PendingEventOptionId;
        public CombatSessionResult PendingCombatResult;
        public bool IsSettlementAcknowledged;
        public ExpeditionResultSummary ResultSummary;
        public int EnteredNodeCount;
        public int PendingInsertOrderSeed;

        public ExpeditionRuntimeNode GetCurrentNode()
        {
            return CurrentNodeEntry == null ? null : BuildRuntimeNode(CurrentNodeEntry);
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

            return LstNodeRecord.LastOrDefault(record => record != null && record.QueueEntryInstId == CurrentNodeEntry.QueueEntryInstId);
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
                DebugLogs.Add($"[缺失节点] queueEntry={CurrentNodeEntry.QueueEntryInstId} nodeConfigId={CurrentNodeEntry.NodeConfigId}");
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
            DebugLogs.Add($"[进入节点] {record.NodeConfigId} queue={record.QueueAfterEnter}");
            return record;
        }

        public void ClearCurrentNode()
        {
            CurrentNodeEntry = null;
        }

        public bool HasPendingNodes()
        {
            return LstPendingNodeQueue != null && LstPendingNodeQueue.Count > 0;
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
            LstPendingNodeQueue.Add(entry);
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
            LstPendingNodeQueue.Insert(0, entry);
            return entry;
        }

        public ExpeditionPendingNodeEntry InsertTemporaryNodeAtFront(
            ExpeditionTable.EnumExpeditionNodeType nodeType,
            string referencedConfigId,
            string sourceNodeConfigId,
            string reason,
            string sourcePendingInsertInstId)
        {
            if (string.IsNullOrWhiteSpace(referencedConfigId))
            {
                return null;
            }

            if (nodeType != ExpeditionTable.EnumExpeditionNodeType.Event
                && nodeType != ExpeditionTable.EnumExpeditionNodeType.Combat)
            {
                return null;
            }

            var entry = CreateTemporaryPendingNodeEntry(
                nodeType,
                referencedConfigId,
                sourceNodeConfigId,
                reason,
                sourcePendingInsertInstId);
            LstPendingNodeQueue.Insert(0, entry);
            return entry;
        }

        public void ScheduleInsertionAfterNode(string triggerNodeConfigId, string nodeConfigId, string reason, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(triggerNodeConfigId) || string.IsNullOrWhiteSpace(nodeConfigId))
            {
                return;
            }

            LstScheduledInsertion.Add(new ExpeditionScheduledNodeInsertion
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
            if (string.IsNullOrWhiteSpace(triggerNodeConfigId) || LstScheduledInsertion == null || LstScheduledInsertion.Count == 0)
            {
                return new List<ExpeditionPendingNodeEntry>();
            }

            var insertions = LstScheduledInsertion
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

        public ExpeditionPendingInsertNodeEntry RegisterPendingInsertNode(
            int passedNodeCount,
            ExpeditionTable.EnumExpeditionNodeType nodeType,
            string referencedConfigId,
            string sourceNodeConfigId,
            string sourceQueueEntryInstId,
            string reason)
        {
            if (passedNodeCount <= 0)
            {
                DebugLogs.Add($"[延迟插入] 无效 passedNodeCount={passedNodeCount}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(referencedConfigId))
            {
                DebugLogs.Add("[延迟插入] 引用配置为空，忽略本次登记。");
                return null;
            }

            if (nodeType != ExpeditionTable.EnumExpeditionNodeType.Event
                && nodeType != ExpeditionTable.EnumExpeditionNodeType.Combat)
            {
                DebugLogs.Add($"[延迟插入] 不支持的临时节点类型 {nodeType}");
                return null;
            }

            var entry = new ExpeditionPendingInsertNodeEntry
            {
                PendingInsertInstId = Guid.NewGuid().ToString("N"),
                RemainingPassedNodeCount = passedNodeCount,
                CreateOrder = ++PendingInsertOrderSeed,
                NodeType = nodeType,
                EventConfigId = nodeType == ExpeditionTable.EnumExpeditionNodeType.Event ? referencedConfigId : string.Empty,
                CombatEncounterConfigId = nodeType == ExpeditionTable.EnumExpeditionNodeType.Combat ? referencedConfigId : string.Empty,
                SourceNodeConfigId = sourceNodeConfigId,
                SourceQueueEntryInstId = sourceQueueEntryInstId,
                Reason = reason,
                DebugLabel = BuildTemporaryNodeLabel(nodeType, referencedConfigId),
                IsConsumed = false,
            };
            LstPendingInsertNode.Add(entry);
            DebugLogs.Add($"[延迟插入] 登记 {entry.DebugLabel} remaining={entry.RemainingPassedNodeCount} source={entry.SourceNodeConfigId}");
            return entry;
        }

        public List<ExpeditionPendingNodeEntry> ResolvePendingInsertNodesAfterSettlement(string sourceNodeConfigId)
        {
            var lstInsertedEntry = new List<ExpeditionPendingNodeEntry>();
            if (LstPendingInsertNode == null || LstPendingInsertNode.Count == 0)
            {
                return lstInsertedEntry;
            }

            var lstExpiredEntry = new List<ExpeditionPendingInsertNodeEntry>();
            foreach (var pendingInsertNode in LstPendingInsertNode)
            {
                if (pendingInsertNode == null || pendingInsertNode.IsConsumed)
                {
                    continue;
                }

                pendingInsertNode.RemainingPassedNodeCount -= 1;
                DebugLogs.Add($"[延迟插入] 递减 {pendingInsertNode.DebugLabel} remaining={pendingInsertNode.RemainingPassedNodeCount}");
                if (pendingInsertNode.RemainingPassedNodeCount <= 0)
                {
                    pendingInsertNode.IsConsumed = true;
                    lstExpiredEntry.Add(pendingInsertNode);
                }
            }

            foreach (var expiredEntry in lstExpiredEntry)
            {
                var referencedConfigId = GetPendingInsertReferencedConfigId(expiredEntry);
                var insertedEntry = InsertTemporaryNodeAtFront(
                    expiredEntry.NodeType,
                    referencedConfigId,
                    sourceNodeConfigId,
                    expiredEntry.Reason,
                    expiredEntry.PendingInsertInstId);
                if (insertedEntry == null)
                {
                    DebugLogs.Add($"[延迟插入] 到期插入失败 {expiredEntry.DebugLabel}");
                    continue;
                }

                lstInsertedEntry.Add(insertedEntry);
                DebugLogs.Add($"[延迟插入] 到期插入 {expiredEntry.DebugLabel}");
            }

            LstPendingInsertNode.RemoveAll(item => item == null || item.IsConsumed);
            return lstInsertedEntry;
        }

        public bool AreAllPlayerMarblesDead()
        {
            var aliveCount = LstMarbleSnapshot.Count(snapshot => snapshot.HasValue && !snapshot.Value.IsDead && snapshot.Value.CurrentHp > 0);
            return aliveCount <= 0;
        }

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

        private ExpeditionRuntimeNode BuildRuntimeNode(ExpeditionPendingNodeEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.IsTemporaryRuntimeNode)
            {
                return BuildTemporaryRuntimeNode(entry);
            }

            var staticNode = GetNode(entry.NodeConfigId);
            if (staticNode == null)
            {
                return null;
            }

            return new ExpeditionRuntimeNode
            {
                NodeConfigId = staticNode.NodeConfigId,
                DisplayNodeLabel = staticNode.NodeConfigId,
                NodeType = staticNode.NodeType,
                IsTemporaryRuntimeNode = false,
                EventConfigId = staticNode.EventConfigId,
                CombatEncounterConfigId = staticNode.CombatEncounterConfigId,
                RoutePolicy = staticNode.RoutePolicy.ToString(),
                StaticNodeConfig = staticNode,
            };
        }

        private ExpeditionRuntimeNode BuildTemporaryRuntimeNode(ExpeditionPendingNodeEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.NodeType == ExpeditionTable.EnumExpeditionNodeType.Event)
            {
                if (ExpeditionConfigBridge.ResolveEvent(entry.EventConfigId) == null)
                {
                    return null;
                }
            }

            if (entry.NodeType == ExpeditionTable.EnumExpeditionNodeType.Combat)
            {
                if (ExpeditionConfigBridge.ResolveCombatEncounter(entry.CombatEncounterConfigId) == null)
                {
                    return null;
                }
            }

            return new ExpeditionRuntimeNode
            {
                NodeConfigId = entry.NodeConfigId,
                DisplayNodeLabel = entry.DebugLabel,
                NodeType = entry.NodeType,
                IsTemporaryRuntimeNode = true,
                EventConfigId = entry.EventConfigId,
                CombatEncounterConfigId = entry.CombatEncounterConfigId,
                RoutePolicy = ExpeditionTable.EnumExpeditionRoutePolicy.FixedNext.ToString(),
                StaticNodeConfig = null,
            };
        }

        private static ExpeditionPendingNodeEntry CreateTemporaryPendingNodeEntry(
            ExpeditionTable.EnumExpeditionNodeType nodeType,
            string referencedConfigId,
            string sourceNodeConfigId,
            string reason,
            string sourcePendingInsertInstId)
        {
            var queueEntryInstId = Guid.NewGuid().ToString("N");
            var shortQueueId = queueEntryInstId.Length > 8 ? queueEntryInstId.Substring(0, 8) : queueEntryInstId;
            var debugLabel = BuildTemporaryNodeLabel(nodeType, referencedConfigId);
            return new ExpeditionPendingNodeEntry
            {
                QueueEntryInstId = queueEntryInstId,
                NodeConfigId = $"{debugLabel}:{shortQueueId}",
                IsDynamic = true,
                IsTemporaryRuntimeNode = true,
                NodeType = nodeType,
                EventConfigId = nodeType == ExpeditionTable.EnumExpeditionNodeType.Event ? referencedConfigId : string.Empty,
                CombatEncounterConfigId = nodeType == ExpeditionTable.EnumExpeditionNodeType.Combat ? referencedConfigId : string.Empty,
                SourceNodeConfigId = sourceNodeConfigId,
                SourceTransitionId = string.Empty,
                SourcePendingInsertInstId = sourcePendingInsertInstId,
                Reason = reason,
                DebugLabel = debugLabel,
            };
        }

        private static string BuildTemporaryNodeLabel(ExpeditionTable.EnumExpeditionNodeType nodeType, string referencedConfigId)
        {
            return nodeType == ExpeditionTable.EnumExpeditionNodeType.Event
                ? $"temp_event:{referencedConfigId}"
                : $"temp_combat:{referencedConfigId}";
        }

        private static string GetPendingInsertReferencedConfigId(ExpeditionPendingInsertNodeEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            return entry.NodeType == ExpeditionTable.EnumExpeditionNodeType.Event
                ? entry.EventConfigId
                : entry.CombatEncounterConfigId;
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
