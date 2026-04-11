using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleGetTargetAbility : Ability<MarbleRuntimeData>, IFixedUpdate
    {
        public void OnFixedUpdate()
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false)
                return;

            // TODO: 从战斗管理器中获取最近的敌人
            Marble target = Owner.CombatManager?.GetNearestEnemy(Owner as Marble);

            if(target == null)
                return;

            Owner.RuntimeData.TargetMarbleInstId = target.RuntimeData.InstId;
        }

    }
}
