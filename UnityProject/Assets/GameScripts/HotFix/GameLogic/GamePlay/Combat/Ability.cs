using UnityEngine;

namespace GameLogic.GamePlay.Combat
{
    public interface IAbility
    {
        int Priority { get; }
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

    public abstract class Ability<T> : IAbility
    {
        public virtual int Priority { get; protected set; } = 0;
        public T Owner {get; private set;}

        public virtual void Init(T owner)
        {
            Owner = owner;
        }

        public virtual void OnAdd() { }
        public virtual void OnRemove() { }

        public static int SortByPriority(Ability<T> a, Ability<T> b)
        {
            return b.Priority.CompareTo(a.Priority);
        }
    }
}
