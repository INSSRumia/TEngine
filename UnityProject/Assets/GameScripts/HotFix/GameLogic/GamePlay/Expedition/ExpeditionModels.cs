using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Gameplay.Combat.Marble;
using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionConstants
    {
        public const string MinimalExpeditionId = "MinimalExpedition";
        public const int PlayerCamp = 1;
        public const int EnemyCamp = 2;
    }

    public enum EnumExpeditionFlowPhase
    {
        None = 0,
        Preparing = 1,
        EnteringNode = 2,
        WaitingEventChoice = 3,
        InCombat = 4,
        ApplyingNodeResult = 5,
        Settling = 6,
        Finished = 7,
    }

    public enum EnumExpeditionEndReason
    {
        None = 0,
        Victory = 1,
        Defeat = 2,
    }

    public enum EnumExpeditionNodeProcessStatus
    {
        Pending = 0,
        Entered = 1,
        Resolved = 2,
    }

    [Serializable]
    public partial struct MarblePersistentData
    {
        public string PersistentId;
        public string ConfigId;
        public string DisplayName;
        public int Level;
        public int CurrentHp;
        public int MaxHp;
        public int Exp;
        public bool IsDead;

        public static MarblePersistentData CreateDefault(string persistentId, string configId, string displayName, int level)
        {
            var maxHp = ExpeditionConfigBridge.ResolveMarbleMaxHp(configId, level);
            return new MarblePersistentData
            {
                PersistentId = persistentId,
                ConfigId = configId,
                DisplayName = displayName,
                Level = level,
                CurrentHp = maxHp,
                MaxHp = maxHp,
                Exp = 0,
                IsDead = false,
            };
        }
    }

    [Serializable]
    public sealed class ExpeditionPersistentDataStore
    {
        public int Crystal;
        public List<MarblePersistentData?> Marbles = new List<MarblePersistentData?>();
        public ExpeditionResultSummary LastResult;

        public void EnsureInitialized()
        {
            if (Marbles.Count > 0)
            {
                return;
            }

            Crystal = 0;
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_1", "lancer", "先锋一号", 0));
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_2", "archer", "先锋二号", 0));
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_3", "soldier", "先锋三号", 0));
        }

        public MarblePersistentData? GetMarble(string persistentId)
        {
            return Marbles.Find(marble => marble.HasValue && marble.Value.PersistentId == persistentId);
        }

        public void SetMarble(MarblePersistentData marble)
        {
            for (int i = 0; i < Marbles.Count; i++)
            {
                if (!Marbles[i].HasValue || Marbles[i].Value.PersistentId != marble.PersistentId)
                {
                    continue;
                }

                Marbles[i] = marble;
                return;
            }

            Marbles.Add(marble);
        }
    }

    [Serializable]
    public sealed class ExpeditionRunState
    {
        public string RunId;
        public string ExpeditionId;
        public EnumExpeditionFlowPhase Phase;
        public EnumExpeditionEndReason EndReason;
        public int CurrentNodeIndex;
        public int TotalCrystalGained;
        public List<MarblePersistentData?> MarbleSnapshots = new List<MarblePersistentData?>();
        public List<ExpeditionTable.ExpeditionRouteNodeConfig> Route = new List<ExpeditionTable.ExpeditionRouteNodeConfig>();
        public List<ExpeditionNodeRecord> NodeRecords = new List<ExpeditionNodeRecord>();
        public string PendingEventOptionId;
        public CombatSessionResult PendingCombatResult;
        public bool IsSettlementAcknowledged;
        public ExpeditionResultSummary ResultSummary;

        public ExpeditionTable.ExpeditionRouteNodeConfig GetCurrentNode()
        {
            if (CurrentNodeIndex < 0 || CurrentNodeIndex >= Route.Count)
            {
                return null;
            }

            return Route[CurrentNodeIndex];
        }

        public ExpeditionNodeRecord GetCurrentRecord()
        {
            var node = GetCurrentNode();
            if (node == null)
            {
                return null;
            }

            var record = NodeRecords.Find(item => item.NodeId == node.NodeId);
            if (record != null)
            {
                return record;
            }

            record = new ExpeditionNodeRecord
            {
                NodeId = node.NodeId,
                NodeType = node.NodeType,
                Status = EnumExpeditionNodeProcessStatus.Pending,
            };
            NodeRecords.Add(record);
            return record;
        }

        public bool AreAllPlayerMarblesDead()
        {
            return MarbleSnapshots.All(snapshot => snapshot.HasValue && (snapshot.Value.IsDead || snapshot.Value.CurrentHp <= 0));
        }
    }

    [Serializable]
    public sealed class ExpeditionNodeRecord
    {
        public string NodeId;
        public ExpeditionTable.EnumExpeditionNodeType NodeType;
        public EnumExpeditionNodeProcessStatus Status;
        public string ChosenOptionId;
        public int GainedCrystal;
        public List<string> AppliedBuffIds = new List<string>();
        public string Summary;
        public CombatSessionResult CombatResult;
    }

    [Serializable]
    public sealed class ExpeditionResultSummary
    {
        public string ExpeditionId;
        public bool IsVictory;
        public EnumExpeditionEndReason EndReason;
        public int CrystalDelta;
        public List<ExpeditionMarbleSummary> MarbleSummaries = new List<ExpeditionMarbleSummary>();
        public List<string> NodeSummaries = new List<string>();

        public string ToDisplayText()
        {
            var status = IsVictory ? "远征成功" : "远征失败";
            var marbleLines = MarbleSummaries.Count == 0
                ? "无参战 Marble 记录"
                : string.Join("\n", MarbleSummaries.Select(summary => $"- {summary.DisplayName}: HP {summary.CurrentHp}/{summary.MaxHp} EXP {summary.Exp} {(summary.IsDead ? "[阵亡]" : "[存活]")}"));
            var nodeLines = NodeSummaries.Count == 0 ? "无节点记录" : string.Join("\n", NodeSummaries.Select(summary => $"- {summary}"));
            return $"{status}\n资源变化: +{CrystalDelta} 晶体\n\n队伍状态:\n{marbleLines}\n\n节点记录:\n{nodeLines}";
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

            return new ExpeditionRunState
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExpeditionId = expedition.ExpeditionId,
                Phase = EnumExpeditionFlowPhase.None,
                EndReason = EnumExpeditionEndReason.None,
                CurrentNodeIndex = 0,
                MarbleSnapshots = marbles?
                    .Where(marble => marble.HasValue && !marble.Value.IsDead)
                    .ToList() ?? new List<MarblePersistentData?>(),
                Route = expedition.Route?.Where(node => node != null).ToList() ?? new List<ExpeditionTable.ExpeditionRouteNodeConfig>(),
            };
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

            foreach (var node in expedition.Route)
            {
                if (node == null)
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
            }

            return true;
        }
    }
}
