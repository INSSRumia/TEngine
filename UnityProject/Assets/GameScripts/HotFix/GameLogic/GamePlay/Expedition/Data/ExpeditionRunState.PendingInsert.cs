using System;
using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed partial class ExpeditionRunState
    {
        // 把一个临时 event/combat 节点直接插到队首。它不会写回静态 Route。
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

        // 登记一条“若干节点后插入临时节点”的延迟请求。
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
                DebugTrace.RecordPendingInsert(
                    $"无效 passedNodeCount={passedNodeCount}",
                    Phase,
                    sourceNodeConfigId,
                    sourceQueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Warning);
                return null;
            }

            if (string.IsNullOrWhiteSpace(referencedConfigId))
            {
                DebugTrace.RecordPendingInsert(
                    "引用配置为空，忽略本次登记。",
                    Phase,
                    sourceNodeConfigId,
                    sourceQueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Warning);
                return null;
            }

            if (nodeType != ExpeditionTable.EnumExpeditionNodeType.Event
                && nodeType != ExpeditionTable.EnumExpeditionNodeType.Combat)
            {
                DebugTrace.RecordPendingInsert(
                    $"不支持的临时节点类型 {nodeType}",
                    Phase,
                    sourceNodeConfigId,
                    sourceQueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Warning);
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
            DebugTrace.RecordPendingInsert(
                $"登记 {entry.DebugLabel} remaining={entry.RemainingPassedNodeCount} source={entry.SourceNodeConfigId}",
                Phase,
                sourceNodeConfigId,
                sourceQueueEntryInstId);
            return entry;
        }

        // 在每次节点结算后统一递减所有延迟插入请求，并把到期节点插到队首。
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
                DebugTrace.RecordPendingInsert(
                    $"递减 {pendingInsertNode.DebugLabel} remaining={pendingInsertNode.RemainingPassedNodeCount}",
                    Phase,
                    sourceNodeConfigId,
                    pendingInsertNode.SourceQueueEntryInstId);
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
                    DebugTrace.RecordPendingInsert(
                        $"到期插入失败 {expiredEntry.DebugLabel}",
                        Phase,
                        sourceNodeConfigId,
                        expiredEntry.SourceQueueEntryInstId,
                        EnumExpeditionDebugTraceSeverity.Warning);
                    continue;
                }

                lstInsertedEntry.Add(insertedEntry);
                DebugTrace.RecordPendingInsert(
                    $"到期插入 {expiredEntry.DebugLabel}",
                    Phase,
                    sourceNodeConfigId,
                    expiredEntry.SourceQueueEntryInstId);
            }

            LstPendingInsertNode.RemoveAll(item => item == null || item.IsConsumed);
            return lstInsertedEntry;
        }

        // 把一个队列条目解析成统一运行时节点。
        // 静态节点会回指原始 Route 配置；临时节点会走专门的临时解析逻辑。
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

        // 把临时队列条目解析成 event/combat 节点，并在这里校验引用配置是否存在。
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

        // 创建临时节点对应的队列条目。
        // 这里会把 event/combat 的真实配置 Id 包进条目里，后续由运行时节点再解析。
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

        // 给临时节点拼一个统一调试标签，方便在日志和队列文本里辨认。
        private static string BuildTemporaryNodeLabel(ExpeditionTable.EnumExpeditionNodeType nodeType, string referencedConfigId)
        {
            return nodeType == ExpeditionTable.EnumExpeditionNodeType.Event
                ? $"temp_event:{referencedConfigId}"
                : $"temp_combat:{referencedConfigId}";
        }

        // 根据临时节点类型，取出它真正引用的配置 Id。
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
    }
}
