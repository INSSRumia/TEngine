using UnityEngine;
using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleSyncScaleAbility : Ability<IScale>
    {
        public override void OnAdd()
        {
            base.OnAdd();
            Sync();
        }
        public void Sync()
        {
            if (Owner == null || Owner.RuntimeData == null)
                return;

            var scale = Owner.RuntimeData.Scale;
            var curScale = Owner.transform.localScale.x;
            if (Mathf.Approximately(curScale, scale))
                return;

            Owner.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
