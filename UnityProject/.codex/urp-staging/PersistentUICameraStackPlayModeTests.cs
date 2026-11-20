using System.Collections;
using System.Collections.Generic;
using Launcher.Rendering;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class PersistentUICameraStackPlayModeTests
{
    private Scene _previous;
    private readonly List<Scene> _scenes = new List<Scene>();
    private readonly List<GameObject> _objects = new List<GameObject>();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Assert.IsInstanceOf<UniversalRenderPipelineAsset>(GraphicsSettings.currentRenderPipeline);
        _previous = SceneManager.GetActiveScene();
        NewScene();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        for (var i = _objects.Count - 1; i >= 0; --i)
            if (_objects[i] != null)
                Object.Destroy(_objects[i]);
        _objects.Clear();
        yield return null;
        if (_previous.IsValid() && _previous.isLoaded)
            SceneManager.SetActiveScene(_previous);
        for (var i = _scenes.Count - 1; i >= 0; --i)
            if (_scenes[i].IsValid() && _scenes[i].isLoaded)
                yield return SceneManager.UnloadSceneAsync(_scenes[i]);
        _scenes.Clear();
    }

    [UnityTest]
    public IEnumerator InitialBindingEnableDisableAndDestructionAreRecoveredWithoutManualRefresh()
    {
        var main = NewCamera("main", true);
        var otherOverlay = NewCamera("other-overlay", false);
        otherOverlay.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
        var stack = main.GetUniversalAdditionalCameraData().cameraStack;
        stack.Add(otherOverlay);
        var ui = NewCamera("ui", false);
        var binder = ui.gameObject.AddComponent<PersistentUICameraStack>();
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay, ui }, stack);

        stack.Add(ui); // 外部误加的同一 UI 引用应消除，不影响其他 Overlay。
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay, ui }, stack);

        ui.enabled = false;
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay }, stack);
        ui.enabled = true;
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay, ui }, stack);

        binder.enabled = false; // 验证 OnDisable 立即清理，不依赖另一次 Refresh。
        CollectionAssert.AreEqual(new[] { otherOverlay }, stack);
        binder.enabled = true;
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay, ui }, stack);

        main.enabled = false;
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay }, stack);
        main.enabled = true;
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay, ui }, stack);

        ui.gameObject.SetActive(false);
        CollectionAssert.AreEqual(new[] { otherOverlay }, stack);
        ui.gameObject.SetActive(true);
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay, ui }, stack);

        Object.Destroy(main.gameObject);
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { otherOverlay }, stack, "销毁旧 Base 后仍清理其缓存列表。");
        var replacement = NewCamera("replacement", true);
        yield return TwoFrames();
        var newStack = replacement.GetUniversalAdditionalCameraData().cameraStack;
        CollectionAssert.AreEqual(new[] { ui }, newStack);
        Object.Destroy(ui.gameObject);
        yield return TwoFrames();
        Assert.AreEqual(0, newStack.Count, "不能遗留已销毁 UI 相机引用。");
    }

    [UnityTest]
    public IEnumerator PersistentUiRebindsAfterSceneUnloadAndFreshRootWorksAfterRestartSequence()
    {
        var oldScene = SceneManager.GetActiveScene();
        var oldMain = NewCamera("old-main", true);
        var ui = NewCamera("persistent-ui", false);
        var root = ui.gameObject;
        Object.DontDestroyOnLoad(root);
        root.AddComponent<PersistentUICameraStack>();
        yield return TwoFrames();
        var oldStack = oldMain.GetUniversalAdditionalCameraData().cameraStack;
        CollectionAssert.AreEqual(new[] { ui }, oldStack);

        NewScene();
        var nextMain = NewCamera("next-main", true);
        yield return TwoFrames();
        var nextStack = nextMain.GetUniversalAdditionalCameraData().cameraStack;
        CollectionAssert.AreEqual(new[] { ui }, nextStack);
        Assert.AreEqual(0, oldStack.Count);
        yield return SceneManager.UnloadSceneAsync(oldScene);
        yield return TwoFrames();
        Assert.IsTrue(root != null, "模拟 Single 卸载旧场景，UI 根必须仍存活。");
        CollectionAssert.AreEqual(new[] { ui }, nextStack);

        // 不调用热更 SingletonSystem：复现其禁用主相机、释放旧 UI、重建入口的顺序。
        nextMain.gameObject.SetActive(false);
        Object.Destroy(root);
        yield return TwoFrames();
        Assert.AreEqual(0, nextStack.Count);
        NewScene();
        var restartedMain = NewCamera("restarted-main", true);
        var restartedUi = NewCamera("restarted-ui", false);
        Object.DontDestroyOnLoad(restartedUi.gameObject);
        restartedUi.gameObject.AddComponent<PersistentUICameraStack>();
        yield return TwoFrames();
        CollectionAssert.AreEqual(new[] { restartedUi },
            restartedMain.GetUniversalAdditionalCameraData().cameraStack);
        Assert.AreEqual(0, nextStack.Count, "没有静态服务把新 UI 重新接回旧 Base。");
    }

    private void NewScene()
    {
        var scene = SceneManager.CreateScene("UICameraStack-" + System.Guid.NewGuid().ToString("N"));
        _scenes.Add(scene);
        SceneManager.SetActiveScene(scene);
    }

    private Camera NewCamera(string name, bool main)
    {
        var go = new GameObject(name);
        _objects.Add(go);
        var camera = go.AddComponent<Camera>();
        camera.targetDisplay = 7; // 合成拓扑测试，不向用户 display 0 输出，也不修改其主相机。
        if (main)
            go.tag = "MainCamera";
        return camera;
    }

    private static IEnumerator TwoFrames()
    {
        yield return null;
        yield return null;
    }
}
