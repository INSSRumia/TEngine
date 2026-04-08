using UnityEngine;

namespace GameLogic.GamePlay.Common
{
    public interface IAbility
    {
        int Priority { get; }
        AbilityExecutionMode ExecutionMode { get; }
        void Init(ASC owner);
        void OnAdd();
        void OnRemove();
        void OnUpdate(float elapseSeconds, float realElapseSeconds);
        void OnFixedUpdate(float elapseSeconds, float realElapseSeconds);
    }

    [System.Flags]
    public enum AbilityExecutionMode
    {
        None = 0,
        Update = 1 << 0,
        FixedUpdate = 1 << 1,
    }

    public abstract class Ability<TRuntimeData> : IAbility
    {
        public virtual int Priority { get; protected set; } = 10000;
        public virtual AbilityExecutionMode ExecutionMode => AbilityExecutionMode.None;

        private ASC _owner;
        public ASC<TRuntimeData> Owner => _owner as ASC<TRuntimeData>;

        public virtual void Init(ASC owner)
        {
            _owner = owner;
        }

        public virtual void OnAdd() { }
        public virtual void OnRemove() { }
        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) { }
        public virtual void OnFixedUpdate(float elapseSeconds, float realElapseSeconds) { }

        public static int SortByPriority(Ability<TRuntimeData> a, Ability<TRuntimeData> b)
        {
            return b.Priority.CompareTo(a.Priority);
        }
    }
}
