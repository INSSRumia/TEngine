using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Gameplay.Combat.Marble;
using TEngine;
using GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionConfigBridge
    {
        public static ExpeditionRunState CreateConfiguredRun(IEnumerable<MarblePersistentData?> marbles, string preferredExpeditionConfigId)
        {
            var expedition = ResolveStartupExpedition(preferredExpeditionConfigId);
            if (expedition == null)
            {
                Log.Warning($"[远征] 无法解析启动远征配置。首选ConfigId:{preferredExpeditionConfigId}");
                return null;
            }

            if (!ValidateExpedition(expedition))
            {
                Log.Warning($"[远征] 远征配置校验失败。远征ConfigId:{expedition.ExpeditionConfigId}");
                return null;
            }

            var runState = new ExpeditionRunState
            {
                ExpeditionInstId = Guid.NewGuid().ToString("N"),
                ExpeditionConfigId = expedition.ExpeditionConfigId,
                Phase = EnumExpeditionFlowPhase.None,
                EndReason = EnumExpeditionEndReason.None,
                MarbleSnapshots = marbles?
                    .Where(marble => marble.HasValue && !marble.Value.IsDead)
                    .ToList() ?? new List<MarblePersistentData?>(),
                Route = expedition.Route?.Where(node => node != null).ToList() ?? new List<ExpeditionRouteNodeConfig>(),
            };

            var firstNode = runState.Route.FirstOrDefault(node => node != null);
            if (firstNode != null)
            {
                runState.EnqueueNode(firstNode.NodeConfigId, false, string.Empty, string.Empty, "initial_entry");
            }

            return runState;
        }

        public static ExpeditionConfig ResolveStartupExpedition(string preferredExpeditionConfigId)
        {
            var expeditionTable = ConfigSystem.Instance.Tables?.TbExpedition;
            if (expeditionTable == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(preferredExpeditionConfigId))
            {
                var expedition = expeditionTable.GetOrDefault(preferredExpeditionConfigId);
                if (expedition != null)
                {
                    return expedition;
                }
            }

            return expeditionTable.DataList.Count > 0 ? expeditionTable.DataList[0] : null;
        }

        public static ExpeditionEventConfig ResolveEvent(string eventConfigId)
        {
            if (string.IsNullOrEmpty(eventConfigId))
            {
                return null;
            }

            return ConfigSystem.Instance.Tables?.TbExpeditionEvent?.GetOrDefault(eventConfigId);
        }

        public static ExpeditionCombatEncounterConfig ResolveCombatEncounter(string combatEncounterConfigId)
        {
            if (string.IsNullOrEmpty(combatEncounterConfigId))
            {
                return null;
            }

            return ConfigSystem.Instance.Tables?.TbExpeditionCombatEncounter?.GetOrDefault(combatEncounterConfigId);
        }

        public static ExpeditionCombatEncounterConfig ResolveDebugCombatEncounter(string preferredExpeditionConfigId)
        {
            var expedition = ResolveStartupExpedition(preferredExpeditionConfigId);
            if (expedition?.Route != null)
            {
                foreach (var node in expedition.Route)
                {
                    if (node == null || node.NodeType != EnumExpeditionNodeType.Combat)
                    {
                        continue;
                    }

                    var encounter = ResolveCombatEncounter(node.CombatEncounterConfigId);
                    if (encounter != null)
                    {
                        return encounter;
                    }
                }
            }

            var encounterTable = ConfigSystem.Instance.Tables?.TbExpeditionCombatEncounter;
            return encounterTable != null && encounterTable.DataList.Count > 0 ? encounterTable.DataList[0] : null;
        }

        public static int ResolveMarbleMaxHp(string marbleConfigId, int level)
        {
            try
            {
                var levelConfig = MarbleFactory.GetMarbleLevelConfig(marbleConfigId, level);
                if (levelConfig != null)
                {
                    return levelConfig.Hp;
                }
            }
            catch (Exception exception)
            {
                Log.Warning($"[远征] ResolveMarbleMaxHp 回退处理 {marbleConfigId}:{level}。{exception.Message}");
            }

            return 100;
        }

        private static bool ValidateExpedition(ExpeditionConfig expedition)
        {
            if (expedition == null || expedition.Route == null || expedition.Route.Count == 0)
            {
                return false;
            }

            var nodeIdSet = new HashSet<string>();
            foreach (var node in expedition.Route)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.NodeConfigId) || !nodeIdSet.Add(node.NodeConfigId))
                {
                    return false;
                }

                switch (node.NodeType)
                {
                    case EnumExpeditionNodeType.Event:
                        if (ResolveEvent(node.EventConfigId) == null)
                        {
                            Log.Warning($"[远征] 节点:{node.NodeConfigId} 缺少事件配置 eventConfigId:{node.EventConfigId}");
                            return false;
                        }

                        break;
                    case EnumExpeditionNodeType.Combat:
                        if (ResolveCombatEncounter(node.CombatEncounterConfigId) == null)
                        {
                            Log.Warning($"[远征] 节点:{node.NodeConfigId} 缺少战斗遭遇配置 combatEncounterConfigId:{node.CombatEncounterConfigId}");
                            return false;
                        }

                        break;
                    default:
                        Log.Warning($"[远征] 不支持的节点类型:{node.NodeType} nodeConfigId:{node.NodeConfigId}");
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

        private static bool ValidateRoutePolicy(ExpeditionRouteNodeConfig node)
        {
            switch (node.RoutePolicy)
            {
                case EnumExpeditionRoutePolicy.FixedNext:
                    return true;
                case EnumExpeditionRoutePolicy.BySelectedOption:
                    if (node.OptionRoutes == null || node.OptionRoutes.Count == 0)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 使用了 BySelectedOption 但没有选项路由。");
                        return false;
                    }

                    return true;
                case EnumExpeditionRoutePolicy.ByConditions:
                    if (node.Transitions == null || node.Transitions.Count == 0)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 使用了 ByConditions 但没有转换条件。");
                        return false;
                    }

                    return true;
                default:
                    Log.Warning($"[远征] 不支持的路由策略:{node.RoutePolicy} nodeConfigId:{node.NodeConfigId}");
                    return false;
            }
        }

        private static bool ValidateTransitions(ExpeditionConfig expedition, ExpeditionRouteNodeConfig node)
        {
            var transitionIds = new HashSet<string>();
            if (node.Transitions != null)
            {
                foreach (var transition in node.Transitions)
                {
                    if (transition == null || string.IsNullOrWhiteSpace(transition.TransitionId))
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 包含无效的转换条目。");
                        return false;
                    }

                    transitionIds.Add(transition.TransitionId);

                    if (string.IsNullOrWhiteSpace(transition.TargetNodeConfigId))
                    {
                        continue;
                    }

                    var targetNodeExists = expedition.Route.Any(routeNode => routeNode != null && routeNode.NodeConfigId == transition.TargetNodeConfigId);
                    if (!targetNodeExists)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 的转换:{transition.TransitionId} 指向不存在的节点:{transition.TargetNodeConfigId}");
                        return false;
                    }
                }
            }

            if (node.RoutePolicy == EnumExpeditionRoutePolicy.BySelectedOption)
            {
                var eventConfig = ResolveEvent(node.EventConfigId);
                foreach (var optionRoute in node.OptionRoutes)
                {
                    if (optionRoute == null || string.IsNullOrWhiteSpace(optionRoute.OptionId))
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 包含无效的选项路由。");
                        return false;
                    }

                    var optionExists = eventConfig?.Options?.Any(option => option != null && option.OptionId == optionRoute.OptionId) ?? false;
                    if (!optionExists)
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 选项路由引用了不存在的选项:{optionRoute.OptionId}");
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(optionRoute.TransitionId) && !transitionIds.Contains(optionRoute.TransitionId))
                    {
                        Log.Warning($"[远征] 节点:{node.NodeConfigId} 选项路由引用了不存在的转换:{optionRoute.TransitionId}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
