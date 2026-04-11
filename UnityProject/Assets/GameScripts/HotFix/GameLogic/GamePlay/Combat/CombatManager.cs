using TEngine;
using UnityEngine;
using System.Collections.Generic;
using System;
using GameLogic.Gameplay.Combat.Marble;

namespace GameLogic.GamePlay.Combat
{
    public interface ICombatManager
    {
        Marble GetNearestEnemy(Marble marble);
        Marble GetTarget(int instId);
        void Register(Marble marble);
        void Unregister(Marble marble);
        bool IsEnemy(Marble a, Marble b);
        IReadOnlyList<Marble> GetAllActiveMarbles();
    }

    public class CombatManager : ICombatManager
    {
        private readonly List<Marble> _activeMarbles = new List<Marble>();
        private int _nextInstId = 1;

        public CombatManager()
        {
            Instance = this;
        }

        public static CombatManager Instance { get; private set; }

        public void Register(Marble marble)
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

        public void Unregister(Marble marble)
        {
            if (marble != null)
                _activeMarbles.Remove(marble);
        }

        public Marble GetNearestEnemy(Marble marble)
        {
            if (marble?.RuntimeData == null)
                return null;

            Marble nearest = null;
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

        public Marble GetTarget(int instId)
        {
            foreach (var marble in _activeMarbles)
            {
                if (marble?.RuntimeData != null && marble.RuntimeData.InstId == instId)
                    return marble;
            }
            return null;
        }

        public bool IsEnemy(Marble a, Marble b)
        {
            if (a?.RuntimeData == null || b?.RuntimeData == null)
                return false;
            return a.RuntimeData.Camp != b.RuntimeData.Camp;
        }

        public IReadOnlyList<Marble> GetAllActiveMarbles()
        {
            return _activeMarbles;
        }
    }
}

