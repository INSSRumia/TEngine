using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionBlackboard
    {
        private readonly List<string> _lstFlag = new List<string>();
        private readonly List<string> _lstInventoryItemId = new List<string>();
        private readonly List<string> _lstChosenOptionId = new List<string>();
        private readonly List<string> _lstCompletedEventId = new List<string>();
        private readonly Dictionary<string, int> _dictCounter = new Dictionary<string, int>();

        public bool HasFlag(string flagId)
        {
            return !string.IsNullOrWhiteSpace(flagId) && _lstFlag.Contains(flagId);
        }

        public void AddFlag(string flagId)
        {
            AddUnique(_lstFlag, flagId);
        }

        public bool HasItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId) && _lstInventoryItemId.Contains(itemId);
        }

        public void AddItem(string itemId)
        {
            AddUnique(_lstInventoryItemId, itemId);
        }

        public bool HasChosenOption(string optionId)
        {
            return !string.IsNullOrWhiteSpace(optionId) && _lstChosenOptionId.Contains(optionId);
        }

        public void AddChosenOption(string optionId)
        {
            AddUnique(_lstChosenOptionId, optionId);
        }

        public bool HasCompletedEvent(string eventId)
        {
            return !string.IsNullOrWhiteSpace(eventId) && _lstCompletedEventId.Contains(eventId);
        }

        public void AddCompletedEvent(string eventId)
        {
            AddUnique(_lstCompletedEventId, eventId);
        }

        public int GetCounterValue(string counterId)
        {
            if (string.IsNullOrWhiteSpace(counterId))
                return 0;

            if(_dictCounter.TryGetValue(counterId, out var value))
                return value;

            return 0;
        }

        public void SetCounterValue(string counterId, int value)
        {
            if (string.IsNullOrWhiteSpace(counterId))
                return;

            if(_dictCounter.ContainsKey(counterId))
                _dictCounter[counterId] = value;
            else
                _dictCounter.Add(counterId, value);
        }

        public void AddCounterValue(string counterId, int delta)
        {
            SetCounterValue(counterId, GetCounterValue(counterId) + delta);
        }

        public string ToDebugString()
        {
            var builder = new StringBuilder();
            builder.Append("flags=[");
            builder.Append(string.Join(",", _lstFlag));
            builder.Append("]\nitems=[");
            builder.Append(string.Join(",", _lstInventoryItemId));
            builder.Append("]\nchosen=[");
            builder.Append(string.Join(",", _lstChosenOptionId));
            builder.Append("]\ncompleted=[");
            builder.Append(string.Join(",", _lstCompletedEventId));
            builder.Append("]\ncounters=[");
            builder.Append(string.Join(",", _dictCounter.Select(counter => $"{counter.Key}:{counter.Value}")));
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
