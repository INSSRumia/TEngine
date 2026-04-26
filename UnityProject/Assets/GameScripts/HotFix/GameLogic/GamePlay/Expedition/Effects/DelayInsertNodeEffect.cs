using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public class DelayInsertNodeEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.DelayInsertNodeEffectConfig _config;

        public DelayInsertNodeEffect(ExpeditionTable.DelayInsertNodeEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context?.RunState == null || _config == null)
            {
                return;
            }

            var pendingInsertNode = context.RunState.RegisterPendingInsertNode(
                _config.PassedNodeCount,
                _config.NodeType,
                _config.Id,
                context.NodeRecord?.NodeConfigId ?? string.Empty,
                context.NodeRecord?.QueueEntryInstId ?? string.Empty,
                BuildReason());
            if (pendingInsertNode == null)
            {
                context.NodeRecord?.AddRouteDecisionLog($"延迟插入节点 Effect 配置无效，已跳过。nodeType={_config.NodeType} id={_config.Id}");
                return;
            }

            var dictTokenValue = new Dictionary<string, string>
            {
                ["passed_node_count"] = _config.PassedNodeCount.ToString(),
                ["node_type"] = _config.NodeType.ToString(),
                ["id"] = _config.Id ?? string.Empty,
            };
            context.AddSummaryTemplate(
                _config.Summary,
                dictTokenValue,
                $"将在 {_config.PassedNodeCount} 个节点后插入 {pendingInsertNode.DebugLabel}。");
            context.NodeRecord?.AddRouteDecisionLog(
                $"登记延迟插入节点 {pendingInsertNode.DebugLabel}，remaining={pendingInsertNode.RemainingPassedNodeCount}。");
        }

        private string BuildReason()
        {
            if (!string.IsNullOrWhiteSpace(_config?.Summary))
            {
                return _config.Summary;
            }

            return $"delay_insert_node:{_config?.NodeType}:{_config?.Id}";
        }
    }
}
