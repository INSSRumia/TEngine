using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class DirectionCombineStrategy : ICombineStrategy<Vector2>
    {
        public Vector2 Combine(List<PriorityValue<Vector2>> items)
        {
            if (items == null || items.Count == 0)
                return Vector2.zero;

            Vector2 result = Vector2.zero;
            foreach (var item in items)
            {
                result += item.Value;
            }

            if (result.sqrMagnitude < 0.001f)
                return Vector2.zero;

            return result.normalized;
        }
    }
}
