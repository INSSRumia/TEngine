using TEngine;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace GameLogic.Gameplay.Combat
{
    public abstract class ASC : MonoBehaviour
    {
        public Rigidbody2D Rigidbody { get; private set; }
        public ICombatManager CombatManager { get; private set; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            CombatManager = Combat.CombatManager.Instance;
        }
    }

    public abstract class ASC<TRuntimeData> : ASC
    {
        [Sirenix.OdinInspector.ShowInInspector]
        public TRuntimeData RuntimeData { get; private set; }
        private readonly List<IAbility> _lstAbility = new List<IAbility>();
        private readonly List<IAbility> _lstUpdateAbility = new List<IAbility>();
        private readonly List<IAbility> _lstFixedUpdateAbility = new List<IAbility>();
        private readonly Dictionary<Type, List<IAbility>> _abilityInterfaceMap = new Dictionary<Type, List<IAbility>>();

        private readonly List<IAbility> _coreAbilities = new List<IAbility>();
        private readonly List<IAbility> _optionalAbilities = new List<IAbility>();
        private readonly List<IAbility> _dynamicAbilities = new List<IAbility>();

        public IReadOnlyList<IAbility> Abilities => _lstAbility;
        public IReadOnlyList<IAbility> CoreAbilities => _coreAbilities;
        public IReadOnlyList<IAbility> OptionalAbilities => _optionalAbilities;
        public IReadOnlyList<IAbility> DynamicAbilities => _dynamicAbilities;

        public virtual void Init(TRuntimeData data)
        {
            RuntimeData = data;
        }

        public void AddAbility<T>(Ability<T> ability) where T : ASC<TRuntimeData>
        {
            if (_lstAbility.Contains(ability))
            {
                Log.Error($"重复添加能力: {ability.GetType().Name}");
                return;
            }

            var category = ability.Category;
            switch (category)
            {
                case AbilityCategory.Core:
                    _coreAbilities.Add(ability);
                    break;
                case AbilityCategory.Optional:
                    _optionalAbilities.Add(ability);
                    break;
                case AbilityCategory.Dynamic:
                    _dynamicAbilities.Add(ability);
                    break;
            }

            _lstAbility.Add(ability);
            _lstAbility.Sort(SortByPriority);
            RegisterAbilityExecution(ability);
            ability.Init(this as T);
            ability.OnAdd();
        }

        public void RemoveAbility(IAbility ability)
        {
            if (!_lstAbility.Contains(ability))
            {
                Log.Error($"移除不存在的能力: {ability.GetType().Name}");
                return;
            }

            var category = ability.Category;
            switch (category)
            {
                case AbilityCategory.Core:
                    _coreAbilities.Remove(ability);
                    break;
                case AbilityCategory.Optional:
                    _optionalAbilities.Remove(ability);
                    break;
                case AbilityCategory.Dynamic:
                    _dynamicAbilities.Remove(ability);
                    break;
            }

            _lstAbility.Remove(ability);
            _lstUpdateAbility.Remove(ability);
            _lstFixedUpdateAbility.Remove(ability);
            UnregisterAbilityInterfaces(ability);
            ability.OnRemove();
        }

        public void RemoveAllAbilities()
        {
            _lstUpdateAbility.Clear();
            _lstFixedUpdateAbility.Clear();
            _abilityInterfaceMap.Clear();
            _coreAbilities.Clear();
            _optionalAbilities.Clear();
            _dynamicAbilities.Clear();
            foreach (var ability in _lstAbility)
            {
                ability.OnRemove();
            }
            _lstAbility.Clear();
        }

        public void ClearOptionalAbilities()
        {
            for (int i = _optionalAbilities.Count - 1; i >= 0; i--)
            {
                RemoveAbility(_optionalAbilities[i]);
            }
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

        private void Update()
        {
            OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void FixedUpdate()
        {
            OnFixedUpdate(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (RuntimeData == null)
                return;

            foreach (var ability in _lstUpdateAbility)
            {
                if (ability is IAbilityUpdate updateAbility)
                {
                    updateAbility.OnAbilityUpdate(elapseSeconds, realElapseSeconds);
                }
            }
        }

        public virtual void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var ability in _lstFixedUpdateAbility)
            {
                if (ability is IAbilityFixedUpdate fixedUpdateAbility)
                {
                    fixedUpdateAbility.OnAbilityFixedUpdate(elapseSeconds, realElapseSeconds);
                }
            }
        }

        private void RegisterAbilityExecution(IAbility ability)
        {
            if (ability is IAbilityUpdate)
            {
                _lstUpdateAbility.Add(ability);
                _lstUpdateAbility.Sort(SortByPriority);
            }

            if (ability is IAbilityFixedUpdate)
            {
                _lstFixedUpdateAbility.Add(ability);
                _lstFixedUpdateAbility.Sort(SortByPriority);
            }

            RegisterAbilityInterfaces(ability);
        }

        public void GetAbilities<TAbility>(ref List<TAbility> result) where TAbility : class
        {
            if (!_abilityInterfaceMap.TryGetValue(typeof(TAbility), out var abilityList))
                return;

            foreach (var ability in abilityList)
            {
                if (ability is TAbility typedAbility)
                    result.Add(typedAbility);
            }
        }

        private void RegisterAbilityInterfaces(IAbility ability)
        {
            var interfaces = ability.GetType().GetInterfaces();
            foreach (var interfaceType in interfaces)
            {
                if (!_abilityInterfaceMap.TryGetValue(interfaceType, out var abilityList))
                {
                    abilityList = new List<IAbility>();
                    _abilityInterfaceMap.Add(interfaceType, abilityList);
                }

                if (!abilityList.Contains(ability))
                {
                    abilityList.Add(ability);
                    abilityList.Sort(SortByPriority);
                }
            }
        }

        private void UnregisterAbilityInterfaces(IAbility ability)
        {
            foreach (var pair in _abilityInterfaceMap)
            {
                pair.Value.Remove(ability);
            }
        }

        private static int SortByPriority(IAbility a, IAbility b)
        {
            return b.Priority.CompareTo(a.Priority);
        }

        private static class s_emptyAbilityList<TAbility>
        {
            public static readonly List<TAbility> Value = new List<TAbility>(0);
        }
    }
}
