using GameLogic.Gameplay.Combat.Equipment;
using GameLogic.Gameplay.Combat;
using TEngine;
using UnityEngine;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleAbility : Ability<Marble>
    {
        public MarbleAbility() : base(MarbleFactory.GetNextInstAbilityId) { }
    }
}
