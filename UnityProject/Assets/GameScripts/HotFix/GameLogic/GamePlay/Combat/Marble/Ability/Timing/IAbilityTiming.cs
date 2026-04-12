using System;

namespace GameLogic.Gameplay.Combat.Marble
{
    public interface IAbilityTiming
    {
        bool IsActive { get; }
        bool IsCooldown { get; }
        bool CanActivate { get; }
        float ActiveRemaining { get; }
        float CooldownRemaining { get; }

        event Action Activated;
        event Action ActiveEnded;
        event Action CooldownEnded;

        void Reset();
        bool TryActivate();
        void Update(float deltaTime);
    }
}
