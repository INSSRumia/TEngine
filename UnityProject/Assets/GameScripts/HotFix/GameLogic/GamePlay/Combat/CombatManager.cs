using TEngine;
using UnityEngine;
using System.Collections.Generic;
namespace GameLogic.Gameplay.Combat
{
    public interface ICombatManager
    {
        Marble.Marble GetNearestEnemy(Marble.Marble marble);
        Marble.Marble GetTarget(int instId);
        void Register(Marble.Marble marble);
        void Unregister(Marble.Marble marble);
        bool IsEnemy(Marble.Marble a, Marble.Marble b);
        IReadOnlyList<Marble.Marble> GetAllActiveMarbles();
    }

    public class CombatManager : ICombatManager
    {
        private readonly List<Marble.Marble> _activeMarbles = new List<Marble.Marble>();
        private int _nextInstId = 1;

        public CombatManager()
        {
            Instance = this;
        }

        public static CombatManager Instance { get; private set; }

        public void Register(Marble.Marble marble)
        {
            if (marble == null || marble.RuntimeData == null)
                return;

            if (!_activeMarbles.Contains(marble))
            {
                if (marble.RuntimeData.InstId == 0)
                    marble.RuntimeData.InstId = _nextInstId++;
                _activeMarbles.Add(marble);
            }
        }

        public void Unregister(Marble.Marble marble)
        {
            if (marble != null)
                _activeMarbles.Remove(marble);
        }

        public Marble.Marble GetNearestEnemy(Marble.Marble marble)
        {
            if (marble?.RuntimeData == null)
                return null;

            Marble.Marble nearest = null;
            float minDist = float.MaxValue;
            var selfCamp = marble.RuntimeData.Camp;

            foreach (var other in _activeMarbles)
            {
                if (other == null || other.RuntimeData == null || !other.RuntimeData.IsAlive)
                    continue;

                if (other.RuntimeData.Camp == selfCamp)
                    continue;

                float dist = Vector2.Distance(marble.transform.position, other.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = other;
                }
            }

            return nearest;
        }

        public Marble.Marble GetTarget(int instId)
        {
            foreach (var marble in _activeMarbles)
            {
                if (marble?.RuntimeData != null && marble.RuntimeData.InstId == instId)
                    return marble;
            }
            return null;
        }

        public bool IsEnemy(Marble.Marble a, Marble.Marble b)
        {
            if (a?.RuntimeData == null || b?.RuntimeData == null)
                return false;
            return a.RuntimeData.Camp != b.RuntimeData.Camp;
        }

        public IReadOnlyList<Marble.Marble> GetAllActiveMarbles()
        {
            return _activeMarbles;
        }
    }
}

