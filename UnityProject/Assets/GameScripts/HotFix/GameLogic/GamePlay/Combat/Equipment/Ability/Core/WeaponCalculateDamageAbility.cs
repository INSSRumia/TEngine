using UnityEngine;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Equipment
{
    /// <summary>
    /// 武器伤害计算入口。
    /// 该能力负责把武器自身配置与 OwnerMarble 的攻击投影汇总为一次可发射/可命中的伤害值，
    /// 供 BowFireAbility、SwordCollisionAttackAbility 等攻击能力复用。
    /// </summary>
    public class WeaponCalculateDamageAbility : EquipmentAbility
    {
        private WeaponEquipment _owner;
        public int? Attack {get; private set;}
        public WeaponCalculateDamageAbility(WeaponCalculateDamageAbilityConfig config)
        {
            Attack = config.Attack;
        }
        public override void OnAdd()
        {
            base.OnAdd();
            if(EquipmentOwner is WeaponEquipment weaponEquipment)
                _owner = weaponEquipment;
        }
        public override void OnRemove()
        {
            base.OnRemove();
            _owner = null;
        }
        private const float VELOCITY_DAMAGE_FACTOR = 1f;

        public int CalculateDamage()
        {
            if (_owner.RuntimeData.IsBroken)
                return 0;

            // Attack 为空表示沿用 OwnerMarble 当前攻击力；否则使用武器配置覆盖基础攻击。
            bool isUseOwnerAttack = Attack == null;
            int attack = isUseOwnerAttack ? _owner.OwnerMarble.RuntimeData.Config.Attack : Attack.Value;
            // TODO: Marble应该提供一个计算伤害的核心Ability，计算伤害时应该使用这个Ability
            int attackAddition = _owner.OwnerMarble.RuntimeData.Config.AttackAddition;
            float attackMultiplier = _owner.OwnerMarble.RuntimeData.Config.AttackMultiplier;
            attack = Mathf.RoundToInt((attack + attackAddition) * attackMultiplier);

            return Mathf.Max(0, attack);
        }
    }
}
