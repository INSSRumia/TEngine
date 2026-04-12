using GameLogic.Gameplay.Combat.Equipment;
using GameLogic.GamePlay.Combat;
using TEngine;
using UnityEngine;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleAbility : Ability<Marble>
    {
        public MarbleAbility() : base(MarbleFactory.GetNextInstId) { }
    }
}
