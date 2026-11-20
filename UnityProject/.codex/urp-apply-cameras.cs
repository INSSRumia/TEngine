using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Reviewed narrow adapter for the approved scenes/UIRoot camera adaptation.
// Preview is read-only. Apply adds official UniversalAdditionalCameraData plus the reviewed stack helper.
public static class UrpCameraAdapter
{
    const string MainScene = "Assets/Scenes/main.unity";
    const string TestScene = "Assets/AssetRaw/Scenes/Test.unity";
    const string UiRoot = "Assets/TEngine/Settings/Prefab/UIRoot.prefab";
    const string HelperType = "Launcher.Rendering.PersistentUICameraStack";

    static Type Helper() => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(HelperType, false)).FirstOrDefault(t => t != null)
        ?? throw new InvalidOperationException("Missing reviewed helper type " + HelperType);

    static JObject Describe(Camera camera) => new JObject {
        ["scene"] = camera.gameObject.scene.path, ["root"] = camera.transform.root.name,
        ["path"] = camera.name, ["tag"] = camera.gameObject.tag, ["enabled"] = camera.enabled,
        ["depth"] = camera.depth, ["cullingMask"] = camera.cullingMask, ["clearFlags"] = camera.clearFlags.ToString(),
        ["orthographic"] = camera.orthographic, ["hdr"] = camera.allowHDR, ["msaa"] = camera.allowMSAA,
        ["hasData"] = camera.TryGetComponent<UniversalAdditionalCameraData>(out var data),
        ["renderType"] = camera.TryGetComponent<UniversalAdditionalCameraData>(out var d) ? d.renderType.ToString() : null,
        // 只有 Base 相机能安全读取 cameraStack；Overlay/无数据时官方 getter 返回 null。
        ["stack"] = camera.TryGetComponent<UniversalAdditionalCameraData>(out var s) && s.renderType == CameraRenderType.Base && s.cameraStack != null ? s.cameraStack.Count : -1,
        ["helper"] = camera.GetComponent(Helper()) != null
    };

    public static object PreviewCameras()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying)
            throw new InvalidOperationException("Editor is not idle");
        var scenes = Enumerable.Range(0, SceneManager.sceneCount).Select(i => SceneManager.GetSceneAt(i)).ToArray();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UiRoot);
        var prefabCameras = new JArray();
        if (prefab != null) foreach (var camera in prefab.GetComponentsInChildren<Camera>(true)) prefabCameras.Add(Describe(camera));
        return new JObject {
            ["pipeline"] = AssetDatabase.GetAssetPath(GraphicsSettings.currentRenderPipeline),
            ["openScenes"] = new JArray(scenes.Select(s => new JObject { ["path"] = s.path, ["dirty"] = s.isDirty })),
            ["openCameras"] = new JArray(UnityEngine.Object.FindObjectsByType<Camera>().Where(c => c.gameObject.scene.isLoaded).Select(Describe)),
            ["prefabCameras"] = prefabCameras
        };
    }

    public static object ApplyCameras()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Editor is not idle");
        var open = Enumerable.Range(0, SceneManager.sceneCount).Select(i => SceneManager.GetSceneAt(i)).ToArray();
        if (open.Length != 1 || open[0].path != MainScene || open[0].isDirty)
            throw new InvalidOperationException("Expected only clean main.unity open: " + new JArray(open.Select(s => (string)s.path)));
        if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
            throw new InvalidOperationException("URP is not the active pipeline");

        var report = new JObject();

        // 1. UIRoot prefab: explicit Overlay data + reviewed helper on the existing UI camera.
        var contents = PrefabUtility.LoadPrefabContents(UiRoot);
        try
        {
            var cameras = contents.GetComponentsInChildren<Camera>(true);
            if (cameras.Length != 1 || cameras[0].name != "UICamera")
                throw new InvalidOperationException("UIRoot camera set differs from reviewed baseline: " + string.Join(",", cameras.Select(c => c.name)));
            var ui = cameras[0];
            if (ui.cullingMask != 32 || ui.clearFlags != CameraClearFlags.Depth || !Mathf.Approximately(ui.depth, 2))
                throw new InvalidOperationException("UIRoot camera differs from reviewed baseline");
            var data = ui.TryGetComponent<UniversalAdditionalCameraData>(out var existing) ? existing : ui.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderType = CameraRenderType.Overlay;
            if (ui.GetComponent(Helper()) == null) ui.gameObject.AddComponent(Helper());
            report["prefab"] = new JObject { ["camera"] = Describe(ui), ["addedData"] = existing == null };
            PrefabUtility.SaveAsPrefabAsset(contents, UiRoot);
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }

        // 2. Both existing main cameras: explicit Base data, unchanged rendering parameters.
        report["scenes"] = new JArray(EnsureBase(MainScene, false));

        var test = SceneManager.GetSceneByPath(TestScene);
        var opened = !test.IsValid() || !test.isLoaded;
        if (opened) test = EditorSceneManager.OpenScene(TestScene, OpenSceneMode.Additive);
        try
        {
            ((JArray)report["scenes"]).Add(EnsureBase(TestScene, true));
            if (opened)
            {
                if (!EditorSceneManager.SaveScene(test)) throw new InvalidOperationException("Failed to save " + TestScene);
                EditorSceneManager.CloseScene(test, true);
            }
        }
        finally
        {
            if (opened && test.IsValid() && test.isLoaded) EditorSceneManager.CloseScene(test, true);
        }

        AssetDatabase.SaveAssets();
        report["after"] = (JToken)PreviewCameras();
        return report;

        JObject EnsureBase(string path, bool saveScene)
        {
            var scene = SceneManager.GetSceneByPath(path);
            if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("Scene not loaded: " + path);
            var cameras = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>(true)).ToArray();
            var mains = cameras.Where(c => c.CompareTag("MainCamera")).ToArray();
            if (mains.Length != 1) throw new InvalidOperationException(path + " main camera count is " + mains.Length);
            var main = mains[0];
            var added = !main.TryGetComponent<UniversalAdditionalCameraData>(out var data);
            if (added) data = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderType = CameraRenderType.Base;
            if (saveScene && !EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Failed to save " + path);
            return new JObject { ["scene"] = path, ["camera"] = Describe(main), ["addedData"] = added,
                ["saved"] = path == MainScene ? EditorSceneManager.SaveScene(scene) : saveScene };
        }
    }
}
