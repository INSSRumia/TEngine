using System.Collections.Generic;

namespace GameLogic.Gameplay.Combat
{
    public class ScalarMaxCombineStrategy : IPriorityCombineStrategy<float>
    {
        public float Combine(List<PriorityValue<float>> items)
        {
            if (items == null || items.Count == 0)
                return 0f;

            if(items[0].CombineType == EnumCombineType.Override)
                return items[0].Value;

            float maxValue = items[0].Value;
            for (int i = 1; i < items.Count; i++)
            {
                float v = items[i].Value;
                if (v > maxValue)
                    maxValue = v;
            }
            return maxValue;
        }
    }
}
