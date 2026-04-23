using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Gameplay.Combat.Marble;
using TEngine;
using UnityEngine;

namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionConstants
    {
        public const string MinimalExpeditionId = "MinimalExpedition";
        public const int PlayerCamp = 1;
        public const int EnemyCamp = 2;
    }

    public enum EnumExpeditionNodeType
    {
        None = 0,
        Event = 1,
        Combat = 2,
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
            var maxHp = ExpeditionStaticRouteFactory.ResolveMarbleMaxHp(configId, level);
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
        public List<ExpeditionNodeConfig> Route = new List<ExpeditionNodeConfig>();
        public List<ExpeditionNodeRecord> NodeRecords = new List<ExpeditionNodeRecord>();
        public string PendingEventOptionId;
        public CombatSessionResult PendingCombatResult;
        public bool IsSettlementAcknowledged;
        public ExpeditionResultSummary ResultSummary;

        public ExpeditionNodeConfig GetCurrentNode()
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
    public sealed class ExpeditionNodeConfig
    {
        public string NodeId;
        public EnumExpeditionNodeType NodeType;
        public ExpeditionEventNodeConfig EventConfig;
        public ExpeditionCombatNodeConfig CombatConfig;
    }

    [Serializable]
    public sealed class ExpeditionNodeRecord
    {
        public string NodeId;
        public EnumExpeditionNodeType NodeType;
        public EnumExpeditionNodeProcessStatus Status;
        public string ChosenOptionId;
        public int GainedCrystal;
        public List<string> AppliedBuffIds = new List<string>();
        public string Summary;
        public CombatSessionResult CombatResult;
    }

    [Serializable]
    public sealed class ExpeditionEventNodeConfig
    {
        public string EventId;
        public string Title;
        public string Description;
        public List<ExpeditionEventOptionConfig> Options = new List<ExpeditionEventOptionConfig>();

        public ExpeditionEventOptionConfig GetOption(string optionId)
        {
            return Options.Find(option => option.OptionId == optionId);
        }
    }

    [Serializable]
    public sealed class ExpeditionEventOptionConfig
    {
        public string OptionId;
        public string Title;
        public string Description;
        public ExpeditionEventEffect Effect = new ExpeditionEventEffect();
    }

    [Serializable]
    public sealed class ExpeditionEventEffect
    {
        public int CrystalDelta;
        public int ExpDelta;
        public int HpDelta;
        public string Summary;

        public void Apply(ExpeditionRunState runState)
        {
            if (runState == null)
            {
                return;
            }

            runState.TotalCrystalGained += CrystalDelta;
            for (int i = 0; i < runState.MarbleSnapshots.Count; i++)
            {
                if(!runState.MarbleSnapshots[i].HasValue)
                    continue;

                var snapshot = runState.MarbleSnapshots[i].Value;
                snapshot.Exp += ExpDelta;
                snapshot.CurrentHp = Mathf.Clamp(snapshot.CurrentHp + HpDelta, 0, snapshot.MaxHp);
                snapshot.IsDead = snapshot.CurrentHp <= 0;

                runState.MarbleSnapshots[i] = snapshot;
            }
        }
    }

    [Serializable]
    public sealed class ExpeditionCombatNodeConfig
    {
        public string CombatId;
        public string Title;
        public string Description;
        public int VictoryCrystalReward;
        public int VictoryExpReward;
        public List<ExpeditionEnemyMarbleConfig> EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>();
    }

    [Serializable]
    public sealed class ExpeditionEnemyMarbleConfig
    {
        public string EnemyId;
        public string ConfigId;
        public string DisplayName;
        public int Level;
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

    public static class ExpeditionStaticRouteFactory
    {
        public static ExpeditionRunState CreateMinimalRun(IEnumerable<MarblePersistentData?> marbles)
        {
            return new ExpeditionRunState
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExpeditionId = ExpeditionConstants.MinimalExpeditionId,
                Phase = EnumExpeditionFlowPhase.None,
                EndReason = EnumExpeditionEndReason.None,
                CurrentNodeIndex = 0,
                MarbleSnapshots = marbles
                    .Where(marble => marble.HasValue && !marble.Value.IsDead)
                    .ToList(),
                Route = CreateMinimalRoute(),
            };
        }

        public static List<ExpeditionNodeConfig> CreateMinimalRoute()
        {
            return new List<ExpeditionNodeConfig>
            {
                new ExpeditionNodeConfig
                {
                    NodeId = "event_supply_cache",
                    NodeType = EnumExpeditionNodeType.Event,
                    EventConfig = new ExpeditionEventNodeConfig
                    {
                        EventId = "event_supply_cache",
                        Title = "废墟补给点",
                        Description = "队伍在废墟中发现两份应急补给，你要立刻决定如何处理。",
                        Options = new List<ExpeditionEventOptionConfig>
                        {
                            new ExpeditionEventOptionConfig
                            {
                                OptionId = "option_salvage",
                                Title = "拆解补给箱",
                                Description = "快速拆解出可回收晶体，但搬运时会造成碰撞损伤。",
                                Effect = new ExpeditionEventEffect
                                {
                                    CrystalDelta = 60,
                                    HpDelta = -8,
                                    Summary = "拆解补给箱，获得 60 晶体，全队失去 8 点生命。"
                                }
                            },
                            new ExpeditionEventOptionConfig
                            {
                                OptionId = "option_briefing",
                                Title = "整理战术笔记",
                                Description = "没有额外晶体，但每名 Marble 都能获得战前经验。",
                                Effect = new ExpeditionEventEffect
                                {
                                    ExpDelta = 12,
                                    Summary = "整理战术笔记，全队获得 12 点经验。"
                                }
                            }
                        }
                    }
                },
                new ExpeditionNodeConfig
                {
                    NodeId = "combat_outpost",
                    NodeType = EnumExpeditionNodeType.Combat,
                    CombatConfig = new ExpeditionCombatNodeConfig
                    {
                        CombatId = "combat_outpost",
                        Title = "前哨遭遇战",
                        Description = "敌方巡逻 Marble 正在封锁出口，击败它们即可带走补给。",
                        VictoryCrystalReward = 120,
                        VictoryExpReward = 15,
                        EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>
                        {
                            new ExpeditionEnemyMarbleConfig
                            {
                                EnemyId = "enemy_1",
                                ConfigId = "archer",
                                DisplayName = "巡逻 Marble A",
                                Level = 0,
                            },
                            new ExpeditionEnemyMarbleConfig
                            {
                                EnemyId = "enemy_2",
                                ConfigId = "soldier",
                                DisplayName = "巡逻 Marble B",
                                Level = 0,
                            }
                        }
                    }
                }
            };
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
    }
}
