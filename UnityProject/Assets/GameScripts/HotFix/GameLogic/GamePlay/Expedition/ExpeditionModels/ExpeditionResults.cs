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
        public List<string> LstAppliedBuffId = new List<string>();
        public List<string> LstEffectSummarie = new List<string>();
        public List<string> LstRouteDecisionLog = new List<string>();
        public List<string> LstInsertedNodeId = new List<string>();
        public string Summary;
        public CombatSessionResult CombatResult;

        public void RecordEffectSummary(int gainedMoney, IEnumerable<string> lstSummaryLines)
        {
            GainedMoney = gainedMoney;
            LstEffectSummarie = FilterSummaryLines(lstSummaryLines);
            Summary = JoinSummaryLines(LstEffectSummarie);
        }

        public void RecordCombatSummary(CombatSessionResult combatResult, int gainedMoney, IEnumerable<string> lstSummaryLines)
        {
            CombatResult = combatResult;
            GainedMoney = gainedMoney;
            LstEffectSummarie = FilterSummaryLines(lstSummaryLines);

            var lstSummaryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(combatResult?.Summary))
                lstSummaryParts.Add(combatResult.Summary);

            lstSummaryParts.AddRange(LstEffectSummarie);
            Summary = JoinSummaryLines(lstSummaryParts);
        }

        public void AddRouteDecisionLog(string log)
        {
            if (string.IsNullOrWhiteSpace(log))
                return;

            LstRouteDecisionLog.Add(log);
        }

        public void RecordInsertedNodeIds(IEnumerable<string> lstNodeIds)
        {
            if (lstNodeIds == null)
                return;

            LstInsertedNodeId.AddRange(lstNodeIds.Where(nodeId => !string.IsNullOrWhiteSpace(nodeId)));
            if (LstInsertedNodeId.Count == 0)
                return;

            AddRouteDecisionLog($"动态插入节点: {string.Join(", ", LstInsertedNodeId)}");
        }

        public static string BuildNodeSummary(ExpeditionNodeRecord record)
        {
            if (record == null)
                return string.Empty;

            var parts = new List<string>
            {
                $"#{record.EntryOrder} 节点 {record.NodeId}"
            };
            if (!string.IsNullOrWhiteSpace(record.Summary))
            {
                parts.Add(record.Summary);
            }

            if (!string.IsNullOrWhiteSpace(record.ResolvedTransitionId))
            {
                parts.Add($"出口 {record.ResolvedTransitionId} -> {record.NextNodeId}");
            }

            if (record.LstInsertedNodeId.Count > 0)
            {
                parts.Add($"插入节点 [{string.Join(",", record.LstInsertedNodeId)}]");
            }

            if (record.LstRouteDecisionLog.Count > 0)
            {
                parts.Add(string.Join(" / ", record.LstRouteDecisionLog));
            }

            return string.Join(" | ", parts);
        }

        private static List<string> FilterSummaryLines(IEnumerable<string> lstSummaryLines)
        {
            if (lstSummaryLines == null)
                return new List<string>();

            return lstSummaryLines.Where(summary => !string.IsNullOrWhiteSpace(summary)).ToList();
        }

        private static string JoinSummaryLines(IEnumerable<string> lstSummaryLines)
        {
            if (lstSummaryLines == null)
                return string.Empty;

            return string.Join("\n", lstSummaryLines.Where(summary => !string.IsNullOrWhiteSpace(summary)));
        }
    }

    [Serializable]
    public sealed class ExpeditionResultSummary
    {
        public string ExpeditionId;
        public bool IsVictory;
        public EnumExpeditionEndReason EndReason;
        public int MoneyDelta;
        public List<ExpeditionMarbleSummary> LstMarbleSummarie = new List<ExpeditionMarbleSummary>();
        public List<string> LstNodeSummarie = new List<string>();

        public string ToDisplayText()
        {
            var status = IsVictory ? "远征成功" : "远征失败";
            var marbleLines = LstMarbleSummarie.Count == 0
                ? "无参战 Marble 记录"
                : string.Join("\n", LstMarbleSummarie.Select(summary => $"- {summary.DisplayName}: HP {summary.CurrentHp}/{summary.MaxHp} EXP {summary.Exp} {(summary.IsDead ? "[阵亡]" : "[存活]")}"));
            var nodeLines = LstNodeSummarie.Count == 0 ? "无节点记录" : string.Join("\n", LstNodeSummarie.Select(summary => $"- {summary}"));
            return $"{status}\n资源变化: +{MoneyDelta} 晶体\n\n队伍状态:\n{marbleLines}\n\n节点记录:\n{nodeLines}";
        }
        public static ExpeditionResultSummary BuildResultSummary(ExpeditionRunState runState)
        {
            return new ExpeditionResultSummary
            {
                ExpeditionId = runState.ExpeditionId,
                IsVictory = runState.EndReason == EnumExpeditionEndReason.Victory,
                EndReason = runState.EndReason,
                MoneyDelta = runState.TotalMoneyGained,
                LstMarbleSummarie = runState.MarbleSnapshots.Where(snapshot => snapshot.HasValue).Select(snapshot => new ExpeditionMarbleSummary
                {
                    PersistentId = snapshot.Value.PersistentId,
                    DisplayName = snapshot.Value.DisplayName,
                    CurrentHp = snapshot.Value.CurrentHp,
                    MaxHp = snapshot.Value.MaxHp,
                    Exp = snapshot.Value.Exp,
                    IsDead = snapshot.Value.IsDead,
                }).ToList(),
                LstNodeSummarie = runState.NodeRecords.Select(ExpeditionNodeRecord.BuildNodeSummary).Where(summary => !string.IsNullOrWhiteSpace(summary)).ToList(),
            };
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
