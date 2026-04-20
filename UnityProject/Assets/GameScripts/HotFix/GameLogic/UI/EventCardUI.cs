using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using GameLogic.Gameplay.Expedition;

namespace GameLogic
{
	[Window(UILayer.UI, location : "EventCardUI")]
	public partial class EventCardUI
	{
		#region 事件

		private partial void OnClickOptionABtn()
		{
			SubmitOption(0);
		}

		private partial void OnClickOptionBBtn()
		{
			SubmitOption(1);
		}

		#endregion
        private void SubmitOption(int optionIndex)
        {
            var eventNode = ExpeditionFlowController.Instance.GetCurrentEventNode();
            if (eventNode == null || optionIndex < 0 || optionIndex >= eventNode.Options.Count)
            {
                return;
            }

            ExpeditionFlowController.Instance.SubmitEventChoice(eventNode.Options[optionIndex].OptionId);
        }

        protected override void OnRefresh()
        {
            var eventNode = ExpeditionFlowController.Instance.GetCurrentEventNode();
            if (eventNode == null)
            {
                _tmpTitle.text = "事件缺失";
                _tmpDesc.text = "当前没有可执行的事件节点。";
				SetOption(_btnOptionA, "返回", "");
				SetOption(_btnOptionB, "返回", "");
                return;
            }

            _tmpTitle.text = eventNode.Title;
            _tmpDesc.text = eventNode.Description;

            var optionA = eventNode.Options.Count > 0 ? eventNode.Options[0] : null;
            var optionB = eventNode.Options.Count > 1 ? eventNode.Options[1] : null;
			SetOption(_btnOptionA, optionA == null ? "无可选项" : optionA.Title, optionA == null ? "" : optionA.Description);
			SetOption(_btnOptionB, optionB == null ? "无可选项" : optionB.Title, optionB == null ? "" : optionB.Description);
            _btnOptionA.interactable = optionA != null;
            _btnOptionB.interactable = optionB != null;
        }

		private void SetOption(Button button, string title, string desc)
		{
			button.interactable = true;
			_tmpOptionATitle.text = title;
			_tmpOptionADesc.text = desc;
		}
	}
}
