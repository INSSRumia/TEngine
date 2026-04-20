using System.Linq;
using GameLogic.Gameplay.Expedition;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    // [Window(UILayer.UI, location: ExpeditionConstants.SharedUiPrefabLocation, fullScreen: true)]
    // class ExpeditionMainUI : UIWindow
    // {
    //     private Text _textTitle;
    //     private Text _textMarbleSummary;
    //     private Text _textLastResult;
    //     private Button _btnStartExpedition;
    //     private Button _btnCombatDebug;

    //     protected override void ScriptGenerator()
    //     {
    //         // var root = ExpeditionRuntimeUiFactory.EnsureWindowRoot(rectTransform, "m_goRuntimeRoot", new UnityEngine.Color(0.05f, 0.08f, 0.12f, 0.92f));
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textTitle", new UnityEngine.Vector2(0f, 280f), new UnityEngine.Vector2(620f, 80f), 40, TextAnchor.MiddleCenter);
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textMarbleSummary", new UnityEngine.Vector2(0f, 70f), new UnityEngine.Vector2(620f, 260f), 26, TextAnchor.UpperLeft);
    //         // ExpeditionRuntimeUiFactory.EnsureText(root, "m_textLastResult", new UnityEngine.Vector2(0f, -120f), new UnityEngine.Vector2(620f, 180f), 24, TextAnchor.UpperLeft);
    //         // ExpeditionRuntimeUiFactory.EnsureButton(root, "m_btnStartExpedition", new UnityEngine.Vector2(0f, -250f), new UnityEngine.Vector2(360f, 70f), "开始最小远征");
    //         // ExpeditionRuntimeUiFactory.EnsureButton(root, "m_btnCombatDebug", new UnityEngine.Vector2(0f, -340f), new UnityEngine.Vector2(360f, 70f), "Combat 调试后门");

    //         _textTitle = FindChildComponent<Text>("m_goRuntimeRoot/m_textTitle");
    //         _textMarbleSummary = FindChildComponent<Text>("m_goRuntimeRoot/m_textMarbleSummary");
    //         _textLastResult = FindChildComponent<Text>("m_goRuntimeRoot/m_textLastResult");
    //         _btnStartExpedition = FindChildComponent<Button>("m_goRuntimeRoot/m_btnStartExpedition");
    //         _btnCombatDebug = FindChildComponent<Button>("m_goRuntimeRoot/m_btnCombatDebug");
    //     }

    //     protected override void OnCreate()
    //     {
    //         _btnStartExpedition?.onClick.AddListener(OnStartExpeditionClicked);
    //         _btnCombatDebug?.onClick.AddListener(OnCombatDebugClicked);
    //     }

    //     protected override void OnRefresh()
    //     {
    //         var controller = ExpeditionFlowController.Instance;
    //         var persistentData = controller.PersistentData;

    //         _textTitle.text = "远征入口 / 出征准备";
    //         _textMarbleSummary.text = $"当前晶体: {persistentData.Crystal}\n\n可出征 Marble:\n{BuildMarbleSummary(persistentData)}";

    //         var resultSummary = controller.GetDisplayableResult();
    //         _textLastResult.text = resultSummary == null
    //             ? "上次远征结果: 暂无记录"
    //             : $"上次远征结果:\n{resultSummary.ToDisplayText()}";
    //     }

    //     protected override void OnDestroy()
    //     {
    //         if (_btnStartExpedition != null)
    //         {
    //             _btnStartExpedition.onClick.RemoveListener(OnStartExpeditionClicked);
    //         }

    //         if (_btnCombatDebug != null)
    //         {
    //             _btnCombatDebug.onClick.RemoveListener(OnCombatDebugClicked);
    //         }
    //     }

    //     private static string BuildMarbleSummary(ExpeditionPersistentDataStore persistentData)
    //     {
    //         return string.Join("\n", persistentData.Marbles.Select(marble =>
    //             $"- {marble.DisplayName} ({marble.ConfigId}) HP {marble.CurrentHp}/{marble.MaxHp} EXP {marble.Exp} {(marble.IsDead ? "[不可用]" : "[可出征]")}"));
    //     }

    //     private void OnStartExpeditionClicked()
    //     {
    //         ExpeditionFlowController.Instance.StartMinimalExpedition();
    //     }

    //     private void OnCombatDebugClicked()
    //     {
    //         ExpeditionFlowController.Instance.StartCombatDebug();
    //     }
    // }
}
