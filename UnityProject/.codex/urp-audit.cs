using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Read-only audit. No asset/scene/settings changes and no scene loading/saving.
public static class UrpReadOnlyAudit
{
    static string PathOf(UnityEngine.Object obj) => obj == null ? null : AssetDatabase.GetAssetPath(obj);
    static JObject CameraInfo(Camera camera) => new JObject
    {
        ["name"] = camera.name,
        ["scene"] = camera.gameObject.scene.path,
        ["clearFlags"] = camera.clearFlags.ToString(),
        ["depth"] = camera.depth,
        ["cullingMask"] = camera.cullingMask,
        ["orthographic"] = camera.orthographic,
        ["enabled"] = camera.enabled,
        ["urpData"] = camera.TryGetComponent<UniversalAdditionalCameraData>(out var data) ? data.renderType.ToString() : "none"
    };
    public static object Run()
    {
        var materials = new JArray();
        foreach (var path in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        foreach (var material in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>())
            materials.Add(new JObject {
                ["path"] = path, ["name"] = material.name,
                ["shader"] = material.shader == null ? null : material.shader.name,
                ["shaderPath"] = PathOf(material.shader),
                ["supportedInCurrentPipeline"] = material.shader != null && material.shader.isSupported,
                ["pipelineTag"] = material.GetTag("RenderPipeline", false, "")
            });
        var prefabs = new JArray();
        var renderers = new JArray();
        foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                renderers.Add(new JObject {
                    ["path"] = path, ["object"] = renderer.name, ["type"] = renderer.GetType().Name,
                    ["materials"] = new JArray(renderer.sharedMaterials.Select(m => new JObject {
                        ["name"] = m == null ? null : m.name,
                        ["path"] = PathOf(m),
                        ["shader"] = m == null || m.shader == null ? null : m.shader.name
                    }))
                });
            var cameras = prefab.GetComponentsInChildren<Camera>(true);
            if (cameras.Length > 0) prefabs.Add(new JObject {
                ["path"] = path, ["cameras"] = new JArray(cameras.Select(CameraInfo))
            });
        }
        return new JObject {
            ["unity"] = Application.unityVersion,
            ["defaultPipeline"] = PathOf(GraphicsSettings.defaultRenderPipeline),
            ["currentPipeline"] = PathOf(GraphicsSettings.currentRenderPipeline),
            ["qualityNames"] = JArray.FromObject(QualitySettings.names),
            ["qualityIndex"] = QualitySettings.GetQualityLevel(),
            ["materials"] = materials,
            ["prefabRenderers"] = renderers,
            ["prefabsWithCameras"] = prefabs,
            ["loadedSceneCameras"] = new JArray(Resources.FindObjectsOfTypeAll<Camera>()
                .Where(c => c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded).Select(CameraInfo))
        };
    }
}
