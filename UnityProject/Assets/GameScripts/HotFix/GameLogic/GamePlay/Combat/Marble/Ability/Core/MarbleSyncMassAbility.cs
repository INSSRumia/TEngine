using UnityEngine;
using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleSyncMassAbility : MarbleAbility
    {
        public MarbleSyncMassAbility(MarbleSyncMassAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Sync();
        }
        public void Sync()
        {
            if (Owner == null || Owner.RuntimeData == null || Owner.Rigidbody == null)
                return;

            var mass = Owner.RuntimeData.Mass;
            if (Mathf.Approximately(Owner.Rigidbody.mass, mass))
                return;

            Owner.Rigidbody.mass = mass;
        }
    }
}
