using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic.Marble
{
    public class Marble : MonoBehaviour
    {
        public MarbleRuntimeData RuntimeData { get; private set; }
        private readonly List<MarbleAbility> _lstAbility = new List<MarbleAbility>();
        public IReadOnlyList<MarbleAbility> Abilities => _lstAbility;
        
        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Init(MarbleRuntimeData data)
        {
            RuntimeData = data;
            RuntimeData.Marble = this;
        }
        
        public void AddAbility(MarbleAbility ability)
        {
            if(_lstAbility.Contains(ability))
            {
                Log.Error($"重复添加能力: {ability.GetType().Name}");
                return;
            }
            _lstAbility.Add(ability);
            _lstAbility.Sort(MarbleAbility.SortByPriority);
            ability.Init(this);
            ability.OnAdd();
        }
        
        public void RemoveAbility(MarbleAbility ability)
        {
            if(!_lstAbility.Contains(ability))
            {
                Log.Error($"移除不存在的能力: {ability.GetType().Name}");                
                ability.OnRemove();
            }
            _lstAbility.Remove(ability);
        }
        
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var ability in _lstAbility)
            {
                ability.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }
        
        public void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var ability in _lstAbility)
            {
                ability.OnFixedUpdate(elapseSeconds, realElapseSeconds);
            }
        }
    }
}
