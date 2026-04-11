using UnityEngine;

namespace GameLogic.GamePlay.Combat
{
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
