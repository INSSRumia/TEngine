using TEngine;
using UnityEngine;
using System.Collections.Generic;
using System;
using GameLogic.Marble;

namespace GameLogic.GamePlay.Common
{
    public interface ICombatManager
    {
        Marble.Marble GetNearestEnemy(Marble.Marble marble);
        Marble.Marble GetTarget(int instId);
    }
}
