using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class AngularVelocityCombineStrategy : IPriorityCombineStrategy<float>
    {
        public float Combine(List<PriorityValue<float>> items)
        {
            if (items == null || items.Count == 0)
                return 0f;

            float sum = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                if(items[i].CombineType == EnumCombineType.Override)
                    return items[i].Value;
                sum += items[i].Value;
            }
            return sum;
        }
    }
}
