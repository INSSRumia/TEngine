using System;
using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
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
}
