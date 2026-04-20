using GameLogic.Gameplay.Expedition;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    // [Window(UILayer.Top, location: ExpeditionConstants.SharedUiPrefabLocation, fullScreen: true)]
    // class EventCardUI : UIWindow
    // {
    //     private Text _textTitle;
    //     private Text _textDescription;
    //     private Button _btnOptionA;
    //     private Button _btnOptionB;

    //     protected override void ScriptGenerator()
    //     {
    //         // var root = ExpeditionRuntimeUiFactory.EnsureWindowRoot(rectTransform, "m_goRuntimeRoot", new Color(0.08f, 0.07f, 0.12f, 0.94f));
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textTitle", new Vector2(0f, 240f), new Vector2(620f, 80f), 40, TextAnchor.MiddleCenter);
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textDescription", new Vector2(0f, 40f), new Vector2(620f, 240f), 26, TextAnchor.UpperLeft);
    //         // ExpeditionRuntimeUiFactory.EnsureButton(root, "m_btnOptionA", new Vector2(0f, -170f), new Vector2(440f, 80f), "选项 A");
    //         // ExpeditionRuntimeUiFactory.EnsureButton(root, "m_btnOptionB", new Vector2(0f, -280f), new Vector2(440f, 80f), "选项 B");

    //         _textTitle = FindChildComponent<Text>("m_goRuntimeRoot/m_textTitle");
    //         _textDescription = FindChildComponent<Text>("m_goRuntimeRoot/m_textDescription");
    //         _btnOptionA = FindChildComponent<Button>("m_goRuntimeRoot/m_btnOptionA");
    //         _btnOptionB = FindChildComponent<Button>("m_goRuntimeRoot/m_btnOptionB");
    //     }

    //     protected override void OnCreate()
    //     {
    //         _btnOptionA?.onClick.AddListener(() => SubmitOption(0));
    //         _btnOptionB?.onClick.AddListener(() => SubmitOption(1));
    //     }

    //     protected override void OnRefresh()
    //     {
    //         var eventNode = ExpeditionFlowController.Instance.GetCurrentEventNode();
    //         if (eventNode == null)
    //         {
    //             _textTitle.text = "事件缺失";
    //             _textDescription.text = "当前没有可执行的事件节点。";
    //             // ExpeditionRuntimeUiFactory.SetButtonLabel(_btnOptionA, "返回");
    //             // ExpeditionRuntimeUiFactory.SetButtonLabel(_btnOptionB, "返回");
    //             return;
    //         }

    //         _textTitle.text = eventNode.Title;
    //         _textDescription.text = eventNode.Description;

    //         var optionA = eventNode.Options.Count > 0 ? eventNode.Options[0] : null;
    //         var optionB = eventNode.Options.Count > 1 ? eventNode.Options[1] : null;
    //         // ExpeditionRuntimeUiFactory.SetButtonLabel(_btnOptionA, optionA == null ? "无可选项" : $"{optionA.Title}\n{optionA.Description}");
    //         // ExpeditionRuntimeUiFactory.SetButtonLabel(_btnOptionB, optionB == null ? "无可选项" : $"{optionB.Title}\n{optionB.Description}");
    //         _btnOptionA.interactable = optionA != null;
    //         _btnOptionB.interactable = optionB != null;
    //     }

    //     protected override void OnDestroy()
    //     {
    //         _btnOptionA?.onClick.RemoveAllListeners();
    //         _btnOptionB?.onClick.RemoveAllListeners();
    //     }

    //     private void SubmitOption(int optionIndex)
    //     {
    //         var eventNode = ExpeditionFlowController.Instance.GetCurrentEventNode();
    //         if (eventNode == null || optionIndex < 0 || optionIndex >= eventNode.Options.Count)
    //         {
    //             return;
    //         }

    //         ExpeditionFlowController.Instance.SubmitEventChoice(eventNode.Options[optionIndex].OptionId);
    //     }
    // }
}
