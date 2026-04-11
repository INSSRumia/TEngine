using TEngine;
using UnityEngine;
using System.Collections.Generic;
using System;
using GameLogic.Gameplay.Combat.Marble;

namespace GameLogic.GamePlay.Combat
{
    public interface ICombatManager
    {
        Gameplay.Combat.Marble.Marble GetNearestEnemy(Gameplay.Combat.Marble.Marble marble);
        Gameplay.Combat.Marble.Marble GetTarget(int instId);
    }
}
