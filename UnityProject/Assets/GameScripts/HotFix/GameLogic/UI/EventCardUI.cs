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
				SetOptionA(true, "返回", "");
				SetOptionB(true, "返回", "");
                return;
            }

            _tmpTitle.text = eventNode.Title;
            _tmpDesc.text = eventNode.Description;

            var optionA = eventNode.Options.Count > 0 ? eventNode.Options[0] : null;
            var optionB = eventNode.Options.Count > 1 ? eventNode.Options[1] : null;
			SetOptionA(optionA != null, optionA == null ? "无可选项" : optionA.Title, optionA == null ? "" : optionA.Description);
			SetOptionB(optionB != null, optionB == null ? "无可选项" : optionB.Title, optionB == null ? "" : optionB.Description);
        }

		private void SetOptionA(bool interactable, string title, string desc)
		{
			_btnOptionA.interactable = interactable;
			_tmpOptionATitle.text = title;
			_tmpOptionADesc.text = desc;
		}
        private void SetOptionB(bool interactable, string title, string desc)
        {
            _btnOptionB.interactable = interactable;
            _tmpOptionBTitle.text = title;
            _tmpOptionBDesc.text = desc;
        }
	}
}
