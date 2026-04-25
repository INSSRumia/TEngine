using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionBlackboardCounter
    {
        public string CounterId;
        public int Value;
    }

    [Serializable]
    public sealed class ExpeditionBlackboard
    {
        public List<string> Flags = new List<string>();
        public List<string> InventoryItemIds = new List<string>();
        public List<string> ChosenOptionIds = new List<string>();
        public List<string> CompletedEventIds = new List<string>();
        public List<ExpeditionBlackboardCounter> Counters = new List<ExpeditionBlackboardCounter>();

        public bool HasFlag(string flagId)
        {
            return !string.IsNullOrWhiteSpace(flagId) && Flags.Contains(flagId);
        }

        public void AddFlag(string flagId)
        {
            AddUnique(Flags, flagId);
        }

        public bool HasItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId) && InventoryItemIds.Contains(itemId);
        }

        public void AddItem(string itemId)
        {
            AddUnique(InventoryItemIds, itemId);
        }

        public bool HasChosenOption(string optionId)
        {
            return !string.IsNullOrWhiteSpace(optionId) && ChosenOptionIds.Contains(optionId);
        }

        public void AddChosenOption(string optionId)
        {
            AddUnique(ChosenOptionIds, optionId);
        }

        public bool HasCompletedEvent(string eventId)
        {
            return !string.IsNullOrWhiteSpace(eventId) && CompletedEventIds.Contains(eventId);
        }

        public void AddCompletedEvent(string eventId)
        {
            AddUnique(CompletedEventIds, eventId);
        }

        public int GetCounterValue(string counterId)
        {
            if (string.IsNullOrWhiteSpace(counterId))
            {
                return 0;
            }

            var counter = Counters.Find(item => item != null && item.CounterId == counterId);
            return counter?.Value ?? 0;
        }

        public void SetCounterValue(string counterId, int value)
        {
            if (string.IsNullOrWhiteSpace(counterId))
            {
                return;
            }

            var counter = Counters.Find(item => item != null && item.CounterId == counterId);
            if (counter == null)
            {
                Counters.Add(new ExpeditionBlackboardCounter
                {
                    CounterId = counterId,
                    Value = value,
                });
                return;
            }

            counter.Value = value;
        }

        public void AddCounterValue(string counterId, int delta)
        {
            SetCounterValue(counterId, GetCounterValue(counterId) + delta);
        }

        public string ToDebugString()
        {
            var builder = new StringBuilder();
            builder.Append("flags=[");
            builder.Append(string.Join(",", Flags));
            builder.Append("]\nitems=[");
            builder.Append(string.Join(",", InventoryItemIds));
            builder.Append("]\nchosen=[");
            builder.Append(string.Join(",", ChosenOptionIds));
            builder.Append("]\ncompleted=[");
            builder.Append(string.Join(",", CompletedEventIds));
            builder.Append("]\ncounters=[");
            builder.Append(string.Join(",", Counters.Where(counter => counter != null).Select(counter => $"{counter.CounterId}:{counter.Value}")));
            builder.Append("]\n");
            return builder.ToString();
        }

        private static void AddUnique(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value) || values.Contains(value))
            {
                return;
            }

            values.Add(value);
        }
    }
}
