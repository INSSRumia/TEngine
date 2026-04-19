using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.Gameplay.Combat.Equipment
{
    public class WeaponEquipment : Equipment
    {
        [SerializeField] private List<Collider2D> _damageColliders = new List<Collider2D>();

        public new WeaponRuntimeData RuntimeData => base.RuntimeData as WeaponRuntimeData;

        public bool CanDealDamageFromCollider(Collider2D sourceCollider)
        {
            if (sourceCollider == null)
                return false;

            if (_damageColliders == null || _damageColliders.Count == 0)
                return true;

            return _damageColliders.Contains(sourceCollider);
        }
    }
}
