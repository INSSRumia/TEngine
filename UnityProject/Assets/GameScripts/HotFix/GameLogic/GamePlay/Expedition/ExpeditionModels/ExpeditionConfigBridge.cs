using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Gameplay.Combat.Marble;
using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionConfigBridge
    {
        public static ExpeditionRunState CreateConfiguredRun(IEnumerable<MarblePersistentData?> marbles, string preferredExpeditionId)
        {
            var expedition = ResolveStartupExpedition(preferredExpeditionId);
            if (expedition == null)
            {
                Log.Warning($"[远征] 无法解析启动远征配置。首选Id:{preferredExpeditionId}");
                return null;
            }

            if (!ValidateExpedition(expedition))
            {
                Log.Warning($"[远征] 远征配置校验失败。远征Id:{expedition.ExpeditionId}");
                return null;
            }

            var runState = new ExpeditionRunState
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExpeditionId = expedition.ExpeditionId,
                Phase = EnumExpeditionFlowPhase.None,
                EndReason = EnumExpeditionEndReason.None,
                MarbleSnapshots = marbles?
                    .Where(marble => marble.HasValue && !marble.Value.IsDead)
                    .ToList() ?? new List<MarblePersistentData?>(),
                Route = expedition.Route?.Where(node => node != null).ToList() ?? new List<ExpeditionTable.ExpeditionRouteNodeConfig>(),
            };

            var firstNode = runState.Route.FirstOrDefault(node => node != null);
            if (firstNode != null)
            {
                runState.EnqueueNode(firstNode.NodeId, false, string.Empty, string.Empty, "initial_entry");
            }

            return runState;
        }

        public static ExpeditionTable.ExpeditionConfig ResolveStartupExpedition(string preferredExpeditionId)
        {
            var expeditionTable = ConfigSystem.Instance.Tables?.TbExpedition;
            if (expeditionTable == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(preferredExpeditionId))
            {
                var expedition = expeditionTable.GetOrDefault(preferredExpeditionId);
                if (expedition != null)
                {
                    return expedition;
                }
            }

            return expeditionTable.DataList.Count > 0 ? expeditionTable.DataList[0] : null;
        }

        public static ExpeditionTable.ExpeditionEventConfig ResolveEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return null;
            }

            return ConfigSystem.Instance.Tables?.TbExpeditionEvent?.GetOrDefault(eventId);
        }

        public static ExpeditionTable.ExpeditionCombatEncounterConfig ResolveCombatEncounter(string encounterId)
        {
            if (string.IsNullOrEmpty(encounterId))
            {
                return null;
            }

            return ConfigSystem.Instance.Tables?.TbExpeditionCombatEncounter?.GetOrDefault(encounterId);
        }

        public static ExpeditionTable.ExpeditionCombatEncounterConfig ResolveDebugCombatEncounter(string preferredExpeditionId)
        {
            var expedition = ResolveStartupExpedition(preferredExpeditionId);
            if (expedition?.Route != null)
            {
                foreach (var node in expedition.Route)
                {
                    if (node == null || node.NodeType != ExpeditionTable.EnumExpeditionNodeType.Combat)
                    {
                        continue;
                    }

                    var encounter = ResolveCombatEncounter(node.CombatEncounterId);
                    if (encounter != null)
                    {
                        return encounter;
                    }
                }
            }

            var encounterTable = ConfigSystem.Instance.Tables?.TbExpeditionCombatEncounter;
            return encounterTable != null && encounterTable.DataList.Count > 0 ? encounterTable.DataList[0] : null;
        }

        public static int ResolveMarbleMaxHp(string configId, int level)
        {
            try
            {
                var levelConfig = MarbleFactory.GetMarbleLevelConfig(configId, level);
                if (levelConfig != null)
                {
                    return levelConfig.Hp;
                }
            }
            catch (Exception exception)
            {
                Log.Warning($"[远征] ResolveMarbleMaxHp 回退处理 {configId}:{level}。{exception.Message}");
            }

            return 100;
        }

        private static bool ValidateExpedition(ExpeditionTable.ExpeditionConfig expedition)
        {
            if (expedition == null || expedition.Route == null || expedition.Route.Count == 0)
            {
                return false;
            }

            var nodeIdSet = new HashSet<string>();
            foreach (var node in expedition.Route)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.NodeId) || !nodeIdSet.Add(node.NodeId))
                {
                    return false;
                }

                switch (node.NodeType)
                {
                    case ExpeditionTable.EnumExpeditionNodeType.Event:
                        if (ResolveEvent(node.EventId) == null)
                        {
                            Log.Warning($"[远征] 节点:{node.NodeId} 缺少事件配置 eventId:{node.EventId}");
                            return false;
                        }

                        break;
                    case ExpeditionTable.EnumExpeditionNodeType.Combat:
                        if (ResolveCombatEncounter(node.CombatEncounterId) == null)
                        {
                            Log.Warning($"[远征] 节点:{node.NodeId} 缺少战斗遭遇配置 encounterId:{node.CombatEncounterId}");
                            return false;
                        }

                        break;
                    default:
                        Log.Warning($"[远征] 不支持的节点类型:{node.NodeType} nodeId:{node.NodeId}");
                        return false;
                }

                if (!ValidateRoutePolicy(node))
                {
                    return false;
                }

                if (!ValidateTransitions(expedition, node))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateRoutePolicy(ExpeditionTable.ExpeditionRouteNodeConfig node)
        {
            switch (node.RoutePolicy)
            {
                case ExpeditionTable.EnumExpeditionRoutePolicy.FixedNext:
                    return true;
                case ExpeditionTable.EnumExpeditionRoutePolicy.BySelectedOption:
                    if (node.OptionRoutes == null || node.OptionRoutes.Count == 0)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 使用了 BySelectedOption 但没有选项路由。");
                        return false;
                    }

                    return true;
                case ExpeditionTable.EnumExpeditionRoutePolicy.ByConditions:
                    if (node.Transitions == null || node.Transitions.Count == 0)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 使用了 ByConditions 但没有转换条件。");
                        return false;
                    }

                    return true;
                default:
                    Log.Warning($"[远征] 不支持的路由策略:{node.RoutePolicy} nodeId:{node.NodeId}");
                    return false;
            }
        }

        private static bool ValidateTransitions(ExpeditionTable.ExpeditionConfig expedition, ExpeditionTable.ExpeditionRouteNodeConfig node)
        {
            var transitionIds = new HashSet<string>();
            if (node.Transitions != null)
            {
                foreach (var transition in node.Transitions)
                {
                    if (transition == null || string.IsNullOrWhiteSpace(transition.TransitionId))
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 包含无效的转换条目。");
                        return false;
                    }

                    transitionIds.Add(transition.TransitionId);

                    if (string.IsNullOrWhiteSpace(transition.TargetNodeId))
                    {
                        continue;
                    }

                    var targetNodeExists = expedition.Route.Any(routeNode => routeNode != null && routeNode.NodeId == transition.TargetNodeId);
                    if (!targetNodeExists)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 的转换:{transition.TransitionId} 指向不存在的节点:{transition.TargetNodeId}");
                        return false;
                    }
                }
            }

            if (node.RoutePolicy == ExpeditionTable.EnumExpeditionRoutePolicy.BySelectedOption)
            {
                var eventConfig = ResolveEvent(node.EventId);
                foreach (var optionRoute in node.OptionRoutes)
                {
                    if (optionRoute == null || string.IsNullOrWhiteSpace(optionRoute.OptionId))
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 包含无效的选项路由。");
                        return false;
                    }

                    var optionExists = eventConfig?.Options?.Any(option => option != null && option.OptionId == optionRoute.OptionId) ?? false;
                    if (!optionExists)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 选项路由引用了不存在的选项:{optionRoute.OptionId}");
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(optionRoute.TransitionId) && !transitionIds.Contains(optionRoute.TransitionId))
                    {
                        Log.Warning($"[远征] 节点:{node.NodeId} 选项路由引用了不存在的转换:{optionRoute.TransitionId}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
