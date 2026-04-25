using System;
using System.Linq;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionRouteDecision
    {
        public string TransitionId;
        public string TargetNodeConfigId;
        public string Summary;
    }

    public static class ExpeditionRouteResolver
    {
        public static ExpeditionRouteDecision Resolve(
            ExpeditionRunState runState,
            ExpeditionRouteNodeConfig node,
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
                case EnumExpeditionRoutePolicy.FixedNext:
                    return ResolveFixedNext(node);
                case EnumExpeditionRoutePolicy.BySelectedOption:
                    return ResolveBySelectedOption(node, record);
                case EnumExpeditionRoutePolicy.ByConditions:
                    return ResolveByConditions(runState, node, record);
                default:
                    return new ExpeditionRouteDecision
                    {
                        Summary = $"节点 {node.NodeConfigId} 使用了不支持的路由策略 {node.RoutePolicy}。",
                    };
            }
        }

        private static ExpeditionRouteDecision ResolveFixedNext(ExpeditionRouteNodeConfig node)
        {
            var transition = node.Transitions?
                .Where(item => item != null)
                .OrderByDescending(item => item.Priority)
                .FirstOrDefault();
            if (transition == null)
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeConfigId} 为固定出口模式，但未配置任何出口，按叶子节点处理。",
                };
            }

            return new ExpeditionRouteDecision
            {
                TransitionId = transition.TransitionId,
                TargetNodeConfigId = transition.TargetNodeConfigId,
                Summary = $"节点 {node.NodeConfigId} 按固定出口推进，选择优先级最高的出口 {transition.TransitionId}，优先级 {transition.Priority}。",
            };
        }

        private static ExpeditionRouteDecision ResolveBySelectedOption(
            ExpeditionRouteNodeConfig node,
            ExpeditionNodeRecord record)
        {
            var optionId = record?.ChosenOptionId;
            if (string.IsNullOrWhiteSpace(optionId))
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeConfigId} 需要根据选项出口，但当前没有选项输入。",
                };
            }

            var optionRoute = node.OptionRoutes?
                .FirstOrDefault(route => route != null && route.OptionId == optionId);
            if (optionRoute == null)
            {
                return new ExpeditionRouteDecision
                {
                    Summary = $"节点 {node.NodeConfigId} 没有找到选项 {optionId} 对应的出口映射。",
                };
            }

            return ResolveTransition(node, optionRoute.TransitionId, $"节点 {node.NodeConfigId} 根据选项 {optionId} 选择出口。");
        }

        private static ExpeditionRouteDecision ResolveByConditions(
            ExpeditionRunState runState,
            ExpeditionRouteNodeConfig node,
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
                    Summary = $"节点 {node.NodeConfigId} 的条件出口均未命中。",
                };
            }

            return new ExpeditionRouteDecision
            {
                TransitionId = matchedTransition.TransitionId,
                TargetNodeConfigId = matchedTransition.TargetNodeConfigId,
                Summary = $"节点 {node.NodeConfigId} 命中条件出口 {matchedTransition.TransitionId}，优先级 {matchedTransition.Priority}。",
            };
        }

        private static ExpeditionRouteDecision ResolveTransition(
            ExpeditionRouteNodeConfig node,
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
                TargetNodeConfigId = transition.TargetNodeConfigId,
                Summary = $"{prefixSummary} transition={transition.TransitionId} target={transition.TargetNodeConfigId}",
            };
        }
    }
}
