using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLogic.Gameplay.Combat.Marble;
using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionConstants
    {
        public const string MinimalExpeditionId = "MinimalExpedition";
        public const int PlayerCamp = 1;
        public const int EnemyCamp = 2;
    }

    public enum EnumExpeditionFlowPhase
    {
        None = 0,
        Preparing = 1,
        EnteringNode = 2,
        WaitingEventChoice = 3,
        InCombat = 4,
        ApplyingNodeResult = 5,
        Settling = 6,
        Finished = 7,
    }

    public enum EnumExpeditionEndReason
    {
        None = 0,
        Victory = 1,
        Defeat = 2,
    }

    public enum EnumExpeditionNodeProcessStatus
    {
        Pending = 0,
        Entered = 1,
        Resolved = 2,
    }

    [Serializable]
    public partial struct MarblePersistentData
    {
        public string PersistentId;
        public string ConfigId;
        public string DisplayName;
        public int Level;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;

        public static MarblePersistentData CreateDefault(string persistentId, string configId, string displayName, int level)
        {
            var maxHp = ExpeditionConfigBridge.ResolveMarbleMaxHp(configId, level);
            return new MarblePersistentData
            {
                PersistentId = persistentId,
                ConfigId = configId,
                DisplayName = displayName,
                Level = level,
                CurrentHp = maxHp,
                MaxHp = maxHp,
                Exp = 0,
                IsDead = false,
            };
        }
    }

    [Serializable]
    public sealed class ExpeditionPersistentDataStore
    {
        public int Money;
        public List<MarblePersistentData?> Marbles = new List<MarblePersistentData?>();
        public ExpeditionResultSummary LastResult;

        public void EnsureInitialized()
        {
            if (Marbles.Count > 0)
            {
                return;
            }

            Money = 0;
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_1", "lancer", "先锋一号", 0));
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_2", "archer", "先锋二号", 0));
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_3", "soldier", "先锋三号", 0));
        }

        public MarblePersistentData? GetMarble(string persistentId)
        {
            return Marbles.Find(marble => marble.HasValue && marble.Value.PersistentId == persistentId);
        }

        public void SetMarble(MarblePersistentData marble)
        {
            for (int i = 0; i < Marbles.Count; i++)
            {
                if (!Marbles[i].HasValue || Marbles[i].Value.PersistentId != marble.PersistentId)
                {
                    continue;
                }

                Marbles[i] = marble;
                return;
            }

            Marbles.Add(marble);
        }
    }

    [Serializable]
    public sealed class ExpeditionBlackboardCounter
    {
        public string CounterId;
        public int Value;
    }

    [Serializable]
    public sealed class ExpeditionBlackboard
    {
        public List<string> Flags = new List<string>();
        public List<string> InventoryItemIds = new List<string>();
        public List<string> ChosenOptionIds = new List<string>();
        public List<string> CompletedEventIds = new List<string>();
        public List<ExpeditionBlackboardCounter> Counters = new List<ExpeditionBlackboardCounter>();

        public bool HasFlag(string flagId)
        {
            return !string.IsNullOrWhiteSpace(flagId) && Flags.Contains(flagId);
        }

        public void AddFlag(string flagId)
        {
            AddUnique(Flags, flagId);
        }

        public bool HasItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId) && InventoryItemIds.Contains(itemId);
        }

        public void AddItem(string itemId)
        {
            AddUnique(InventoryItemIds, itemId);
        }

        public bool HasChosenOption(string optionId)
        {
            return !string.IsNullOrWhiteSpace(optionId) && ChosenOptionIds.Contains(optionId);
        }

        public void AddChosenOption(string optionId)
        {
            AddUnique(ChosenOptionIds, optionId);
        }

        public bool HasCompletedEvent(string eventId)
        {
            return !string.IsNullOrWhiteSpace(eventId) && CompletedEventIds.Contains(eventId);
        }

        public void AddCompletedEvent(string eventId)
        {
            AddUnique(CompletedEventIds, eventId);
        }

        public int GetCounterValue(string counterId)
        {
            if (string.IsNullOrWhiteSpace(counterId))
            {
                return 0;
            }

            var counter = Counters.Find(item => item != null && item.CounterId == counterId);
            return counter?.Value ?? 0;
        }

        public void SetCounterValue(string counterId, int value)
        {
            if (string.IsNullOrWhiteSpace(counterId))
            {
                return;
            }

            var counter = Counters.Find(item => item != null && item.CounterId == counterId);
            if (counter == null)
            {
                Counters.Add(new ExpeditionBlackboardCounter
                {
                    CounterId = counterId,
                    Value = value,
                });
                return;
            }

            counter.Value = value;
        }

        public void AddCounterValue(string counterId, int delta)
        {
            SetCounterValue(counterId, GetCounterValue(counterId) + delta);
        }

        public string ToDebugString()
        {
            var builder = new StringBuilder();
            builder.Append("flags=[");
            builder.Append(string.Join(",", Flags));
            builder.Append("]\nitems=[");
            builder.Append(string.Join(",", InventoryItemIds));
            builder.Append("]\nchosen=[");
            builder.Append(string.Join(",", ChosenOptionIds));
            builder.Append("]\ncompleted=[");
            builder.Append(string.Join(",", CompletedEventIds));
            builder.Append("]\ncounters=[");
            builder.Append(string.Join(",", Counters.Where(counter => counter != null).Select(counter => $"{counter.CounterId}:{counter.Value}")));
            builder.Append("]\n");
            return builder.ToString();
        }

        private static void AddUnique(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value) || values.Contains(value))
            {
                return;
            }

            values.Add(value);
        }
    }

    [Serializable]
    public sealed class ExpeditionPendingNodeEntry
    {
        public string QueueEntryId;
        public string NodeId;
        public bool IsDynamic;
        public string SourceNodeId;
        public string SourceTransitionId;
        public string Reason;
    }

    [Serializable]
    public sealed class ExpeditionScheduledNodeInsertion
    {
        public string RequestId;
        public string TriggerNodeId;
        public string NodeId;
        public int Priority;
        public string Reason;
        public bool IsConsumed;
    }

    [Serializable]
    public sealed class ExpeditionRunState
    {
        public string RunId;
        public string ExpeditionId;
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
            return CurrentNodeEntry == null ? null : GetNode(CurrentNodeEntry.NodeId);
        }

        public ExpeditionTable.ExpeditionRouteNodeConfig GetNode(string nodeId)
        {
            return string.IsNullOrWhiteSpace(nodeId)
                ? null
                : Route?.FirstOrDefault(node => node != null && node.NodeId == nodeId);
        }

        public ExpeditionNodeRecord GetCurrentRecord()
        {
            if (CurrentNodeEntry == null)
            {
                return null;
            }

            return NodeRecords.LastOrDefault(record => record != null && record.QueueEntryId == CurrentNodeEntry.QueueEntryId);
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
                DebugLogs.Add($"[MissingNode] queueEntry={CurrentNodeEntry.QueueEntryId} nodeId={CurrentNodeEntry.NodeId}");
                CurrentNodeEntry = null;
                EndReason = EnumExpeditionEndReason.Defeat;
                return null;
            }

            var record = new ExpeditionNodeRecord
            {
                QueueEntryId = CurrentNodeEntry.QueueEntryId,
                NodeId = node.NodeId,
                NodeType = node.NodeType,
                Status = EnumExpeditionNodeProcessStatus.Entered,
                EntryOrder = ++EnteredNodeCount,
                RoutePolicy = node.RoutePolicy.ToString(),
                SourceNodeId = CurrentNodeEntry.SourceNodeId,
                SourceTransitionId = CurrentNodeEntry.SourceTransitionId,
                EnqueueReason = CurrentNodeEntry.Reason,
                WasDynamicallyInserted = CurrentNodeEntry.IsDynamic,
                QueueBeforeEnter = queueBeforeEnter,
                QueueAfterEnter = DescribeQueue(),
                BlackboardBefore = Blackboard?.ToDebugString() ?? string.Empty,
            };
            NodeRecords.Add(record);
            DebugLogs.Add($"[EnterNode] {record.NodeId} queue={record.QueueAfterEnter}");
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
            string nodeId,
            bool isDynamic,
            string sourceNodeId,
            string sourceTransitionId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            var entry = CreatePendingNodeEntry(nodeId, isDynamic, sourceNodeId, sourceTransitionId, reason);
            PendingNodeQueue.Add(entry);
            return entry;
        }

        public ExpeditionPendingNodeEntry InsertNodeAtFront(
            string nodeId,
            bool isDynamic,
            string sourceNodeId,
            string sourceTransitionId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            var entry = CreatePendingNodeEntry(nodeId, isDynamic, sourceNodeId, sourceTransitionId, reason);
            PendingNodeQueue.Insert(0, entry);
            return entry;
        }

        public void ScheduleInsertionAfterNode(string triggerNodeId, string nodeId, string reason, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(triggerNodeId) || string.IsNullOrWhiteSpace(nodeId))
            {
                return;
            }

            ScheduledInsertions.Add(new ExpeditionScheduledNodeInsertion
            {
                RequestId = Guid.NewGuid().ToString("N"),
                TriggerNodeId = triggerNodeId,
                NodeId = nodeId,
                Priority = priority,
                Reason = reason,
                IsConsumed = false,
            });
        }

        public List<ExpeditionPendingNodeEntry> TriggerScheduledInsertions(string triggerNodeId)
        {
            if (string.IsNullOrWhiteSpace(triggerNodeId) || ScheduledInsertions == null || ScheduledInsertions.Count == 0)
            {
                return new List<ExpeditionPendingNodeEntry>();
            }

            var insertions = ScheduledInsertions
                .Where(item => item != null && !item.IsConsumed && item.TriggerNodeId == triggerNodeId)
                .OrderByDescending(item => item.Priority)
                .ToList();
            var insertedEntries = new List<ExpeditionPendingNodeEntry>(insertions.Count);
            for (int i = insertions.Count - 1; i >= 0; i--)
            {
                var insertion = insertions[i];
                insertion.IsConsumed = true;
                var entry = InsertNodeAtFront(
                    insertion.NodeId,
                    true,
                    triggerNodeId,
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
                return "<empty>";
            }

            return string.Join(" -> ", PendingNodeQueue.Select(entry =>
            {
                if (entry == null)
                {
                    return "<null>";
                }

                var suffix = entry.IsDynamic ? "[dynamic]" : string.Empty;
                return $"{entry.NodeId}{suffix}";
            }));
        }

        private static ExpeditionPendingNodeEntry CreatePendingNodeEntry(
            string nodeId,
            bool isDynamic,
            string sourceNodeId,
            string sourceTransitionId,
            string reason)
        {
            return new ExpeditionPendingNodeEntry
            {
                QueueEntryId = Guid.NewGuid().ToString("N"),
                NodeId = nodeId,
                IsDynamic = isDynamic,
                SourceNodeId = sourceNodeId,
                SourceTransitionId = sourceTransitionId,
                Reason = reason,
            };
        }
    }

    [Serializable]
    public sealed class ExpeditionNodeRecord
    {
        public string QueueEntryId;
        public string NodeId;
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        public EnumExpeditionNodeProcessStatus Status;
        public int EntryOrder;
        public string SourceNodeId;
        public string SourceTransitionId;
        public string EnqueueReason;
        public bool WasDynamicallyInserted;
        public string RoutePolicy;
        public string ChosenOptionId;
        public int GainedMoney;
        public string ResolvedTransitionId;
        public string NextNodeId;
        public string QueueBeforeEnter;
        public string QueueAfterEnter;
        public string QueueBeforeRoute;
        public string QueueAfterRoute;
        public string BlackboardBefore;
        public string BlackboardAfter;
        public List<string> AppliedBuffIds = new List<string>();
        public List<string> EffectSummaries = new List<string>();
        public List<string> RouteDecisionLogs = new List<string>();
        public List<string> InsertedNodeIds = new List<string>();
        public string Summary;
        public CombatSessionResult CombatResult;
    }

    [Serializable]
    public sealed class ExpeditionResultSummary
    {
        public string ExpeditionId;
        public bool IsVictory;
        public EnumExpeditionEndReason EndReason;
        public int MoneyDelta;
        public List<ExpeditionMarbleSummary> MarbleSummaries = new List<ExpeditionMarbleSummary>();
        public List<string> NodeSummaries = new List<string>();

        public string ToDisplayText()
        {
            var status = IsVictory ? "远征成功" : "远征失败";
            var marbleLines = MarbleSummaries.Count == 0
                ? "无参战 Marble 记录"
                : string.Join("\n", MarbleSummaries.Select(summary => $"- {summary.DisplayName}: HP {summary.CurrentHp}/{summary.MaxHp} EXP {summary.Exp} {(summary.IsDead ? "[阵亡]" : "[存活]")}"));
            var nodeLines = NodeSummaries.Count == 0 ? "无节点记录" : string.Join("\n", NodeSummaries.Select(summary => $"- {summary}"));
            return $"{status}\n资源变化: +{MoneyDelta} 晶体\n\n队伍状态:\n{marbleLines}\n\n节点记录:\n{nodeLines}";
        }
    }

    [Serializable]
    public sealed class ExpeditionMarbleSummary
    {
        public string PersistentId;
        public string DisplayName;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;
    }

    public interface IExpeditionCondition
    {
        bool Evaluate(ExpeditionConditionExecutionContext context);
    }

    public sealed class ExpeditionConditionExecutionContext
    {
        public ExpeditionConditionExecutionContext(
            ExpeditionRunState runState,
            ExpeditionTable.ExpeditionRouteNodeConfig currentNode,
            ExpeditionNodeRecord currentRecord)
        {
            RunState = runState;
            CurrentNode = currentNode;
            CurrentRecord = currentRecord;
        }

        public ExpeditionRunState RunState { get; }

        public ExpeditionTable.ExpeditionRouteNodeConfig CurrentNode { get; }

        public ExpeditionNodeRecord CurrentRecord { get; }
    }

    public static class ExpeditionConditionFactory
    {
        public static bool AreAllSatisfied(
            IEnumerable<ExpeditionTable.ExpeditionConditionConfig> configs,
            ExpeditionConditionExecutionContext context)
        {
            if (configs == null)
            {
                return true;
            }

            foreach (var config in configs)
            {
                if (!CreateCondition(config).Evaluate(context))
                {
                    return false;
                }
            }

            return true;
        }

        public static IExpeditionCondition CreateCondition(ExpeditionTable.ExpeditionConditionConfig config)
        {
            return config switch
            {
                ExpeditionTable.HasFlagConditionConfig hasFlagConfig => new HasFlagCondition(hasFlagConfig),
                ExpeditionTable.HasItemConditionConfig hasItemConfig => new HasItemCondition(hasItemConfig),
                ExpeditionTable.HasChosenOptionConditionConfig chosenOptionConfig => new HasChosenOptionCondition(chosenOptionConfig),
                ExpeditionTable.CounterAtLeastConditionConfig counterConfig => new CounterAtLeastCondition(counterConfig),
                _ => new AlwaysFalseCondition(),
            };
        }
    }

    public sealed class HasFlagCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.HasFlagConditionConfig _config;

        public HasFlagCondition(ExpeditionTable.HasFlagConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return context?.RunState?.Blackboard?.HasFlag(_config.FlagId) ?? false;
        }
    }

    public sealed class HasItemCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.HasItemConditionConfig _config;

        public HasItemCondition(ExpeditionTable.HasItemConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return context?.RunState?.Blackboard?.HasItem(_config.ItemId) ?? false;
        }
    }

    public sealed class HasChosenOptionCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.HasChosenOptionConditionConfig _config;

        public HasChosenOptionCondition(ExpeditionTable.HasChosenOptionConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return context?.RunState?.Blackboard?.HasChosenOption(_config.OptionId) ?? false;
        }
    }

    public sealed class CounterAtLeastCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.CounterAtLeastConditionConfig _config;

        public CounterAtLeastCondition(ExpeditionTable.CounterAtLeastConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return (context?.RunState?.Blackboard?.GetCounterValue(_config.CounterId) ?? 0) >= _config.MinValue;
        }
    }

    public sealed class AlwaysFalseCondition : IExpeditionCondition
    {
        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return false;
        }
    }

    public sealed class ExpeditionRouteDecision
    {
        public string TransitionId;
        public string TargetNodeId;
        public string Summary;
    }

    public static class ExpeditionRouteResolver
    {
        public static ExpeditionRouteDecision Resolve(
            ExpeditionRunState runState,
            ExpeditionTable.ExpeditionRouteNodeConfig node,
            ExpeditionNodeRecord record)
        {
            if (runState == null || node == null)
            {
                return new ExpeditionRouteDecision
                {
                    Summary = "节点或运行态为空，无法解析出口。",
                };
            }

            switch (node.RoutePolicy)
            {
                case ExpeditionTable.EnumExpeditionRoutePolicy.FixedNext:
                    return ResolveFixedNext(node);
                case ExpeditionTable.EnumExpeditionRoutePolicy.BySelectedOption:
                    return ResolveBySelectedOption(node, record);
                case ExpeditionTable.EnumExpeditionRoutePolicy.ByConditions:
                    return ResolveByConditions(runState, node, record);
                default:
                    return new ExpeditionRouteDecision
                    {
                        Summary = $"节点 {node.NodeId} 使用了不支持的路由策略 {node.RoutePolicy}。",
                    };
            }
        }

        private static ExpeditionRouteDecision ResolveFixedNext(ExpeditionTable.ExpeditionRouteNodeConfig node)
        {
            if (string.IsNullOrWhiteSpace(node.DefaultTransitionId))
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeId} 为固定出口模式，但未配置默认出口，按叶子节点处理。",
                };
            }

            return ResolveTransition(node, node.DefaultTransitionId, $"节点 {node.NodeId} 按固定出口推进。");
        }

        private static ExpeditionRouteDecision ResolveBySelectedOption(
            ExpeditionTable.ExpeditionRouteNodeConfig node,
            ExpeditionNodeRecord record)
        {
            var optionId = record?.ChosenOptionId;
            if (string.IsNullOrWhiteSpace(optionId))
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeId} 需要根据选项出口，但当前没有选项输入。",
                };
            }

            var optionRoute = node.OptionRoutes?
                .FirstOrDefault(route => route != null && route.OptionId == optionId);
            if (optionRoute == null)
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeId} 没有找到选项 {optionId} 对应的出口映射。",
                };
            }

            return ResolveTransition(node, optionRoute.TransitionId, $"节点 {node.NodeId} 根据选项 {optionId} 选择出口。");
        }

        private static ExpeditionRouteDecision ResolveByConditions(
            ExpeditionRunState runState,
            ExpeditionTable.ExpeditionRouteNodeConfig node,
            ExpeditionNodeRecord record)
        {
            var context = new ExpeditionConditionExecutionContext(runState, node, record);
            var matchedTransition = node.Transitions?
                .Where(transition => transition != null && ExpeditionConditionFactory.AreAllSatisfied(transition.Conditions, context))
                .OrderByDescending(transition => transition.Priority)
                .FirstOrDefault();
            if (matchedTransition == null)
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeId} 的条件出口均未命中。",
                };
            }

            return new ExpeditionRouteDecision
            {
                TransitionId = matchedTransition.TransitionId,
                TargetNodeId = matchedTransition.TargetNodeId,
                Summary = $"节点 {node.NodeId} 命中条件出口 {matchedTransition.TransitionId}，优先级 {matchedTransition.Priority}。",
            };
        }

        private static ExpeditionRouteDecision ResolveTransition(
            ExpeditionTable.ExpeditionRouteNodeConfig node,
            string transitionId,
            string prefixSummary)
        {
            if (string.IsNullOrWhiteSpace(transitionId))
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"{prefixSummary} 但 transitionId 为空，按叶子节点处理。",
                };
            }

            var transition = node.Transitions?
                .FirstOrDefault(item => item != null && item.TransitionId == transitionId);
            if (transition == null)
            {
                return new ExpeditionRouteDecision
                {
                    TransitionId = transitionId,
                    Summary = $"{prefixSummary} 未找到 transition {transitionId}，按叶子节点处理。",
                };
            }

            return new ExpeditionRouteDecision
            {
                TransitionId = transition.TransitionId,
                TargetNodeId = transition.TargetNodeId,
                Summary = $"{prefixSummary} transition={transition.TransitionId} target={transition.TargetNodeId}",
            };
        }
    }

    public static class ExpeditionConfigBridge
    {
        public static ExpeditionRunState CreateConfiguredRun(IEnumerable<MarblePersistentData?> marbles, string preferredExpeditionId)
        {
            var expedition = ResolveStartupExpedition(preferredExpeditionId);
            if (expedition == null)
            {
                Log.Warning($"[Expedition] Unable to resolve startup expedition. PreferredId:{preferredExpeditionId}");
                return null;
            }

            if (!ValidateExpedition(expedition))
            {
                Log.Warning($"[Expedition] Expedition config validation failed. ExpeditionId:{expedition.ExpeditionId}");
                return null;
            }

            var runState = new ExpeditionRunState
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExpeditionId = expedition.ExpeditionId,
                Phase = EnumExpeditionFlowPhase.None,
                EndReason = EnumExpeditionEndReason.None,
                MarbleSnapshots = marbles?
                    .Where(marble => marble.HasValue && !marble.Value.IsDead)
                    .ToList() ?? new List<MarblePersistentData?>(),
                Route = expedition.Route?.Where(node => node != null).ToList() ?? new List<ExpeditionTable.ExpeditionRouteNodeConfig>(),
            };

            var firstNode = runState.Route.FirstOrDefault(node => node != null);
            if (firstNode != null)
            {
                runState.EnqueueNode(firstNode.NodeId, false, string.Empty, string.Empty, "initial_entry");
            }

            return runState;
        }

        public static ExpeditionTable.ExpeditionConfig ResolveStartupExpedition(string preferredExpeditionId)
        {
            var expeditionTable = ConfigSystem.Instance.Tables?.TbExpedition;
            if (expeditionTable == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(preferredExpeditionId))
            {
                var expedition = expeditionTable.GetOrDefault(preferredExpeditionId);
                if (expedition != null)
                {
                    return expedition;
                }
            }

            return expeditionTable.DataList.Count > 0 ? expeditionTable.DataList[0] : null;
        }

        public static ExpeditionTable.ExpeditionEventConfig ResolveEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return null;
            }

            return ConfigSystem.Instance.Tables?.TbExpeditionEvent?.GetOrDefault(eventId);
        }

        public static ExpeditionTable.ExpeditionCombatEncounterConfig ResolveCombatEncounter(string encounterId)
        {
            if (string.IsNullOrEmpty(encounterId))
            {
                return null;
            }

            return ConfigSystem.Instance.Tables?.TbExpeditionCombatEncounter?.GetOrDefault(encounterId);
        }

        public static ExpeditionTable.ExpeditionCombatEncounterConfig ResolveDebugCombatEncounter(string preferredExpeditionId)
        {
            var expedition = ResolveStartupExpedition(preferredExpeditionId);
            if (expedition?.Route != null)
            {
                foreach (var node in expedition.Route)
                {
                    if (node == null || node.NodeType != ExpeditionTable.EnumExpeditionNodeType.Combat)
                    {
                        continue;
                    }

                    var encounter = ResolveCombatEncounter(node.CombatEncounterId);
                    if (encounter != null)
                    {
                        return encounter;
                    }
                }
            }

            var encounterTable = ConfigSystem.Instance.Tables?.TbExpeditionCombatEncounter;
            return encounterTable != null && encounterTable.DataList.Count > 0 ? encounterTable.DataList[0] : null;
        }

        public static int ResolveMarbleMaxHp(string configId, int level)
        {
            try
            {
                var levelConfig = MarbleFactory.GetMarbleLevelConfig(configId, level);
                if (levelConfig != null)
                {
                    return levelConfig.Hp;
                }
            }
            catch (Exception exception)
            {
                Log.Warning($"[Expedition] ResolveMarbleMaxHp fallback for {configId}:{level}. {exception.Message}");
            }

            return 100;
        }

        private static bool ValidateExpedition(ExpeditionTable.ExpeditionConfig expedition)
        {
            if (expedition == null || expedition.Route == null || expedition.Route.Count == 0)
            {
                return false;
            }

            var nodeIdSet = new HashSet<string>();
            foreach (var node in expedition.Route)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.NodeId) || !nodeIdSet.Add(node.NodeId))
                {
                    return false;
                }

                switch (node.NodeType)
                {
                    case ExpeditionTable.EnumExpeditionNodeType.Event:
                        if (ResolveEvent(node.EventId) == null)
                        {
                            Log.Warning($"[Expedition] Missing event config for node:{node.NodeId} eventId:{node.EventId}");
                            return false;
                        }

                        break;
                    case ExpeditionTable.EnumExpeditionNodeType.Combat:
                        if (ResolveCombatEncounter(node.CombatEncounterId) == null)
                        {
                            Log.Warning($"[Expedition] Missing combat encounter config for node:{node.NodeId} encounterId:{node.CombatEncounterId}");
                            return false;
                        }

                        break;
                    default:
                        Log.Warning($"[Expedition] Unsupported node type:{node.NodeType} nodeId:{node.NodeId}");
                        return false;
                }

                if (!ValidateRoutePolicy(node))
                {
                    return false;
                }

                if (!ValidateTransitions(expedition, node))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateRoutePolicy(ExpeditionTable.ExpeditionRouteNodeConfig node)
        {
            switch (node.RoutePolicy)
            {
                case ExpeditionTable.EnumExpeditionRoutePolicy.FixedNext:
                    return true;
                case ExpeditionTable.EnumExpeditionRoutePolicy.BySelectedOption:
                    if (node.OptionRoutes == null || node.OptionRoutes.Count == 0)
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} uses BySelectedOption but has no option routes.");
                        return false;
                    }

                    return true;
                case ExpeditionTable.EnumExpeditionRoutePolicy.ByConditions:
                    if (node.Transitions == null || node.Transitions.Count == 0)
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} uses ByConditions but has no transitions.");
                        return false;
                    }

                    return true;
                default:
                    Log.Warning($"[Expedition] Unsupported route policy:{node.RoutePolicy} nodeId:{node.NodeId}");
                    return false;
            }
        }

        private static bool ValidateTransitions(ExpeditionTable.ExpeditionConfig expedition, ExpeditionTable.ExpeditionRouteNodeConfig node)
        {
            var transitionIds = new HashSet<string>();
            if (node.Transitions != null)
            {
                foreach (var transition in node.Transitions)
                {
                    if (transition == null || string.IsNullOrWhiteSpace(transition.TransitionId))
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} has an invalid transition entry.");
                        return false;
                    }

                    transitionIds.Add(transition.TransitionId);

                    if (string.IsNullOrWhiteSpace(transition.TargetNodeId))
                    {
                        continue;
                    }

                    var targetNodeExists = expedition.Route.Any(routeNode => routeNode != null && routeNode.NodeId == transition.TargetNodeId);
                    if (!targetNodeExists)
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} transition:{transition.TransitionId} targets missing node:{transition.TargetNodeId}");
                        return false;
                    }
                }
            }

            if (node.RoutePolicy == ExpeditionTable.EnumExpeditionRoutePolicy.BySelectedOption)
            {
                var eventConfig = ResolveEvent(node.EventId);
                foreach (var optionRoute in node.OptionRoutes)
                {
                    if (optionRoute == null || string.IsNullOrWhiteSpace(optionRoute.OptionId))
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} contains an invalid option route.");
                        return false;
                    }

                    var optionExists = eventConfig?.Options?.Any(option => option != null && option.OptionId == optionRoute.OptionId) ?? false;
                    if (!optionExists)
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} option route references missing option:{optionRoute.OptionId}");
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(optionRoute.TransitionId) && !transitionIds.Contains(optionRoute.TransitionId))
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} option route references missing transition:{optionRoute.TransitionId}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
