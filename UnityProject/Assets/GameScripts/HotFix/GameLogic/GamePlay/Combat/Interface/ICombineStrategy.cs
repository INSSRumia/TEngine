using System.Collections.Generic;

namespace GameLogic.Gameplay.Combat
{
    public interface ICombineStrategy<T> where T : struct
    {
        T Combine(List<PriorityValue<T>> items);
    }
}
