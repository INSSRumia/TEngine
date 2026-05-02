using System;
using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionPendingNodeEntry
    {
        // 这次排队条目的运行时实例 Id，用来和 NodeRecord 一一对应。
        public string QueueEntryInstId;
        // 要执行的节点标识。静态节点时是 node_config_id，临时节点时是运行时拼出来的调试 Id。
        public string NodeConfigId;
        // 是否为动态插入的节点。主线正常推进为 false。
        public bool IsDynamic;
        // 是否为运行时临时节点。临时节点不依赖静态 Route 配置。
        public bool IsTemporaryRuntimeNode;
        // 临时节点对应的节点类型；静态节点通常为 None。
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        // 临时 event 节点引用的 event_config_id。
        public string EventConfigId;
        // 临时 combat 节点引用的 combat_encounter_config_id。
        public string CombatEncounterConfigId;
        // 这个排队条目是由哪个节点推进或插入出来的。
        public string SourceNodeConfigId;
        // 如果来自静态出口推进，则记录命中的 transition_id。
        public string SourceTransitionId;
        // 如果来自延迟插入系统，则记录触发它的 pending insert 请求 Id。
        public string SourcePendingInsertInstId;
        // 人类可读的入队原因，主要用于调试和摘要。
        public string Reason;
        // 队列展示时使用的调试标签。
        public string DebugLabel;
    }

    [Serializable]
    public class ExpeditionRuntimeNode
    {
        // 当前真正参与流程判断的节点 Id。静态节点和临时节点都会被统一映射成这个结构。
        public string NodeConfigId;
        // 给日志和摘要展示的节点名称。
        public string DisplayNodeLabel;
        // 这次进入流程的节点类型。
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        // 是否为运行时临时节点。
        public bool IsTemporaryRuntimeNode;
        // 运行态节点解析成 event 时使用的 event_config_id。
        public string EventConfigId;
        // 运行态节点解析成 combat 时使用的 combat_encounter_config_id。
        public string CombatEncounterConfigId;
        // 当前节点在运行时视角下采用的路由策略文本。
        public string RoutePolicy;
        // 如果它来自静态 Route，这里保留原始配置；临时节点则为 null。
        public ExpeditionTable.ExpeditionRouteNodeConfig StaticNodeConfig;
    }

    [Serializable]
    public class ExpeditionPendingInsertNodeEntry
    {
        // 一条“若干节点后插入临时节点”的运行时请求 Id。
        public string PendingInsertInstId;
        // 还需要再经过多少次节点结算，才会真正插入队列。
        public int RemainingPassedNodeCount;
        // 登记顺序。多个条目同位到期时用它保证稳定行为。
        public int CreateOrder;
        // 到期后要插入的临时节点类型，目前只支持 event / combat。
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        // 临时 event 节点引用的 event_config_id。
        public string EventConfigId;
        // 临时 combat 节点引用的 combat_encounter_config_id。
        public string CombatEncounterConfigId;
        // 是哪个节点在结算时创建了这条延迟插入请求。
        public string SourceNodeConfigId;
        // 对应创建时的队列条目 Id，便于追日志。
        public string SourceQueueEntryInstId;
        // 业务原因或摘要文案。
        public string Reason;
        // 调试标签，例如 temp_event:xxx。
        public string DebugLabel;
        // 是否已经到期并被消费。
        public bool IsConsumed;
    }

    [Serializable]
    public class ExpeditionRandomEventPoolEntryState
    {
        // 这个池子条目实际引用的 event_config_id。
        public string EventConfigId;
        // 当前剩余权重。无放回抽取后，被抽中的条目会从列表中移除。
        public int Weight;

        public ExpeditionRandomEventPoolEntryState()
        {
        }

        // 从静态配置复制出一份运行时状态，后续可以安全移除已抽中的条目。
        public ExpeditionRandomEventPoolEntryState(ExpeditionTable.ExpeditionRandomEventPoolEntryConfig config)
        {
            EventConfigId = config?.EventConfigId ?? string.Empty;
            Weight = config?.Weight ?? 0;
        }
    }

    [Serializable]
    public class ExpeditionActiveRandomEventPoolState
    {
        // 这次激活的随机池运行时实例 Id。不同来源激活同一个池配置时也能区分。
        public string PoolRuntimeInstId;
        // 对应的随机池配置 Id。
        public string RandomEventPoolConfigId;
        // 激活来源类型，例如 expedition / environment。
        public string SourceType;
        // 激活来源的配置 Id，例如某个 environment_config_id。
        public string SourceConfigId;
        // 当前池内还没被抽走的剩余条目。
        public List<ExpeditionRandomEventPoolEntryState> LstRemainingEntry = new ();

        // 返回当前池内所有可抽取条目的总权重。
        public int GetTotalWeight()
        {
            return LstRemainingEntry?
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.EventConfigId) && entry.Weight > 0)
                .Sum(entry => entry.Weight) ?? 0;
        }
    }

    public class ExpeditionRandomEventDrawResult
    {
        // 这次抽取是否成功。
        public bool IsSuccess;
        // 抽中的 event_config_id。
        public string EventConfigId;
        // 抽中事件来自哪个随机池。
        public string RandomEventPoolConfigId;
        // 面向日志/调试的文字说明。
        public string Summary;
    }
}
