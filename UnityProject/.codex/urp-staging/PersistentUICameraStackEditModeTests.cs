using System.Collections.Generic;
using Launcher.Rendering;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class PersistentUICameraStackEditModeTests
{
    private Scene _previous;
    private readonly List<Scene> _scenes = new List<Scene>();
    private readonly List<GameObject> _objects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        Assert.IsInstanceOf<UniversalRenderPipelineAsset>(GraphicsSettings.currentRenderPipeline,
            "必须先完成本项目 URP 管线配置；不得忽略或将零用例视为通过。");
        _previous = SceneManager.GetActiveScene();
        NewScene();
    }

    [TearDown]
    public void TearDown()
    {
        for (var i = _objects.Count - 1; i >= 0; --i)
            if (_objects[i] != null)
                Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
        if (_previous.IsValid() && _previous.isLoaded)
            SceneManager.SetActiveScene(_previous);
        for (var i = _scenes.Count - 1; i >= 0; --i)
            if (_scenes[i].isLoaded)
                EditorSceneManager.CloseScene(_scenes[i], true);
        _scenes.Clear();
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
    public void ActiveSceneChangeTransfersBindingAndCleansOldBase()
    {
        var oldMain = NewCamera("old-main", true);
        var ui = NewCamera("ui", false);
        var binder = ui.gameObject.AddComponent<PersistentUICameraStack>();
        binder.RefreshBinding();
        var oldStack = oldMain.GetUniversalAdditionalCameraData().cameraStack;
        NewScene();
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

    private void NewScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        _scenes.Add(scene);
        SceneManager.SetActiveScene(scene);
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
