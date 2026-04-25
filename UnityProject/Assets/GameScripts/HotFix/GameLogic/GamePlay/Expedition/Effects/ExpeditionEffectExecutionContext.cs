using System.Collections.Generic;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionEffectExecutionContext
    {
        public ExpeditionEffectExecutionContext(ExpeditionRunState runState, ExpeditionPersistentDataStore persistentData, ExpeditionNodeRecord nodeRecord)
        {
            RunState = runState;
            PersistentData = persistentData;
            NodeRecord = nodeRecord;
        }

        public ExpeditionRunState RunState { get; }

        public ExpeditionPersistentDataStore PersistentData { get; }

        public ExpeditionNodeRecord NodeRecord { get; }

        public List<string> SummaryLines { get; } = new List<string>();

        public int AppliedMoneyDelta { get; private set; }

        public void AddMoneyDelta(int delta)
        {
            AppliedMoneyDelta += delta;
        }

        public void AddSummary(string summary)
        {
            var normalized = NormalizeSummary(summary);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                SummaryLines.Add(normalized);
            }
        }

        private static string NormalizeSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            var normalized = summary.Trim();

            return normalized;
        }
    }
}
