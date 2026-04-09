using UnityEngine;

namespace GameLogic.GamePlay.Common
{
    public interface IAbility
    {
        int Priority { get; }
        void Init(ASC owner);
        void OnAdd();
        void OnRemove();
    }

    public interface IAbilityUpdate
    {
        void OnAbilityUpdate(float elapseSeconds, float realElapseSeconds);
    }

    public interface IAbilityFixedUpdate
    {
        void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds);
    }

    public interface IReceiveDamage
    {
        void ReceiveDamage(int value, ASC source = null);
    }

    public interface IReceiveHeal
    {
        void ReceiveHeal(int value, ASC source = null);
    }

    public interface IReceiveShield
    {
        void ReceiveShield(int value, ASC source = null);
    }

    public interface IAfterReceiveDamage
    {
        void AfterReceiveDamage(IAbility ability);
    }

    public interface IAfterCalculateDamage
    {
        void AfterCalculateDamage(IAbility ability);
    }

    public interface IAfterApplyDamage
    {
        void AfterApplyDamage(IAbility ability);
    }

    public interface IAfterReceiveHeal
    {
        void AfterReceiveHeal(IAbility ability);
    }

    public interface IAfterCalculateHeal
    {
        void AfterCalculateHeal(IAbility ability);
    }

    public interface IAfterApplyHeal
    {
        void AfterApplyHeal(IAbility ability);
    }

    public interface IAfterReceiveShield
    {
        void AfterReceiveShield(IAbility ability);
    }

    public interface IAfterCalculateShield
    {
        void AfterCalculateShield(IAbility ability);
    }

    public interface IAfterApplyShield
    {
        void AfterApplyShield(IAbility ability);
    }

    public abstract class Ability<TRuntimeData> : IAbility
    {
        public virtual int Priority { get; protected set; } = 0;

        private ASC _owner;
        public ASC<TRuntimeData> Owner => _owner as ASC<TRuntimeData>;

        public virtual void Init(ASC owner)
        {
            _owner = owner;
        }

        public virtual void OnAdd() { }
        public virtual void OnRemove() { }

        public static int SortByPriority(Ability<TRuntimeData> a, Ability<TRuntimeData> b)
        {
            return b.Priority.CompareTo(a.Priority);
        }
    }
}
