using UnityEngine;
using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleSyncMassAbility : Ability<IPhysics>
    {
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
