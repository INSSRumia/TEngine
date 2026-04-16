using System;
using GameConfig.Gameplay.Combat;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class FixedDurationAbilityTiming : IAbilityTiming
    {
        private readonly float _duration;
        private readonly float _cooldown;
        private readonly bool _autoActivate;

        public bool IsActive => ActiveRemaining > 0f;
        public bool IsCooldown => CooldownRemaining > 0f;
        public bool CanActivate => !IsActive && !IsCooldown;
        public float ActiveRemaining { get; private set; }
        public float CooldownRemaining { get; private set; }

        public event Action Activated;
        public event Action ActiveEnded;
        public event Action CooldownEnded;

        public FixedDurationAbilityTiming(FixedAbilityTimingConfig config)
            : this(config.Duration, config.Cooldown, config.AutoActivate)
        {
        }

        public FixedDurationAbilityTiming(float duration, float cooldown, bool autoActivate = false)
        {
            _duration = Mathf.Max(0f, duration);
            _cooldown = Mathf.Max(0f, cooldown);
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

            ActiveRemaining = _duration;
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
            CooldownRemaining = _cooldown;
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
    }
}
