using System;
using System.Collections.Generic;
using System.Linq;
using TEngine;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed partial class ExpeditionRunState
    {
        // 远征随机事件抽取统一使用的运行时随机源。
        private static readonly Random _random = new Random();
        // 标记“远征自身配置”激活的随机池来源。
        private const string BaseRandomEventPoolSourceType = "expedition";
        // 标记“环境切换”激活的随机池来源。
        private const string EnvironmentRandomEventPoolSourceType = "environment";

        // 本次远征运行实例 Id。
        public string ExpeditionInstId;
        // 本次远征对应的 expedition_config_id。
        public string ExpeditionConfigId;
        // 当前远征所在环境，用来解析环境事件池和默认战场。
        public string CurrentEnvironmentConfigId;
        // 当前流程阶段，驱动 FSM 与 UI 展示。
        public EnumExpeditionFlowPhase Phase;
        // 当前远征为什么结束。None 表示仍在进行中。
        public EnumExpeditionEndReason EndReason;
        // 本次远征累计获得的 money。
        public int TotalMoneyGained;
        // 远征黑板：选项、flag、item、counter 等都记录在这里。
        public ExpeditionBlackboard Blackboard = new ExpeditionBlackboard();
        // 当前远征队伍的运行时快照。战斗、事件效果都直接改这份数据。
        public List<MarblePersistentData?> LstMarbleSnapshot = new ();
        // 当前远征的静态路线配置原文。
        public List<ExpeditionTable.ExpeditionRouteNodeConfig> LstRouteConfig = new ();
        // 待执行节点队列。主线推进和动态插队最终都会落到这里。
        public List<ExpeditionPendingNodeEntry> LstPendingNodeQueue = new ();
        // 当前正在执行的节点条目。
        public ExpeditionPendingNodeEntry CurrentNodeEntry;
        // 已经进入过的节点记录，用于结算摘要和调试。
        public List<ExpeditionNodeRecord> LstNodeRecord = new ();
        // “若干节点后插入临时节点”的延迟插入请求列表。
        public List<ExpeditionPendingInsertNodeEntry> LstPendingInsertNode = new ();
        // 当前激活中的随机事件池运行时状态。
        public List<ExpeditionActiveRandomEventPoolState> LstActiveRandomEventPool = new ();
        // 本次远征会话级调试追踪；只记录开发期 trace，不承担节点业务记录职责。
        public ExpeditionDebugTrace DebugTrace = new ();
        // 当前事件节点等待玩家提交的 option_id。
        public string PendingEventOptionId;
        // 当前战斗节点等待回写的战斗结果。
        public CombatSessionResult PendingCombatResult;
        // 结算界面是否已经被玩家确认关闭。
        public bool IsSettlementAcknowledged;
        // 本次远征最终生成的结算摘要。
        public ExpeditionResultSummary ResultSummary;
        // 已进入节点的总次数。用于阶段计算、日志排序和结算展示。
        public int EnteredNodeCount;
        // 延迟插入请求的自增登记序号。
        public int PendingInsertOrderSeed;

        // 初始化远征可用的随机池：先挂基础池，再根据初始环境挂环境池。
        public void InitializeRandomEventPools(ExpeditionTable.ExpeditionConfig expedition)
        {
            LstActiveRandomEventPool.Clear();
            ActivateRandomEventPools(
                expedition?.LstRandomEventPoolConfigId,
                BaseRandomEventPoolSourceType,
                expedition?.ExpeditionConfigId,
                true);
            ChangeEnvironment(expedition?.InitialEnvironmentConfigId);
        }

        // 切换当前环境，并按规则移除旧环境池、激活新环境池。
        public bool ChangeEnvironment(string environmentConfigId)
        {
            RemoveRandomEventPoolsBySource(EnvironmentRandomEventPoolSourceType);
            CurrentEnvironmentConfigId = environmentConfigId ?? string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentEnvironmentConfigId))
            {
                DebugTrace.RecordEnvironment("当前环境为空，仅保留远征基础随机事件池。", Phase);
                return true;
            }

            var environment = ExpeditionConfigBridge.ResolveEnvironment(CurrentEnvironmentConfigId);
            if (environment == null)
            {
                DebugTrace.RecordEnvironment(
                    $"未找到环境配置 environmentConfigId={CurrentEnvironmentConfigId}",
                    Phase,
                    severity: EnumExpeditionDebugTraceSeverity.Warning);
                Log.Warning($"[远征] 未找到环境配置。environmentConfigId:{CurrentEnvironmentConfigId}");
                return false;
            }

            ActivateRandomEventPools(
                environment.LstRandomEventPoolConfigId,
                EnvironmentRandomEventPoolSourceType,
                CurrentEnvironmentConfigId,
                true);
            DebugTrace.RecordEnvironment($"当前环境切换为 {CurrentEnvironmentConfigId}", Phase);
            return true;
        }

        // 从所有当前激活的随机池里抽一个事件。池内抽取是无放回的。
        public ExpeditionRandomEventDrawResult DrawRandomEvent()
        {
            var lstValidPool = LstActiveRandomEventPool?
                .Where(pool => pool != null && pool.GetTotalWeight() > 0)
                .ToList() ?? new List<ExpeditionActiveRandomEventPoolState>();
            var totalWeight = lstValidPool.Sum(pool => pool.GetTotalWeight());
            if (totalWeight <= 0)
            {
                return new ExpeditionRandomEventDrawResult
                {
                    IsSuccess = false,
                    Summary = "当前没有可抽取的随机事件，跳过 RandomEvent 节点。",
                };
            }

            var globalWeight = _random.Next(totalWeight);
            var cursor = 0;
            foreach (var pool in lstValidPool)
            {
                var poolWeight = pool.GetTotalWeight();
                if (globalWeight >= cursor + poolWeight)
                {
                    cursor += poolWeight;
                    continue;
                }

                return DrawRandomEventFromPool(pool, globalWeight - cursor);
            }

            return new ExpeditionRandomEventDrawResult
            {
                IsSuccess = false,
                Summary = "随机事件权重定位失败，跳过 RandomEvent 节点。",
            };
        }

        // 激活一组随机池；同来源重复激活时可选择保留旧状态，避免把已抽取进度重置掉。
        private void ActivateRandomEventPools(IEnumerable<string> lstPoolConfigId, string sourceType, string sourceConfigId, bool preserveExistingState)
        {
            if (lstPoolConfigId == null)
                return;

            foreach (var poolConfigId in lstPoolConfigId)
            {
                if (string.IsNullOrWhiteSpace(poolConfigId))
                    continue;

                if (preserveExistingState && HasActiveRandomEventPool(poolConfigId, sourceType, sourceConfigId))
                    continue;

                var poolConfig = ExpeditionConfigBridge.ResolveRandomEventPool(poolConfigId);
                if (poolConfig == null)
                {
                    DebugTrace.RecordRandomEventPool(
                        $"未找到池配置 poolConfigId={poolConfigId}",
                        Phase,
                        severity: EnumExpeditionDebugTraceSeverity.Warning);
                    Log.Warning($"[远征] 未找到随机事件池配置。poolConfigId:{poolConfigId}");
                    continue;
                }

                var poolState = new ExpeditionActiveRandomEventPoolState
                {
                    PoolRuntimeInstId = Guid.NewGuid().ToString("N"),
                    RandomEventPoolConfigId = poolConfig.RandomEventPoolConfigId,
                    SourceType = sourceType ?? string.Empty,
                    SourceConfigId = sourceConfigId ?? string.Empty,
                    LstRemainingEntry = poolConfig.LstEvent?
                        .Where(entry => entry != null)
                        .Select(entry => new ExpeditionRandomEventPoolEntryState(entry))
                        .ToList() ?? new List<ExpeditionRandomEventPoolEntryState>(),
                };
                LstActiveRandomEventPool.Add(poolState);
                DebugTrace.RecordRandomEventPool(
                    $"激活 {poolState.RandomEventPoolConfigId} source={poolState.SourceType}:{poolState.SourceConfigId} entries={poolState.LstRemainingEntry.Count}",
                    Phase);
            }
        }

        // 移除某个来源类型激活出来的所有随机池。环境切换时主要用这个。
        private void RemoveRandomEventPoolsBySource(string sourceType)
        {
            if (LstActiveRandomEventPool == null || LstActiveRandomEventPool.Count == 0)
                return;

            var removedCount = LstActiveRandomEventPool.RemoveAll(pool => pool != null && pool.SourceType == sourceType);
            if (removedCount > 0)
                DebugTrace.RecordRandomEventPool($"移除来源 {sourceType} 的池数量:{removedCount}", Phase);
        }

        // 检查某个来源的某个池是否已经处于激活状态。
        private bool HasActiveRandomEventPool(string poolConfigId, string sourceType, string sourceConfigId)
        {
            return LstActiveRandomEventPool.Any(pool =>
                pool != null
                && pool.RandomEventPoolConfigId == poolConfigId
                && pool.SourceType == (sourceType ?? string.Empty)
                && pool.SourceConfigId == (sourceConfigId ?? string.Empty));
        }

        // 在单个随机池内部按权重抽一个条目，并把命中的条目从剩余列表中移除。
        private static ExpeditionRandomEventDrawResult DrawRandomEventFromPool(ExpeditionActiveRandomEventPoolState pool, int localWeight)
        {
            var cursor = 0;
            for (int i = 0; i < pool.LstRemainingEntry.Count; i++)
            {
                var entry = pool.LstRemainingEntry[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.EventConfigId) || entry.Weight <= 0)
                    continue;

                if (localWeight >= cursor + entry.Weight)
                {
                    cursor += entry.Weight;
                    continue;
                }

                pool.LstRemainingEntry.RemoveAt(i);
                return new ExpeditionRandomEventDrawResult
                {
                    IsSuccess = true,
                    EventConfigId = entry.EventConfigId,
                    RandomEventPoolConfigId = pool.RandomEventPoolConfigId,
                    Summary = $"从随机事件池 {pool.RandomEventPoolConfigId} 抽取事件 {entry.EventConfigId}。",
                };
            }

            return new ExpeditionRandomEventDrawResult
            {
                IsSuccess = false,
                Summary = $"随机事件池 {pool.RandomEventPoolConfigId} 没有命中有效条目。",
            };
        }
    }
}
