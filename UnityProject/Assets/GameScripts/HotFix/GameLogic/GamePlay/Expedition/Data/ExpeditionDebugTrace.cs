using System;
using System.Collections.Generic;

namespace GameLogic.Gameplay.Expedition
{
    public enum EnumExpeditionDebugTraceSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    [Serializable]
    public class ExpeditionDebugTraceEntry
    {
        // 会话内自增序号，便于按记录顺序回放。
        public long Sequence;
        // 追踪分类，例如 Environment / Combat / Queue。
        public string Category;
        // 当前追踪条目的严重级别。
        public EnumExpeditionDebugTraceSeverity Severity;
        // 面向开发期阅读的追踪消息。
        public string Message;
        // 如果本条追踪与某个节点相关，则记录该节点的 ConfigId。
        public string NodeConfigId;
        // 如果本条追踪与某个排队条目相关，则记录它的运行时队列实例 Id。
        public string QueueEntryInstId;
        // 写入追踪时远征所处的流程阶段。
        public EnumExpeditionFlowPhase Phase;
    }

    [Serializable]
    public class ExpeditionDebugTrace
    {
        // 当前远征会话积累的所有调试追踪条目。
        public List<ExpeditionDebugTraceEntry> LstEntry = new ();

        private long _nextSequence;

        public void Record(
            string category,
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            LstEntry.Add(new ExpeditionDebugTraceEntry
            {
                Sequence = ++_nextSequence,
                Category = category ?? string.Empty,
                Severity = severity,
                Message = message,
                NodeConfigId = nodeConfigId ?? string.Empty,
                QueueEntryInstId = queueEntryInstId ?? string.Empty,
                Phase = phase,
            });
        }

        public void RecordEnvironment(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("Environment", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordRandomEvent(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("RandomEvent", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordRandomEventPool(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("RandomEventPool", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordPendingInsert(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("PendingInsert", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordQueue(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("Queue", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordCombat(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("Combat", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordEffect(
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "",
            EnumExpeditionDebugTraceSeverity severity = EnumExpeditionDebugTraceSeverity.Info)
        {
            Record("Effect", message, phase, nodeConfigId, queueEntryInstId, severity);
        }

        public void RecordWarning(
            string category,
            string message,
            EnumExpeditionFlowPhase phase,
            string nodeConfigId = "",
            string queueEntryInstId = "")
        {
            Record(category, message, phase, nodeConfigId, queueEntryInstId, EnumExpeditionDebugTraceSeverity.Warning);
        }
    }
}
