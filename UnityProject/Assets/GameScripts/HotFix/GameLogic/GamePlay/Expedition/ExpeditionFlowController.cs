using System.Collections.Generic;
using System.Linq;
using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionFlowController : Singleton<ExpeditionFlowController>
    {
        private const string FsmName = "MinimalExpeditionFlow";

        private readonly ExpeditionPersistentDataStore _persistentData = new ExpeditionPersistentDataStore();

        public ExpeditionPersistentDataStore PersistentData => _persistentData;
        public ExpeditionRunState CurrentRun { get; private set; }
        public IFsm<ExpeditionFlowController> Fsm { get; private set; }

        public bool IsFlowRunning => CurrentRun != null && Fsm != null;

        protected override void OnInit()
        {
            _persistentData.EnsureInitialized();
            _ = ExpeditionCombatSessionController.Instance;
        }

        public void OpenEntryUi()
        {
            _persistentData.EnsureInitialized();
            GameModule.UI.ShowUIAsync<global::GameLogic.ExpeditionMainUI>();
        }

        public bool StartMinimalExpedition()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
            {
                return false;
            }

            _persistentData.EnsureInitialized();
            GameModule.UI.CloseUI<global::GameLogic.ExpeditionMainUI>();
            DestroyFsmIfNeeded();

            CurrentRun = ExpeditionStaticRouteFactory.CreateMinimalRun(_persistentData.Marbles);
            Fsm = GameModule.Fsm.CreateFsm(FsmName, this,
                new ExpeditionFlowStatePrepare(),
                new ExpeditionFlowStateEnterNode(),
                new ExpeditionFlowStateEvent(),
                new ExpeditionFlowStateCombat(),
                new ExpeditionFlowStateApplyNodeResult(),
                new ExpeditionFlowStateSettlement(),
                new ExpeditionFlowStateFinished());
            Fsm.Start<ExpeditionFlowStatePrepare>();
            return true;
        }

        public bool StartCombatDebug()
        {
            if (IsFlowRunning || ExpeditionCombatSessionController.Instance.IsRunning)
            {
                return false;
            }

            var request = new CombatSessionRequest
            {
                SessionId = "combat_debug_session",
                NodeId = "combat_debug_node",
                CombatId = "combat_debug",
                Title = "Combat 调试后门",
                VictoryCrystalReward = 0,
                VictoryExpReward = 0,
                AlliedMarbles = _persistentData.Marbles.Select(item => item.CreateSnapshot()).ToList(),
                EnemyMarbles = new List<ExpeditionEnemyMarbleConfig>
                {
                    new ExpeditionEnemyMarbleConfig
                    {
                        EnemyId = "debug_enemy_1",
                        ConfigId = "Marble_001",
                        DisplayName = "调试敌方一号",
                        Level = 0,
                    },
                    new ExpeditionEnemyMarbleConfig
                    {
                        EnemyId = "debug_enemy_2",
                        ConfigId = "Marble_001",
                        DisplayName = "调试敌方二号",
                        Level = 0,
                    }
                }
            };

            GameModule.UI.CloseUI<global::GameLogic.ExpeditionMainUI>();
            GameModule.UI.ShowUIAsync<BattleMainUI>();
            return ExpeditionCombatSessionController.Instance.StartSession(request, OnDebugCombatCompleted);
        }

        public ExpeditionEventNodeConfig GetCurrentEventNode()
        {
            return CurrentRun?.GetCurrentNode()?.EventConfig;
        }

        public ExpeditionResultSummary GetDisplayableResult()
        {
            return CurrentRun?.ResultSummary ?? _persistentData.LastResult;
        }

        public void SubmitEventChoice(string optionId)
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.WaitingEventChoice)
            {
                return;
            }

            var eventNode = GetCurrentEventNode();
            if (eventNode?.GetOption(optionId) == null)
            {
                return;
            }

            CurrentRun.PendingEventOptionId = optionId;
        }

        public void SubmitCombatResult(CombatSessionResult result)
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.InCombat || result == null)
            {
                return;
            }

            CurrentRun.PendingCombatResult = result;
        }

        public void AcknowledgeSettlement()
        {
            if (CurrentRun == null || CurrentRun.Phase != EnumExpeditionFlowPhase.Settling)
            {
                return;
            }

            CurrentRun.IsSettlementAcknowledged = true;
        }

        internal void SetPhase(EnumExpeditionFlowPhase phase)
        {
            if (CurrentRun != null)
            {
                CurrentRun.Phase = phase;
            }
        }

        internal bool HasPendingEventChoice()
        {
            return CurrentRun != null && !string.IsNullOrEmpty(CurrentRun.PendingEventOptionId);
        }

        internal bool HasPendingCombatResult()
        {
            return CurrentRun?.PendingCombatResult != null;
        }

        internal ExpeditionNodeConfig GetCurrentNode()
        {
            return CurrentRun?.GetCurrentNode();
        }

        internal ExpeditionNodeRecord EnterCurrentNode()
        {
            var record = CurrentRun?.GetCurrentRecord();
            if (record != null)
            {
                record.Status = EnumExpeditionNodeProcessStatus.Entered;
            }

            return record;
        }

        internal CombatSessionRequest BuildCombatSessionRequest()
        {
            var node = GetCurrentNode();
            var combatConfig = node?.CombatConfig;
            if (combatConfig == null || CurrentRun == null)
            {
                return null;
            }

            return new CombatSessionRequest
            {
                SessionId = $"{CurrentRun.RunId}_{combatConfig.CombatId}",
                NodeId = node.NodeId,
                CombatId = combatConfig.CombatId,
                Title = combatConfig.Title,
                VictoryCrystalReward = combatConfig.VictoryCrystalReward,
                VictoryExpReward = combatConfig.VictoryExpReward,
                AlliedMarbles = CurrentRun.MarbleSnapshots.Select(snapshot => new MarblePersistentDataSnapshot
                {
                    PersistentId = snapshot.PersistentId,
                    ConfigId = snapshot.ConfigId,
                    DisplayName = snapshot.DisplayName,
                    Level = snapshot.Level,
                    CurrentHp = snapshot.CurrentHp,
                    MaxHp = snapshot.MaxHp,
                    Exp = snapshot.Exp,
                    IsDead = snapshot.IsDead,
                }).ToList(),
                EnemyMarbles = combatConfig.EnemyMarbles.Select(enemy => new ExpeditionEnemyMarbleConfig
                {
                    EnemyId = enemy.EnemyId,
                    ConfigId = enemy.ConfigId,
                    DisplayName = enemy.DisplayName,
                    Level = enemy.Level,
                }).ToList(),
            };
        }

        internal bool StartCurrentCombatSession()
        {
            var request = BuildCombatSessionRequest();
            if (request == null)
            {
                return false;
            }

            GameModule.UI.ShowUIAsync<BattleMainUI>();
            return ExpeditionCombatSessionController.Instance.StartSession(request, SubmitCombatResult);
        }

        internal void ApplyCurrentNodeResult()
        {
            var node = GetCurrentNode();
            var record = CurrentRun?.GetCurrentRecord();
            if (node == null || record == null)
            {
                return;
            }

            switch (node.NodeType)
            {
                case EnumExpeditionNodeType.Event:
                    ApplyEventNodeResult(node, record);
                    break;
                case EnumExpeditionNodeType.Combat:
                    ApplyCombatNodeResult(record);
                    break;
            }

            record.Status = EnumExpeditionNodeProcessStatus.Resolved;
            CurrentRun.PendingEventOptionId = null;
            CurrentRun.PendingCombatResult = null;

            if (CurrentRun.AreAllPlayerMarblesDead())
            {
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
            }

            if (CurrentRun.EndReason == EnumExpeditionEndReason.None)
            {
                CurrentRun.CurrentNodeIndex++;
                if (CurrentRun.CurrentNodeIndex >= CurrentRun.Route.Count)
                {
                    CurrentRun.EndReason = EnumExpeditionEndReason.Victory;
                }
            }
        }

        internal bool ShouldEnterSettlement()
        {
            return CurrentRun == null
                   || CurrentRun.EndReason != EnumExpeditionEndReason.None
                   || CurrentRun.CurrentNodeIndex >= CurrentRun.Route.Count;
        }

        internal void SettleCurrentRun()
        {
            if (CurrentRun == null)
            {
                return;
            }

            foreach (var snapshot in CurrentRun.MarbleSnapshots)
            {
                var persistentData = _persistentData.GetMarble(snapshot.PersistentId);
                snapshot.ApplyPersistentWriteback(persistentData);
            }

            _persistentData.Crystal += CurrentRun.TotalCrystalGained;
            CurrentRun.ResultSummary = BuildResultSummary(CurrentRun);
            _persistentData.LastResult = CurrentRun.ResultSummary;
        }

        internal void ReturnToEntry()
        {
            GameModule.UI.CloseUI<global::GameLogic.EventCardUI>();
            GameModule.UI.CloseUI<global::GameLogic.ExpeditionResultUI>();
            GameModule.UI.CloseUI<BattleMainUI>();
            CurrentRun = null;
            OpenEntryUi();
        }

        private void ApplyEventNodeResult(ExpeditionNodeConfig node, ExpeditionNodeRecord record)
        {
            var option = node.EventConfig?.GetOption(CurrentRun.PendingEventOptionId);
            if (option == null)
            {
                return;
            }

            option.Effect.Apply(CurrentRun);
            record.ChosenOptionId = option.OptionId;
            record.GainedCrystal = option.Effect.CrystalDelta;
            record.Summary = option.Effect.Summary;
        }

        private void ApplyCombatNodeResult(ExpeditionNodeRecord record)
        {
            var result = CurrentRun.PendingCombatResult;
            if (result == null)
            {
                return;
            }

            record.CombatResult = result;
            record.GainedCrystal = result.CrystalReward;
            record.Summary = result.Summary;
            CurrentRun.TotalCrystalGained += result.CrystalReward;

            foreach (var marbleResult in result.MarbleResults)
            {
                var snapshot = CurrentRun.MarbleSnapshots.Find(item => item.PersistentId == marbleResult.PersistentId);
                if (snapshot == null)
                {
                    continue;
                }

                snapshot.CurrentHp = marbleResult.RemainingHp;
                snapshot.MaxHp = marbleResult.MaxHp;
                snapshot.Exp += marbleResult.ExpDelta;
                snapshot.IsDead = marbleResult.IsDead;
            }

            if (!result.IsVictory)
            {
                CurrentRun.EndReason = EnumExpeditionEndReason.Defeat;
            }
        }

        private ExpeditionResultSummary BuildResultSummary(ExpeditionRunState runState)
        {
            return new ExpeditionResultSummary
            {
                ExpeditionId = runState.ExpeditionId,
                IsVictory = runState.EndReason == EnumExpeditionEndReason.Victory,
                EndReason = runState.EndReason,
                CrystalDelta = runState.TotalCrystalGained,
                MarbleSummaries = runState.MarbleSnapshots.Select(snapshot => new ExpeditionMarbleSummary
                {
                    PersistentId = snapshot.PersistentId,
                    DisplayName = snapshot.DisplayName,
                    CurrentHp = snapshot.CurrentHp,
                    MaxHp = snapshot.MaxHp,
                    Exp = snapshot.Exp,
                    IsDead = snapshot.IsDead,
                }).ToList(),
                NodeSummaries = runState.NodeRecords
                    .Where(record => !string.IsNullOrEmpty(record.Summary))
                    .Select(record => record.Summary)
                    .ToList(),
            };
        }

        private void OnDebugCombatCompleted(CombatSessionResult result)
        {
            Log.Info($"[ExpeditionFlowController] Combat debug complete. Victory:{result?.IsVictory}");
            GameModule.UI.CloseUI<BattleMainUI>();
            OpenEntryUi();
        }

        private void DestroyFsmIfNeeded()
        {
            if (GameModule.Fsm.HasFsm<ExpeditionFlowController>(FsmName))
            {
                GameModule.Fsm.DestroyFsm<ExpeditionFlowController>(FsmName);
            }

            Fsm = null;
        }
    }
}
