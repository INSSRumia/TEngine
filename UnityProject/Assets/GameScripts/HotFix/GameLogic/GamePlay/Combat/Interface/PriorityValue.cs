using System;

namespace GameLogic.Gameplay.Combat
{
    public struct PriorityValue<T> where T : struct
    {
        public readonly int Id;
        public readonly T Value;
        public readonly int Priority;
        public readonly EnumCombineType CombineType;

        public PriorityValue(int id, T value, int priority, EnumCombineType combineType)
        {
            Id = id;
            Value = value;
            Priority = priority;
            CombineType = combineType;
        }
    }
}
