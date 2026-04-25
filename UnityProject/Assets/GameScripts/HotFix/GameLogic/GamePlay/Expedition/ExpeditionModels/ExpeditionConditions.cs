using System.Collections.Generic;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public interface IExpeditionCondition
    {
        bool Evaluate(ExpeditionConditionExecutionContext context);
    }

    public sealed class ExpeditionConditionExecutionContext
    {
        public ExpeditionConditionExecutionContext(
            ExpeditionRunState runState,
            ExpeditionTable.ExpeditionRouteNodeConfig currentNode,
            ExpeditionNodeRecord currentRecord)
        {
            RunState = runState;
            CurrentNode = currentNode;
            CurrentRecord = currentRecord;
        }

        public ExpeditionRunState RunState { get; }

        public ExpeditionTable.ExpeditionRouteNodeConfig CurrentNode { get; }

        public ExpeditionNodeRecord CurrentRecord { get; }
    }

    public static class ExpeditionConditionFactory
    {
        public static bool AreAllSatisfied(
            IEnumerable<ExpeditionTable.ExpeditionConditionConfig> configs,
            ExpeditionConditionExecutionContext context)
        {
            if (configs == null)
            {
                return true;
            }

            foreach (var config in configs)
            {
                if (!CreateCondition(config).Evaluate(context))
                {
                    return false;
                }
            }

            return true;
        }

        public static IExpeditionCondition CreateCondition(ExpeditionTable.ExpeditionConditionConfig config)
        {
            return config switch
            {
                ExpeditionTable.HasFlagConditionConfig hasFlagConfig => new HasFlagCondition(hasFlagConfig),
                ExpeditionTable.HasItemConditionConfig hasItemConfig => new HasItemCondition(hasItemConfig),
                ExpeditionTable.HasChosenOptionConditionConfig chosenOptionConfig => new HasChosenOptionCondition(chosenOptionConfig),
                ExpeditionTable.CounterAtLeastConditionConfig counterConfig => new CounterAtLeastCondition(counterConfig),
                _ => new AlwaysFalseCondition(),
            };
        }
    }

    public sealed class HasFlagCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.HasFlagConditionConfig _config;

        public HasFlagCondition(ExpeditionTable.HasFlagConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return context?.RunState?.Blackboard?.HasFlag(_config.FlagId) ?? false;
        }
    }

    public sealed class HasItemCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.HasItemConditionConfig _config;

        public HasItemCondition(ExpeditionTable.HasItemConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return context?.RunState?.Blackboard?.HasItem(_config.ItemId) ?? false;
        }
    }

    public sealed class HasChosenOptionCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.HasChosenOptionConditionConfig _config;

        public HasChosenOptionCondition(ExpeditionTable.HasChosenOptionConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return context?.RunState?.Blackboard?.HasChosenOption(_config.OptionId) ?? false;
        }
    }

    public sealed class CounterAtLeastCondition : IExpeditionCondition
    {
        private readonly ExpeditionTable.CounterAtLeastConditionConfig _config;

        public CounterAtLeastCondition(ExpeditionTable.CounterAtLeastConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return (context?.RunState?.Blackboard?.GetCounterValue(_config.CounterId) ?? 0) >= _config.MinValue;
        }
    }

    public sealed class AlwaysFalseCondition : IExpeditionCondition
    {
        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            return false;
        }
    }
}
