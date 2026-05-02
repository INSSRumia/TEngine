using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameLogic.Gameplay.Expedition
{
    public class ExpeditionEffectExecutionContext
    {
        private static readonly Regex _summaryTokenRegex = new Regex(@"\{(?<token>[a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

        public ExpeditionEffectExecutionContext(ExpeditionRunState runState, ExpeditionPersistentDataStore persistentData, ExpeditionNodeRecord nodeRecord)
        {
            RunState = runState;
            PersistentData = persistentData;
            NodeRecord = nodeRecord;
            RewardContext = ExpeditionRewardContext.Create(runState, nodeRecord);
        }

        public ExpeditionRunState RunState { get; }

        public ExpeditionPersistentDataStore PersistentData { get; }

        public ExpeditionNodeRecord NodeRecord { get; }

        public ExpeditionRewardContext RewardContext { get; }

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
                SummaryLines.Add(normalized);
        }

        public void AddSummaryTemplate(string summaryTemplate, IReadOnlyDictionary<string, string> dictTokenValue, string fallbackSummary)
        {
            var template = string.IsNullOrWhiteSpace(summaryTemplate) ? fallbackSummary : summaryTemplate;
            AddSummary(RenderSummaryTemplate(template, dictTokenValue));
        }

        private static string NormalizeSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return string.Empty;

            var normalized = summary.Trim();

            return normalized;
        }

        private string RenderSummaryTemplate(string template, IReadOnlyDictionary<string, string> dictTokenValue)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            return _summaryTokenRegex.Replace(template, match =>
            {
                var token = match.Groups["token"].Value;
                if (dictTokenValue != null && dictTokenValue.TryGetValue(token, out var value))
                    return value ?? string.Empty;

                var diagnostic = $"Summary token 缺失 token={token} template={template}";
                RunState?.DebugTrace?.RecordEffect(
                    diagnostic,
                    RunState?.Phase ?? EnumExpeditionFlowPhase.None,
                    NodeRecord?.NodeConfigId,
                    NodeRecord?.QueueEntryInstId,
                    EnumExpeditionDebugTraceSeverity.Warning);
                return match.Value;
            });
        }
    }
}
