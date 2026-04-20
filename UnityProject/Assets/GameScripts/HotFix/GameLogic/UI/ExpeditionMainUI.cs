using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using GameLogic.Gameplay.Expedition;
using System.Linq;

namespace GameLogic
{
	[Window(UILayer.UI, location : "ExpeditionMainUI")]
	public partial class ExpeditionMainUI
	{
		#region 事件

		private partial void OnClickStartExpeditionBtn()
		{
            ExpeditionFlowController.Instance.StartMinimalExpedition();
		}

		private partial void OnClickCombatDebugBtn()
		{
            ExpeditionFlowController.Instance.StartCombatDebug();
		}
		#endregion

        private static string BuildMarbleSummary(ExpeditionPersistentDataStore persistentData)
        {
            return string.Join("\n", persistentData.Marbles.Select(marble =>
                $"- {marble.DisplayName} ({marble.ConfigId}) HP {marble.CurrentHp}/{marble.MaxHp} EXP {marble.Exp} {(marble.IsDead ? "[不可用]" : "[可出征]")}"));
        }
        protected override void OnRefresh()
        {
            var controller = ExpeditionFlowController.Instance;
            var persistentData = controller.PersistentData;

            _tmpTittle.text = "远征入口 / 出征准备";
            _tmpMarbleSummary.text = $"当前晶体: {persistentData.Crystal}\n\n可出征 Marble:\n{BuildMarbleSummary(persistentData)}";

            var resultSummary = controller.GetDisplayableResult();
            _tmpLastResult.text = resultSummary == null
                ? "上次远征结果: 暂无记录"
                : $"上次远征结果:\n{resultSummary.ToDisplayText()}";
        }
	}
}
