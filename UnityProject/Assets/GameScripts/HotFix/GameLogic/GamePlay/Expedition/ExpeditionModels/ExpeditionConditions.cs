using System.Collections.Generic;
using GameConfig.Gameplay.Expedition;

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
            ExpeditionRouteNodeConfig currentNode,
            ExpeditionNodeRecord currentRecord)
        {
            RunState = runState;
            CurrentNode = currentNode;
            CurrentRecord = currentRecord;
        }

        public ExpeditionRunState RunState { get; }

        public ExpeditionRouteNodeConfig CurrentNode { get; }

        public ExpeditionNodeRecord CurrentRecord { get; }
    }

    public static class ExpeditionConditionFactory
    {
        public static bool AreAllSatisfied(
            IEnumerable<ExpeditionConditionConfig> configs,
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

        public static IExpeditionCondition CreateCondition(ExpeditionConditionConfig config)
        {
            return config switch
            {
                HasFlagConditionConfig hasFlagConfig => new HasFlagCondition(hasFlagConfig),
                HasItemConditionConfig hasItemConfig => new HasItemCondition(hasItemConfig),
                HasChosenOptionConditionConfig chosenOptionConfig => new HasChosenOptionCondition(chosenOptionConfig),
                CounterAtLeastConditionConfig counterConfig => new CounterAtLeastCondition(counterConfig),
                _ => new AlwaysFalseCondition(),
            };
        }
    }

    public sealed class HasFlagCondition : IExpeditionCondition
    {
        private readonly HasFlagConditionConfig _config;

        public HasFlagCondition(HasFlagConditionConfig config)
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
        private readonly HasItemConditionConfig _config;

        public HasItemCondition(HasItemConditionConfig config)
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
        private readonly HasChosenOptionConditionConfig _config;

        public HasChosenOptionCondition(HasChosenOptionConditionConfig config)
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
        private readonly CounterAtLeastConditionConfig _config;

        public CounterAtLeastCondition(CounterAtLeastConditionConfig config)
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
