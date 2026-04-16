using System;
using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class RandomRangeAbilityTiming : IAbilityTiming
    {
        private readonly float _minDuration;
        private readonly float _maxDuration;
        private readonly float _minCooldown;
        private readonly float _maxCooldown;
        private readonly bool _autoActivate;

        public bool IsActive => ActiveRemaining > 0f;
        public bool IsCooldown => CooldownRemaining > 0f;
        public bool CanActivate => !IsActive && !IsCooldown;
        public float ActiveRemaining { get; private set; }
        public float CooldownRemaining { get; private set; }

        public event Action Activated;
        public event Action ActiveEnded;
        public event Action CooldownEnded;

        public RandomRangeAbilityTiming(RandomRangeAbilityTimingConfig config)
            : this(config.MinDuration, config.MaxDuration, config.MinCooldown, config.MaxCooldown, config.AutoActivate)
        {
        }

        public RandomRangeAbilityTiming(float minDuration, float maxDuration, float minCooldown, float maxCooldown, bool autoActivate = false)
        {
            _minDuration = Mathf.Max(0f, Mathf.Min(minDuration, maxDuration));
            _maxDuration = Mathf.Max(_minDuration, Mathf.Max(minDuration, maxDuration));
            _minCooldown = Mathf.Max(0f, Mathf.Min(minCooldown, maxCooldown));
            _maxCooldown = Mathf.Max(_minCooldown, Mathf.Max(minCooldown, maxCooldown));
            _autoActivate = autoActivate;
        }

        public void Reset()
        {
            ActiveRemaining = 0f;
            CooldownRemaining = 0f;
        }

        public bool TryActivate()
        {
            if (!CanActivate)
                return false;

            ActiveRemaining = Sample(_minDuration, _maxDuration);
            Activated?.Invoke();

            if (ActiveRemaining <= 0f)
            {
                CompleteActivePhase();
            }
            return true;
        }

        public void Update(float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);

            if (IsActive)
            {
                ActiveRemaining -= deltaTime;
                if (ActiveRemaining <= 0f)
                {
                    CompleteActivePhase();
                }
                return;
            }

            if (IsCooldown)
            {
                CooldownRemaining -= deltaTime;
                if (CooldownRemaining <= 0f)
                {
                    CooldownRemaining = 0f;
                    CooldownEnded?.Invoke();
                    if (_autoActivate)
                    {
                        TryActivate();
                    }
                }
                return;
            }

            if (_autoActivate)
            {
                TryActivate();
            }
        }

        private void CompleteActivePhase()
        {
            ActiveRemaining = 0f;
            ActiveEnded?.Invoke();
            CooldownRemaining = Sample(_minCooldown, _maxCooldown);
            if (CooldownRemaining <= 0f)
            {
                CooldownRemaining = 0f;
                CooldownEnded?.Invoke();
                if (_autoActivate)
                {
                    TryActivate();
                }
            }
        }

        private static float Sample(float min, float max)
        {
            if (Mathf.Approximately(min, max))
                return min;
            return UnityEngine.Random.Range(min, max);
        }
    }
}
