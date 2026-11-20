using System.Collections.Generic;
using Launcher.Rendering;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// 本组用例只验证相机堆栈的绑定拓扑，不证明画面。
// 使用测试运行器提供的隔离未命名场景，不新建/保存场景，也不改动项目资产。
public sealed class PersistentUICameraStackEditModeTests
{
    private readonly List<GameObject> _objects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        Assert.IsInstanceOf<UniversalRenderPipelineAsset>(GraphicsSettings.currentRenderPipeline,
            "必须先完成本项目 URP 管线配置；不得忽略或将零用例视为通过。");
        Assert.IsTrue(string.IsNullOrEmpty(SceneManager.GetActiveScene().path),
            "预期测试运行器提供隔离的未命名场景；否则拒绝在项目场景内创建测试对象。");
    }

    [TearDown]
    public void TearDown()
    {
        for (var i = _objects.Count - 1; i >= 0; --i)
            if (_objects[i] != null)
                Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [Test]
    public void InitialBindingTouchesOnlyExplicitUiAndSelectedMain()
    {
        var main = NewCamera("main", true);
        var unrelated = NewCamera("unrelated", false);
        var ui = NewCamera("ui", false);
        var binder = ui.gameObject.AddComponent<PersistentUICameraStack>();
        binder.RefreshBinding();
        var data = main.GetUniversalAdditionalCameraData();
        Assert.AreEqual(CameraRenderType.Base, data.renderType);
        CollectionAssert.AreEqual(new[] { ui }, data.cameraStack);
        Assert.AreEqual(CameraRenderType.Overlay, ui.GetUniversalAdditionalCameraData().renderType);
        Assert.IsFalse(unrelated.TryGetComponent<UniversalAdditionalCameraData>(out _),
            "不能为无关相机增加 AdditionalCameraData。");
    }

    [Test]
    public void RepeatedBindingDeduplicatesOnlyOwnCameraAndPreservesOverlayOrder()
    {
        var main = NewCamera("main", true);
        var ui = NewCamera("ui", false);
        var first = NewCamera("first", false);
        var last = NewCamera("last", false);
        first.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
        last.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
        var stack = main.GetUniversalAdditionalCameraData().cameraStack;
        stack.AddRange(new[] { first, ui, last, ui });
        var binder = ui.gameObject.AddComponent<PersistentUICameraStack>();
        for (var i = 0; i < 10; ++i)
            binder.RefreshBinding();
        CollectionAssert.AreEqual(new[] { first, ui, last }, stack);
        binder.enabled = false;
        binder.RefreshBinding();
        CollectionAssert.AreEqual(new[] { first, last }, stack);
    }

    [Test]
    public void ReplacementMainTakesOverAndPreviousStackIsCleaned()
    {
        var oldMain = NewCamera("old-main", true);
        var ui = NewCamera("ui", false);
        var binder = ui.gameObject.AddComponent<PersistentUICameraStack>();
        binder.RefreshBinding();
        var oldStack = oldMain.GetUniversalAdditionalCameraData().cameraStack;
        CollectionAssert.AreEqual(new[] { ui }, oldStack);

        // 旧主相机失去 MainCamera 资格（等价于旧场景主相机被卸载/禁用），应转移绑定并清理旧列表。
        oldMain.tag = "Untagged";
        var newMain = NewCamera("new-main", true);
        binder.RefreshBinding();
        CollectionAssert.DoesNotContain(oldStack, ui);
        CollectionAssert.AreEqual(new[] { ui }, newMain.GetUniversalAdditionalCameraData().cameraStack);
    }

    [Test]
    public void MainTurnedOverlayIsNotForcedBackToBaseAndOwnEntryIsRemoved()
    {
        var main = NewCamera("main", true);
        var ui = NewCamera("ui", false);
        var binder = ui.gameObject.AddComponent<PersistentUICameraStack>();
        binder.RefreshBinding();
        var data = main.GetUniversalAdditionalCameraData();
        var oldStack = data.cameraStack;
        data.renderType = CameraRenderType.Overlay;
        binder.RefreshBinding();
        CollectionAssert.DoesNotContain(oldStack, ui);
        Assert.AreEqual(CameraRenderType.Overlay, data.renderType);
    }

    private Camera NewCamera(string name, bool main)
    {
        var go = new GameObject(name);
        _objects.Add(go);
        var camera = go.AddComponent<Camera>();
        camera.targetDisplay = 7; // 与本项目 display 0 的已打开场景相机隔离；不实际渲染。
        if (main)
            go.tag = "MainCamera";
        return camera;
    }
}
