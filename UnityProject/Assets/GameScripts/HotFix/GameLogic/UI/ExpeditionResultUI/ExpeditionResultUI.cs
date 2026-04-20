using GameLogic.Gameplay.Expedition;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    // [Window(UILayer.Top, location: ExpeditionConstants.SharedUiPrefabLocation, fullScreen: true)]
    // class ExpeditionResultUI : UIWindow
    // {
    //     private Text _textTitle;
    //     private Text _textSummary;
    //     private Button _btnConfirm;

    //     protected override void ScriptGenerator()
    //     {
    //         // var root = ExpeditionRuntimeUiFactory.EnsureWindowRoot(rectTransform, "m_goRuntimeRoot", new Color(0.07f, 0.1f, 0.09f, 0.94f));
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textTitle", new Vector2(0f, 260f), new Vector2(620f, 80f), 40, TextAnchor.MiddleCenter);
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textSummary", new Vector2(0f, 20f), new Vector2(620f, 360f), 26, TextAnchor.UpperLeft);
    //         // ExpeditionRuntimeUiFactory.EnsureButton(root, "m_btnConfirm", new Vector2(0f, -310f), new Vector2(360f, 70f), "返回入口");

    //         _textTitle = FindChildComponent<Text>("m_goRuntimeRoot/m_textTitle");
    //         _textSummary = FindChildComponent<Text>("m_goRuntimeRoot/m_textSummary");
    //         _btnConfirm = FindChildComponent<Button>("m_goRuntimeRoot/m_btnConfirm");
    //     }

    //     protected override void OnCreate()
    //     {
    //         _btnConfirm?.onClick.AddListener(OnConfirmClicked);
    //     }

    //     protected override void OnRefresh()
    //     {
    //         var summary = ExpeditionFlowController.Instance.GetDisplayableResult();
    //         if (summary == null)
    //         {
    //             _textTitle.text = "远征结果";
    //             _textSummary.text = "当前没有可展示的远征结果。";
    //             return;
    //         }

    //         _textTitle.text = summary.IsVictory ? "远征完成" : "远征失败";
    //         _textSummary.text = summary.ToDisplayText();
    //     }

    //     protected override void OnDestroy()
    //     {
    //         if (_btnConfirm != null)
    //         {
    //             _btnConfirm.onClick.RemoveListener(OnConfirmClicked);
    //         }
    //     }

    //     private void OnConfirmClicked()
    //     {
    //         ExpeditionFlowController.Instance.AcknowledgeSettlement();
    //     }
    // }
}
