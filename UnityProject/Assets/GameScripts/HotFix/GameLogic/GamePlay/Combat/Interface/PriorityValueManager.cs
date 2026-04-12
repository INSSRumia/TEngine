using System;
using System.Collections.Generic;

namespace GameLogic.Gameplay.Combat
{
    public class PriorityValueManager<T> where T : struct
    {
        private readonly List<PriorityValue<T>> _items = new List<PriorityValue<T>>();
        private readonly ICombineStrategy<T> _strategy;

        public PriorityValueManager(ICombineStrategy<T> strategy)
        {
            _strategy = strategy;
        }

        public void Add(PriorityValue<T> item)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].Id == item.Id)
                {
                    _items[i] = item;
                    return;
                }
            }
            _items.Add(item);
        }

        public void RemoveById(int id)
        {
            _items.RemoveAll(item => item.Id == id);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public T GetCombinedValue()
        {
            if (_items.Count == 0)
                return default;

            _items.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            PriorityValue<T> topItem = _items[0];
            if (topItem.CombineType == EnumCombineType.Override)
            {
                _items.Clear();
                return topItem.Value;
            }

            T result = _strategy.Combine(_items);
            _items.Clear();
            return result;
        }
    }
}
