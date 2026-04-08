using UnityEngine;
using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleHandleDamageAbility : Ability<MarbleRuntimeData>
    {
        public override int Priority => 9900;

        public void Resolve()
        {
            if (!Owner.RuntimeData.IsAlive)
                return;
            
            int hp = Owner.RuntimeData.Hp;
            int maxHp = Owner.RuntimeData.MaxHp;
            int shield = Owner.RuntimeData.Shield;
            int maxShield = Owner.RuntimeData.MaxShield;
            int damage = Owner.RuntimeData.PendingDamage;
            int heal = Owner.RuntimeData.PendingHeal;
            int defense = Owner.RuntimeData.Defense;

            damage -= defense;
            damage = Mathf.Max(damage, 0);

            // 只要有护盾值存在，就能完全抵挡伤害
            if (shield > 0)
            {
                shield = shield - damage;
                shield = Mathf.Clamp(shield, 0, maxShield);
                damage = 0;
            }
            
            hp += heal - damage;
            hp = Mathf.Clamp(hp, 0, maxHp);
            
            Owner.RuntimeData.Hp = hp;
            Owner.RuntimeData.Shield = shield;
            Owner.RuntimeData.PendingDamage = 0;
            Owner.RuntimeData.PendingHeal = 0;
        }
    }
}
