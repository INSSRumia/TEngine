namespace GameLogic.Gameplay.Combat.Marble
{
    public abstract class TimedMarbleAbility : MarbleAbility, IAbilityUpdate
    {
        protected IAbilityTiming Timing { get; private set; }
        protected bool IsActive => Timing != null && Timing.IsActive;
        protected bool IsCooldown => Timing != null && Timing.IsCooldown;
        protected bool CanActivate => Timing != null && Timing.CanActivate;
        protected float ActiveRemaining => Timing?.ActiveRemaining ?? 0f;
        protected float CooldownRemaining => Timing?.CooldownRemaining ?? 0f;

        protected void SetTiming(IAbilityTiming timing)
        {
            if (Timing != null)
            {
                UnbindTimingEvents(Timing);
            }

            Timing = timing;
            if (Timing != null)
            {
                Timing.Reset();
                BindTimingEvents(Timing);
            }
        }

        public override void OnRemove()
        {
            base.OnRemove();
            if (Timing != null)
            {
                UnbindTimingEvents(Timing);
                Timing = null;
            }
        }

        public void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false || Timing == null)
                return;

            Timing.Update(elapseSeconds);
            OnTimingUpdated(elapseSeconds, realElapseSeconds);
        }

        protected bool TryActivateTiming()
        {
            return Timing != null && Timing.TryActivate();
        }

        public void InitializeTiming(IAbilityTiming timing)
        {
            SetTiming(timing);
        }

        protected virtual void OnTimingUpdated(float elapseSeconds, float realElapseSeconds) { }
        protected virtual void OnTimingActivated() { }
        protected virtual void OnTimingActiveEnded() { }
        protected virtual void OnTimingCooldownEnded() { }

        private void BindTimingEvents(IAbilityTiming timing)
        {
            timing.Activated += OnTimingActivated;
            timing.ActiveEnded += OnTimingActiveEnded;
            timing.CooldownEnded += OnTimingCooldownEnded;
        }

        private void UnbindTimingEvents(IAbilityTiming timing)
        {
            timing.Activated -= OnTimingActivated;
            timing.ActiveEnded -= OnTimingActiveEnded;
            timing.CooldownEnded -= OnTimingCooldownEnded;
        }
    }
}
