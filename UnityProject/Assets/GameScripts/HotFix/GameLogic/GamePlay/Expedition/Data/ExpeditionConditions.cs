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

        public static bool IsAnySatisfied(
            IEnumerable<ExpeditionConditionConfig> configs,
            ExpeditionConditionExecutionContext context)
        {
            if (configs == null)
                return false;

            foreach (var config in configs)
            {
                if (EvaluateCondition(config, context))
                    return true;
            }

            return false;
        }

        public static bool EvaluateCondition(
            ExpeditionConditionConfig config,
            ExpeditionConditionExecutionContext context)
        {
            if (config == null)
                return false;

            return CreateCondition(config).Evaluate(context);
        }

        public static IExpeditionCondition CreateCondition(ExpeditionConditionConfig config)
        {
            return config switch
            {
                HasFlagConditionConfig hasFlagConfig => new HasFlagCondition(hasFlagConfig),
                HasItemConditionConfig hasItemConfig => new HasItemCondition(hasItemConfig),
                HasChosenOptionConditionConfig chosenOptionConfig => new HasChosenOptionCondition(chosenOptionConfig),
                CounterAtLeastConditionConfig counterConfig => new CounterAtLeastCondition(counterConfig),
                AndConditionConfig andConfig => new AndCondition(andConfig),
                OrConditionConfig orConfig => new OrCondition(orConfig),
                NotConditionConfig notConfig => new NotCondition(notConfig),
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

    public sealed class AndCondition : IExpeditionCondition
    {
        private readonly AndConditionConfig _config;

        public AndCondition(AndConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            if (_config?.Conditions == null || _config.Conditions.Count == 0)
                return false;

            return ExpeditionConditionFactory.AreAllSatisfied(_config.Conditions, context);
        }
    }

    public sealed class OrCondition : IExpeditionCondition
    {
        private readonly OrConditionConfig _config;

        public OrCondition(OrConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            if (_config?.Conditions == null || _config.Conditions.Count == 0)
                return false;

            return ExpeditionConditionFactory.IsAnySatisfied(_config.Conditions, context);
        }
    }

    public sealed class NotCondition : IExpeditionCondition
    {
        private readonly NotConditionConfig _config;

        public NotCondition(NotConditionConfig config)
        {
            _config = config;
        }

        public bool Evaluate(ExpeditionConditionExecutionContext context)
        {
            if (_config?.Condition == null)
                return false;

            return !ExpeditionConditionFactory.EvaluateCondition(_config.Condition, context);
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
