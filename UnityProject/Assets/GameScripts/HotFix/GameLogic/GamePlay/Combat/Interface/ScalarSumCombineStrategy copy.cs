using System.Collections.Generic;

namespace GameLogic.Gameplay.Combat
{
    public class ScalarSumCombineStrategy : IPriorityCombineStrategy<float>
    {
        public float Combine(List<PriorityValue<float>> items)
        {
            if (items == null || items.Count == 0)
                return 0f;
            
            float sum = 0f;
            foreach (var item in items)
            {
                if(item.CombineType == EnumCombineType.Override)
                    return item.Value;

                sum += item.Value;
            }
            return sum;
        }
    }
}
