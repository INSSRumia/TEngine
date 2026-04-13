using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.Gameplay.Combat
{
    public class ProjectileRuntimeData : RuntimeData
    {
        public int SourceCamp { get; set; }
        public int SourceMarbleInstId { get; set; }
        public int TargetMarbleInstId { get; set; }
        public Vector2 TargetPoint { get; set; }
        public float CurrentLifetime { get; set; }
        public bool IsFinishedLifetime { get; set; } = false;
        public int Damage { get; set; }
        public int RemainPiercingCount { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 TargetDirection { get; set; }

        private readonly HashSet<int> _hitTargets = new HashSet<int>();

        public ProjectileRuntimeData(string configId, int level) : base(configId, level, ProjectileFactory.GetNextInstId)
        {
        }

        public bool TryMarkHit(int targetInstId)
        {
            if (_hitTargets.Contains(targetInstId))
                return false;
            _hitTargets.Add(targetInstId);
            return true;
        }
    }
}
