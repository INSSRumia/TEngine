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
                Log.Warning($"[Expedition] Unable to resolve startup expedition. PreferredId:{preferredExpeditionId}");
                return null;
            }

            if (!ValidateExpedition(expedition))
            {
                Log.Warning($"[Expedition] Expedition config validation failed. ExpeditionId:{expedition.ExpeditionId}");
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
                Log.Warning($"[Expedition] ResolveMarbleMaxHp fallback for {configId}:{level}. {exception.Message}");
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
                            Log.Warning($"[Expedition] Missing event config for node:{node.NodeId} eventId:{node.EventId}");
                            return false;
                        }

                        break;
                    case ExpeditionTable.EnumExpeditionNodeType.Combat:
                        if (ResolveCombatEncounter(node.CombatEncounterId) == null)
                        {
                            Log.Warning($"[Expedition] Missing combat encounter config for node:{node.NodeId} encounterId:{node.CombatEncounterId}");
                            return false;
                        }

                        break;
                    default:
                        Log.Warning($"[Expedition] Unsupported node type:{node.NodeType} nodeId:{node.NodeId}");
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
                        Log.Warning($"[Expedition] Node:{node.NodeId} uses BySelectedOption but has no option routes.");
                        return false;
                    }

                    return true;
                case ExpeditionTable.EnumExpeditionRoutePolicy.ByConditions:
                    if (node.Transitions == null || node.Transitions.Count == 0)
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} uses ByConditions but has no transitions.");
                        return false;
                    }

                    return true;
                default:
                    Log.Warning($"[Expedition] Unsupported route policy:{node.RoutePolicy} nodeId:{node.NodeId}");
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
                        Log.Warning($"[Expedition] Node:{node.NodeId} has an invalid transition entry.");
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
                        Log.Warning($"[Expedition] Node:{node.NodeId} transition:{transition.TransitionId} targets missing node:{transition.TargetNodeId}");
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
                        Log.Warning($"[Expedition] Node:{node.NodeId} contains an invalid option route.");
                        return false;
                    }

                    var optionExists = eventConfig?.Options?.Any(option => option != null && option.OptionId == optionRoute.OptionId) ?? false;
                    if (!optionExists)
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} option route references missing option:{optionRoute.OptionId}");
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(optionRoute.TransitionId) && !transitionIds.Contains(optionRoute.TransitionId))
                    {
                        Log.Warning($"[Expedition] Node:{node.NodeId} option route references missing transition:{optionRoute.TransitionId}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
