using TEngine;
using UnityEngine;
using System.Collections.Generic;

namespace GameLogic.GamePlay.Common
{
    public abstract class ASC : MonoBehaviour
    {
        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }
    }

    public abstract class ASC<TRuntimeData> : ASC
    {
        public TRuntimeData RuntimeData { get; private set; }
        private readonly List<IAbility> _lstAbility = new List<IAbility>();
        private readonly List<IAbility> _lstUpdateAbility = new List<IAbility>();
        private readonly List<IAbility> _lstFixedUpdateAbility = new List<IAbility>();
        public IReadOnlyList<IAbility> Abilities => _lstAbility;

        public void Init(TRuntimeData data)
        {
            RuntimeData = data;
        }

        public void AddAbility<TAbilityRuntimeData>(Ability<TAbilityRuntimeData> ability)
            where TAbilityRuntimeData : class
        {
            if (_lstAbility.Contains(ability))
            {
                Log.Error($"重复添加能力: {ability.GetType().Name}");
                return;
            }
            _lstAbility.Add(ability);
            _lstAbility.Sort(SortByPriority);
            RegisterAbilityExecution(ability);
            ability.Init(this);
            ability.OnAdd();
        }

        public void RemoveAbility(IAbility ability)
        {
            if (!_lstAbility.Contains(ability))
            {
                Log.Error($"移除不存在的能力: {ability.GetType().Name}");
                return;
            }
            _lstAbility.Remove(ability);
            _lstUpdateAbility.Remove(ability);
            _lstFixedUpdateAbility.Remove(ability);
            ability.OnRemove();
        }

        public TAbility GetAbility<TAbility>() where TAbility : class
        {
            foreach (var ability in _lstAbility)
            {
                if (ability is TAbility result)
                    return result;
            }
            return null;
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (RuntimeData == null)
                return;

            foreach (var ability in _lstUpdateAbility)
            {
                ability.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        public void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var ability in _lstFixedUpdateAbility)
            {
                ability.OnFixedUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        private void RegisterAbilityExecution(IAbility ability)
        {
            var executionMode = ability.ExecutionMode;
            if ((executionMode & AbilityExecutionMode.Update) != 0)
            {
                _lstUpdateAbility.Add(ability);
                _lstUpdateAbility.Sort(SortByPriority);
            }

            if ((executionMode & AbilityExecutionMode.FixedUpdate) != 0)
            {
                _lstFixedUpdateAbility.Add(ability);
                _lstFixedUpdateAbility.Sort(SortByPriority);
            }
        }

        private static int SortByPriority(IAbility a, IAbility b)
        {
            return b.Priority.CompareTo(a.Priority);
        }
    }
}
