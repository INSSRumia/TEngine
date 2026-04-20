using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using GameLogic.Gameplay.Expedition;

namespace GameLogic
{
	[Window(UILayer.UI, location : "ExpeditionResultUI")]
	public partial class ExpeditionResultUI
	{
		#region 事件

		private partial void OnClickConfirmBtn()
		{
            ExpeditionFlowController.Instance.AcknowledgeSettlement();
		}

		#endregion
        protected override void OnRefresh()
        {
            var summary = ExpeditionFlowController.Instance.GetDisplayableResult();
            if (summary == null)
            {
                _tmpTitle.text = "远征结果";
                _tmpSummary.text = "当前没有可展示的远征结果。";
                return;
            }

            _tmpTitle.text = summary.IsVictory ? "远征完成" : "远征失败";
            _tmpSummary.text = summary.ToDisplayText();
        }
	}
}
