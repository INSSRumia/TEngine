using System.Collections.Generic;
using System.Linq;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public interface IExpeditionEffect
    {
        void Execute(ExpeditionEffectExecutionContext context);
    }

    public sealed class ExpeditionEffectExecutionContext
    {
        public ExpeditionEffectExecutionContext(ExpeditionRunState runState, ExpeditionPersistentDataStore persistentData, ExpeditionNodeRecord nodeRecord)
        {
            RunState = runState;
            PersistentData = persistentData;
            NodeRecord = nodeRecord;
        }

        public ExpeditionRunState RunState { get; }

        public ExpeditionPersistentDataStore PersistentData { get; }

        public ExpeditionNodeRecord NodeRecord { get; }

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
            {
                SummaryLines.Add(normalized);
            }
        }

        private static string NormalizeSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            var normalized = summary.Trim();

            return normalized;
        }
    }

    public static class ExpeditionEffectFactory
    {
        public static void ExecuteEffects(IEnumerable<ExpeditionTable.ExpeditionEffectConfig> configs, ExpeditionEffectExecutionContext context)
        {
            if (configs == null || context == null)
            {
                return;
            }

            foreach (var config in configs)
            {
                CreateEffect(config)?.Execute(context);
            }
        }

        public static IExpeditionEffect CreateEffect(ExpeditionTable.ExpeditionEffectConfig config)
        {
            return config switch
            {
                ExpeditionTable.AddMoneyEffectConfig moneyConfig => new AddMoneyEffect(moneyConfig),
                ExpeditionTable.AddPlayerMarbleExpEffectConfig expConfig => new AddPlayerMarbleExpEffect(expConfig),
                ExpeditionTable.AddPlayerMarbleHpEffectConfig hpConfig => new AddPlayerMarbleHpEffect(hpConfig),
                _ => null,
            };
        }
    }

    public sealed class AddMoneyEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddMoneyEffectConfig _config;

        public AddMoneyEffect(ExpeditionTable.AddMoneyEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            context.RunState.TotalMoneyGained += _config.MoneyDelta;
            context.AddMoneyDelta(_config.MoneyDelta);
            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? $"获得 {_config.MoneyDelta} 晶体。"
                : _config.Summary);
        }
    }

    public sealed class AddPlayerMarbleExpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleExpEffectConfig _config;

        public AddPlayerMarbleExpEffect(ExpeditionTable.AddPlayerMarbleExpEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context.RunState.MarbleSnapshots == null)
            {
                return;
            }

            for (int i = 0; i < context.RunState.MarbleSnapshots.Count; i++)
            {
                if (!context.RunState.MarbleSnapshots[i].HasValue)
                {
                    continue;
                }

                var snapshot = context.RunState.MarbleSnapshots[i].Value;
                snapshot.Exp += _config.ExpDelta;
                context.RunState.MarbleSnapshots[i] = snapshot;
            }

            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? $"全队获得 {_config.ExpDelta} 点经验。"
                : _config.Summary);
        }
    }

    public sealed class AddPlayerMarbleHpEffect : IExpeditionEffect
    {
        private readonly ExpeditionTable.AddPlayerMarbleHpEffectConfig _config;

        public AddPlayerMarbleHpEffect(ExpeditionTable.AddPlayerMarbleHpEffectConfig config)
        {
            _config = config;
        }

        public void Execute(ExpeditionEffectExecutionContext context)
        {
            if (context.RunState.MarbleSnapshots == null)
            {
                return;
            }

            for (int i = 0; i < context.RunState.MarbleSnapshots.Count; i++)
            {
                if (!context.RunState.MarbleSnapshots[i].HasValue)
                {
                    continue;
                }

                var snapshot = context.RunState.MarbleSnapshots[i].Value;
                snapshot.CurrentHp = UnityEngine.Mathf.Clamp(snapshot.CurrentHp + _config.HpDelta, 0, snapshot.MaxHp);
                snapshot.IsDead = snapshot.CurrentHp <= 0;
                context.RunState.MarbleSnapshots[i] = snapshot;
            }

            context.AddSummary(string.IsNullOrWhiteSpace(_config.Summary)
                ? (_config.HpDelta >= 0
                    ? $"全队恢复 {_config.HpDelta} 点生命。"
                    : $"全队失去 {System.Math.Abs(_config.HpDelta)} 点生命。")
                : _config.Summary);
        }
    }
}
