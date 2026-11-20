using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// PlayMode 观测与点击辅助；只读取状态，除 Click 之外不改运行时对象。
public static class UrpPlayProbe
{
    static JObject CameraInfo(Camera camera) => new JObject {
        ["name"] = camera.name, ["scene"] = camera.gameObject.scene.name, ["enabled"] = camera.enabled,
        ["depth"] = camera.depth, ["display"] = camera.targetDisplay, ["mask"] = camera.cullingMask,
        ["clear"] = camera.clearFlags.ToString(), ["type"] = camera.cameraType.ToString(),
        ["hasData"] = camera.TryGetComponent<UniversalAdditionalCameraData>(out var d),
        ["renderType"] = camera.TryGetComponent<UniversalAdditionalCameraData>(out var t) ? t.renderType.ToString() : null
    };

    public static object Probe()
    {
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
        var bases = cameras.Where(c => c.TryGetComponent<UniversalAdditionalCameraData>(out var d) && d.renderType == CameraRenderType.Base).ToArray();
        var stacks = bases.Select(b => new JObject {
            ["base"] = b.name,
            ["scene"] = b.gameObject.scene.name,
            ["stack"] = new JArray(b.GetUniversalAdditionalCameraData().cameraStack.Where(c => c != null).Select(c => (string)c.name))
        });
        return new JObject {
            ["pipeline"] = GraphicsSettings.currentRenderPipeline == null ? null : GraphicsSettings.currentRenderPipeline.GetType().Name,
            ["activeScene"] = SceneManager.GetActiveScene().name,
            ["scenes"] = new JArray(Enumerable.Range(0, SceneManager.sceneCount).Select(i => (string)SceneManager.GetSceneAt(i).name)),
            ["cameras"] = new JArray(cameras.Select(CameraInfo)),
            ["baseStacks"] = new JArray(stacks),
            ["canvases"] = new JArray(canvases.Select(c => new JObject {
                ["name"] = c.name, ["renderMode"] = c.renderMode.ToString(),
                ["worldCamera"] = c.worldCamera == null ? null : c.worldCamera.name,
                ["sortingOrder"] = c.sortingOrder, ["enabled"] = c.enabled, ["active"] = c.gameObject.activeInHierarchy,
                ["raycaster"] = c.GetComponent<GraphicRaycaster>() != null
            })),
            ["eventSystem"] = EventSystem.current == null ? null : EventSystem.current.name,
            ["sprites"] = new JArray(UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include)
                .Select(s => new JObject { ["name"] = s.name, ["sprite"] = s.sprite == null ? null : s.sprite.name,
                    ["material"] = s.sharedMaterial == null ? null : s.sharedMaterial.name,
                    ["shader"] = s.sharedMaterial == null || s.sharedMaterial.shader == null ? null : s.sharedMaterial.shader.name }))
        };
    }

    static readonly System.Collections.Generic.List<GameObject> Spawned = new System.Collections.Generic.List<GameObject>();

    // 运行期临时摆放已迁移材质的目标 Prefab，仅用于在真实 URP 下截图确认 Sprite/Lit 渲染，不写资源。
    public static object Showcase(string[] prefabPaths)
    {
        DespawnShowcase();
        var camera = Camera.main ?? UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include)
            .First(c => c.cameraType == CameraType.Game && c.gameObject.scene.name == SceneManager.GetActiveScene().name);
        var forward = camera.transform.forward;
        var right = camera.transform.right;
        var up = camera.transform.up;
        var center = camera.transform.position + forward * Mathf.Max(5f, camera.orthographicSize * 2f);
        var result = new JArray();
        var index = 0;
        foreach (var path in prefabPaths)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { result.Add(new JObject { ["path"] = path, ["error"] = "missing" }); continue; }
            var go = UnityEngine.Object.Instantiate(prefab);
            go.name = "Showcase_" + prefab.name;
            go.transform.position = center + right * ((index - 1) * 2.2f) + up * 0.5f;
            Spawned.Add(go);
            index++;
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            result.Add(new JObject { ["path"] = path, ["renderers"] = renderers.Length,
                ["bounds"] = renderers.Length == 0 ? null : new JArray(renderers[0].bounds.size.x, renderers[0].bounds.size.y),
                ["materials"] = new JArray(renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null)
                    .Select(m => (string)(m.name + " :: " + (m.shader == null ? "null" : m.shader.name)))) });
        }
        return result;
    }

    public static object DespawnShowcase()
    {
        foreach (var go in Spawned) if (go != null) UnityEngine.Object.Destroy(go);
        var count = Spawned.Count;
        Spawned.Clear();
        return new JObject { ["destroyed"] = count };
    }

    public static object SetUiVisible(bool visible)
    {
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include)
            .Where(c => c.renderMode != RenderMode.ScreenSpaceOverlay && c.transform.parent == null || c.name == "UICanvas")
            .Select(c => c.gameObject).Distinct().ToArray();
        foreach (var go in canvases) go.SetActive(visible);
        return new JObject { ["canvases"] = new JArray(canvases.Select(c => (string)c.name)), ["visible"] = visible };
    }

    public static object Buttons()
    {
        return new JArray(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include)
            .Where(b => b.gameObject.activeInHierarchy)
            .Select(b => new JObject {
                ["name"] = b.name, ["object"] = Hierarchy(b.transform),
                ["text"] = string.Join(" | ", b.GetComponentsInChildren<TMPro.TMP_Text>(true).Select(t => t.text)),
                ["interactable"] = b.interactable
            }));
    }

    // 按名称片段点击按钮，便于连续推进业务界面（仍然走真实 EventSystem 射线）。
    public static object ClickButton(string contains)
    {
        var button = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include)
            .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0);
        if (button == null) throw new InvalidOperationException("Button not found: " + contains);
        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        var point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        return new JObject { ["button"] = button.name, ["point"] = new JArray(point.x, point.y), ["click"] = (JToken)Click(point.x, point.y) };
    }

    // 用真实 EventSystem 射线命中再点击，验证 URP 下 UI 相机堆栈的交互链路。
    public static object ClickStart()
    {
        var button = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include)
            .FirstOrDefault(b => b.name == "Start" || b.name.Contains("Start"));
        if (button == null) throw new InvalidOperationException("Start button not found");
        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        var point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        return new JObject { ["button"] = button.name, ["point"] = new JArray(point.x, point.y), ["click"] = (JToken)Click(point.x, point.y) };
    }

    public static object Click(float x, float y)
    {
        if (EventSystem.current == null) throw new InvalidOperationException("No EventSystem");
        var data = new PointerEventData(EventSystem.current) { position = new Vector2(x, y), button = PointerEventData.InputButton.Left };
        var hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);
        if (hits.Count == 0) throw new InvalidOperationException("No UI hit at " + x + "," + y);
        data.pointerCurrentRaycast = hits[0];
        data.pointerPressRaycast = hits[0];
        var target = hits[0].gameObject;
        ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerClickHandler);
        return new JObject { ["hit"] = target.name, ["path"] = Hierarchy(target.transform),
            ["module"] = hits[0].module == null ? null : hits[0].module.GetType().Name };
    }

    static string Hierarchy(Transform t)
    {
        var parts = new System.Collections.Generic.List<string>();
        for (var current = t; current != null; current = current.parent) parts.Insert(0, current.name);
        return string.Join("/", parts);
    }
}

