using UnityEngine;

namespace GameLogic.GamePlay.Combat
{
    public abstract class Ability<T> : IAbility
    {
        public virtual int InstId { get; }
        public virtual int Priority { get; set; } = 0;
        public virtual AbilityCategory Category { get; set; } = AbilityCategory.Optional;
        public T Owner {get; private set;}

        public Ability(int instId)
        {
            InstId = instId;
        }

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
