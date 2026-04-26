using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionNodeRecord
    {
        public string QueueEntryInstId;
        public string NodeConfigId;
        public string DisplayNodeLabel;
        public EnumExpeditionNodeType NodeType;
        public EnumExpeditionNodeProcessStatus Status;
        public int EntryOrder;
        public string SourceNodeConfigId;
        public string SourceTransitionId;
        public string EnqueueReason;
        public bool WasDynamicallyInserted;
        public bool IsTemporaryRuntimeNode;
        public string RoutePolicy;
        public string ChosenOptionId;
        public string ActualEventConfigId;
        public string ActualCombatEncounterConfigId;
        public string RandomEventPoolConfigId;
        public bool WasRandomEventSkipped;
        public int GainedMoney;
        public string ResolvedTransitionId;
        public string NextNodeConfigId;
        public string QueueBeforeEnter;
        public string QueueAfterEnter;
        public string QueueBeforeRoute;
        public string QueueAfterRoute;
        public string BlackboardBefore;
        public string BlackboardAfter;
        public List<string> LstAppliedBuffId = new ();
        public List<string> LstEffectSummarie = new ();
        public List<string> LstRouteDecisionLog = new ();
        public List<string> LstInsertedNodeId = new ();
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
                $"#{record.EntryOrder} 节点 {(!string.IsNullOrWhiteSpace(record.DisplayNodeLabel) ? record.DisplayNodeLabel : record.NodeConfigId)}"
            };
            if (!string.IsNullOrWhiteSpace(record.Summary))
            {
                parts.Add(record.Summary);
            }

            if (!string.IsNullOrWhiteSpace(record.ResolvedTransitionId))
            {
                parts.Add($"出口 {record.ResolvedTransitionId} -> {record.NextNodeConfigId}");
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
        public string ExpeditionConfigId;
        public bool IsVictory;
        public EnumExpeditionEndReason EndReason;
        public int MoneyDelta;
        public List<ExpeditionMarbleSummary> LstMarbleSummarie = new ();
        public List<string> LstNodeSummarie = new ();

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
                ExpeditionConfigId = runState.ExpeditionConfigId,
                IsVictory = runState.EndReason == EnumExpeditionEndReason.Victory,
                EndReason = runState.EndReason,
                MoneyDelta = runState.TotalMoneyGained,
                LstMarbleSummarie = runState.LstMarbleSnapshot.Where(snapshot => snapshot.HasValue).Select(snapshot => new ExpeditionMarbleSummary
                {
                    MarbleInstId = snapshot.Value.MarbleInstId,
                    DisplayName = snapshot.Value.DisplayName,
                    CurrentHp = snapshot.Value.CurrentHp,
                    MaxHp = snapshot.Value.MaxHp,
                    Exp = snapshot.Value.Exp,
                    IsDead = snapshot.Value.IsDead,
                }).ToList(),
                LstNodeSummarie = runState.LstNodeRecord.Select(ExpeditionNodeRecord.BuildNodeSummary).Where(summary => !string.IsNullOrWhiteSpace(summary)).ToList(),
            };
        }
    }

    [Serializable]
    public sealed class ExpeditionMarbleSummary
    {
        public string MarbleInstId;
        public string DisplayName;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;
    }
}
