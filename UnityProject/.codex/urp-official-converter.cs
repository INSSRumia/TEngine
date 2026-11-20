using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Reviewed thin adapter around the installed official converters, not a replacement mapper.
// Lives outside Assets. Preview does not save or import. Execution requires explicit approved entry.
public static class UrpOfficialConverter
{
    const string Namespace = "UnityEditor.Rendering.Universal.";
    static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name, false)).FirstOrDefault(t => t != null)
        ?? throw new InvalidOperationException("Missing installed converter type: " + name);
    static object Property(object obj, string name) => obj.GetType().GetProperty(name,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
    static MethodInfo Method(object obj, string name) => obj.GetType().GetMethod(name,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    static object NewConverter(string name) => Activator.CreateInstance(Find(Namespace + name), true);
    static List<object> Flatten(IEnumerable items)
    {
        var output = new List<object>();
        foreach (object item in items)
        {
            var children = Property(item, "children") as IEnumerable;
            if (children != null) output.AddRange(Flatten(children));
            else output.Add(item);
        }
        return output;
    }
    static Task<List<object>> Scan(object converter)
    {
        var completion = new TaskCompletionSource<List<object>>();
        Action<object> receive = items => {
            try { completion.TrySetResult(Flatten((IEnumerable)items)); }
            catch (Exception ex) { completion.TrySetException(ex); }
        };
        var method = Method(converter, "Scan") ?? throw new InvalidOperationException("No Scan method");
        var delegateType = method.GetParameters()[0].ParameterType;
        var argument = Expression.Parameter(delegateType.GenericTypeArguments[0], "items");
        var callback = Expression.Lambda(delegateType,
            Expression.Invoke(Expression.Constant(receive), Expression.Convert(argument, typeof(object))), argument).Compile();
        method.Invoke(converter, new object[] { callback });
        return completion.Task;
    }
    static JArray DirtyAssets() => new JArray(Resources.FindObjectsOfTypeAll<UnityEngine.Object>()
        .Where(o => o != null && EditorUtility.IsPersistent(o) && EditorUtility.IsDirty(o))
        .Select(o => new { obj = o, path = AssetDatabase.GetAssetPath(o) })
        .Where(x => !string.IsNullOrEmpty(x.path) && !x.path.StartsWith("Library/", StringComparison.Ordinal))
        .Select(x => new JObject { ["path"] = x.path, ["name"] = x.obj.name, ["type"] = x.obj.GetType().FullName,
            ["nativeAsset"] = AssetDatabase.IsNativeAsset(x.obj), ["mainAsset"] = AssetDatabase.IsMainAsset(x.obj),
            ["importerDirty"] = AssetImporter.GetAtPath(x.path) != null && EditorUtility.IsDirty(AssetImporter.GetAtPath(x.path)) }));
    static JArray Scenes() => new JArray(Enumerable.Range(0, SceneManager.sceneCount).Select(i => {
        var scene = SceneManager.GetSceneAt(i);
        return new JObject { ["path"] = scene.path, ["isDirty"] = scene.isDirty, ["isLoaded"] = scene.isLoaded,
            ["active"] = scene == SceneManager.GetActiveScene() };
    }));
    static void RequireIdleClean()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Editor is not idle edit mode");
        if (Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Unsaved scene exists; refusing save/convert");
        var dirty = DirtyAssets();
        var unsafeDirty = new JArray(dirty.Where(d => {
            string path = (string)d["path"];
            if (path == "ProjectSettings/QualitySettings.asset" || path == "ProjectSettings/GraphicsSettings.asset") return false; // Approved settings; official Scan/Convert can retain native settings dirty flags even after persistence.
            bool importedCache = !(bool)d["nativeAsset"] && !(bool)d["importerDirty"]
                && ((string)d["type"] == "UnityEngine.Shader" || ((string)d["type"] == "UnityEngine.Texture2D" && path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)));
            return !importedCache;
        }));
        if (unsafeDirty.Count != 0) throw new InvalidOperationException("Unsaved user assets/importers exist; refusing conversion: " + unsafeDirty);
    }
    static JObject Describe(object item) => new JObject {
        ["name"] = (string)Property(item, "name"), ["enabled"] = (bool?)Property(item, "isEnabled"),
        ["info"] = (string)Property(item, "info"), ["assetPath"] = (string)Property(item, "assetPath"),
        ["globalObjectId"] = Property(item, "GlobalObjectId")?.ToString(),
        ["qualityIndex"] = Property(item, "qualityLevelIndex") == null ? JValue.CreateNull() : JToken.FromObject(Property(item, "qualityLevelIndex"))
    };
    static string OutputFolder()
    {
        var type = Find(Namespace + "UniversalProjectSettings");
        return "Assets/" + (string)type.GetProperty("projectSettingsFolderPath", BindingFlags.Static | BindingFlags.Public).GetValue(null);
    }
    public static async Task<object> PreviewSettings()
    {
        var tier = UnityEditor.Rendering.EditorGraphicsSettings.GetTierSettings(
            BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), GraphicsTier.Tier3);
        var converter = NewConverter("BuiltInToURP3DRenderSettingsConverter");
        var items = await Scan(converter);
        return new JObject {
            ["unity"] = Application.unityVersion, ["pipeline"] = AssetDatabase.GetAssetPath(GraphicsSettings.currentRenderPipeline),
            ["buildTarget"] = EditorUserBuildSettings.activeBuildTarget.ToString(), ["tierRenderingPath"] = tier.renderingPath.ToString(),
            ["tierHdr"] = tier.hdr, ["outputFolder"] = OutputFolder(), ["qualityIndex"] = QualitySettings.GetQualityLevel(),
            ["scenes"] = Scenes(), ["dirtyAssets"] = DirtyAssets(), ["items"] = new JArray(items.Select(Describe)),
            ["existingOutputAssets"] = new JArray(AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith(OutputFolder() + "/", StringComparison.Ordinal)))
        };
    }
    public static async Task<object> PreviewMaterials()
    {
        RequireIdleClean();
        var result = new JObject { ["scenes"] = Scenes(), ["dirtyAssets"] = DirtyAssets() };
        foreach (string type in new[] { "BuiltInToURP3DMaterialUpgrader", "BuiltInToURP3DReadonlyMaterialConverter" })
        {
            var converter = NewConverter(type);
            if (!(bool)Property(converter, "isEnabled")) throw new InvalidOperationException(type + " is disabled");
            result[type] = new JArray((await Scan(converter)).Select(Describe));
        }
        return result;
    }
    static readonly string[] ApprovedPrefabs = {
        "Assets/AssetRaw/Actor/Cube.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Battlefield/普通森林_1.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Equipment/bow.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Equipment/shield.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Equipment/spear.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Equipment/sword.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Marbles/Marble.prefab",
        "Assets/AssetRaw/Actor/Prefabs/Projectiles/arrow.prefab"
    };
    static object CubeItem()
    {
        var cube = AssetDatabase.LoadAssetAtPath<GameObject>(ApprovedPrefabs[0]);
        if (cube == null || cube.GetComponent<MeshRenderer>()?.sharedMaterial?.name != "Default-Material")
            throw new InvalidOperationException("Cube material differs from approved baseline");
        var itemType = Find("UnityEditor.Rendering.Converter.RenderPipelineConverterAssetItem");
        return Activator.CreateInstance(itemType, new object[] { GlobalObjectId.GetGlobalObjectIdSlow(cube).ToString() });
    }
    public static object PreviewCubeOfficialMapping()
    {
        RequireIdleClean();
        var converter = NewConverter("BuiltInToURP3DReadonlyMaterialConverter");
        var mappings = Property(converter, "mappings");
        var cube = AssetDatabase.LoadAssetAtPath<GameObject>(ApprovedPrefabs[0]);
        var material = cube.GetComponent<MeshRenderer>().sharedMaterial;
        object[] args = { material, null };
        var ok = (bool)Method(mappings, "TryGetMappingMaterial").Invoke(mappings, args);
        var mapped = args[1] as Material;
        var builtinObjects = ((IEnumerable)Method(mappings, "GetBuiltInMaterials").Invoke(mappings, null)).Cast<Material>();
        var shaderConverter = NewConverter("BuiltInToURP3DMaterialUpgrader");
        var upgraders = Property(shaderConverter, "upgraders");
        var getUpgrader = Find("UnityEditor.Rendering.MaterialUpgrader").GetMethod("GetUpgrader", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var tmp = new JArray(new[] { "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat", "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat" }.Select(p => {
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            var upgrader = getUpgrader.Invoke(null, new object[] { upgraders, m });
            return new JObject { ["path"] = p, ["shader"] = m.shader.name, ["hasOfficialUpgrader"] = upgrader != null };
        }));
        return new JObject { ["cubeOfficialItem"] = Describe(CubeItem()), ["originalMaterial"] = material.name,
            ["originalId"] = GlobalObjectId.GetGlobalObjectIdSlow(material).ToString(), ["mappingSuccess"] = ok,
            ["mappedMaterial"] = AssetDatabase.GetAssetPath(mapped), ["mappedShader"] = mapped?.shader?.name,
            ["scanSourceDefaultMaterials"] = new JArray(builtinObjects.Where(m => m.name.StartsWith("Default-Material")).Select(m => new JObject { ["name"] = m.name, ["path"] = AssetDatabase.GetAssetPath(m), ["id"] = GlobalObjectId.GetGlobalObjectIdSlow(m).ToString() })),
            ["tmpCandidates"] = tmp };
    }
    public static async Task<object> ConvertApprovedMaterialReferences()
    {
        RequireIdleClean();
        var converter = NewConverter("BuiltInToURP3DReadonlyMaterialConverter");
        if (!(bool)Property(converter, "isEnabled")) throw new InvalidOperationException("Official material reference converter disabled");
        var items = (await Scan(converter)).GroupBy(i => (string)Property(i, "GlobalObjectId")).Select(g => g.First()).ToList();
        if (items.Count != 10 || items.Any(i => !ApprovedPrefabs.Skip(1).Contains((string)Property(i, "assetPath"))) ||
            items.Select(i => (string)Property(i, "assetPath")).Distinct().Count() != 7)
            throw new InvalidOperationException("Official scan differs from reviewed ten Sprite objects: " + new JArray(items.Select(Describe)));
        items.Add(CubeItem()); // Approved Cube missed by official search index; exact official item and conversion, no custom mapping.
        RequireIdleClean();
        var results = new JArray();
        Method(converter, "BeforeConvert").Invoke(converter, null);
        try {
            foreach (var item in items) {
                object[] args = { item, null };
                string status = Method(converter, "Convert").Invoke(converter, args).ToString();
                results.Add(new JObject { ["item"] = Describe(item), ["status"] = status, ["message"] = (string)args[1] });
                if (status != "Success") throw new InvalidOperationException("Partial material conversion; inspect before retry: " + results);
            }
        } finally { Method(converter, "AfterConvert").Invoke(converter, null); }
        var unexpected = DirtyAssets().Where(d => (bool)d["nativeAsset"] &&
            !ApprovedPrefabs.Contains((string)d["path"]) && (string)d["path"] != "ProjectSettings/GraphicsSettings.asset" && (string)d["path"] != "ProjectSettings/QualitySettings.asset").ToArray();
        if (unexpected.Length != 0) throw new InvalidOperationException("Unexpected dirty native objects before save: " + new JArray(unexpected));
        AssetDatabase.SaveAssets(); // Same official window persistence, explicitly authorized after dirty preflight.
        return new JObject { ["results"] = results, ["scenes"] = Scenes(), ["dirtyAssets"] = DirtyAssets() };
    }
    public static async Task<object> ConvertApprovedSettings()
    {
        RequireIdleClean();
        if (GraphicsSettings.defaultRenderPipeline != null || GraphicsSettings.currentRenderPipeline != null)
            throw new InvalidOperationException("Expected pristine Built-in pipeline state, no blind replay");
        if (OutputFolder() != "Assets/URPDefaultResources") throw new InvalidOperationException("Output folder changed");
        if (AssetDatabase.GetAllAssetPaths().Any(p => p.StartsWith(OutputFolder() + "/", StringComparison.Ordinal)))
            throw new InvalidOperationException("Output assets already exist; refusing reuse/overwrite");
        var expected = new[] { "Default Render Pipeline", "Very Low", "Low", "Medium", "High", "Very High", "Ultra" };
        var converter = NewConverter("BuiltInToURP3DRenderSettingsConverter");
        var items = await Scan(converter);
        if (!items.Select(i => (string)Property(i, "name")).OrderBy(n => n).SequenceEqual(expected.OrderBy(n => n))
            || items.Any(i => !(bool)Property(i, "isEnabled")))
            throw new InvalidOperationException("Official settings scan changed or contains disabled items");
        var results = new JArray();
        // Calls precisely the same Scan / Convert implementation used by the official window.
        // No global batch entry point or custom mapping; no SaveAssets of unrelated dirty assets.
        foreach (var item in items)
        {
            var args = new object[] { item, null };
            string status = Method(converter, "Convert").Invoke(converter, args).ToString();
            results.Add(new JObject { ["item"] = (string)Property(item, "name"), ["status"] = status, ["message"] = args[1]?.ToString() });
            if (status != "Success" && status != "Warning")
                throw new InvalidOperationException("Official conversion failed; inspect partial writes, never replay: " + results);
        }
        // Official converter saves each generated asset itself. Save only the two approved settings objects.
        foreach (var path in new[] { "ProjectSettings/GraphicsSettings.asset", "ProjectSettings/QualitySettings.asset" })
        {
            var settings = AssetDatabase.LoadAllAssetsAtPath(path);
            if (settings.Length == 0) throw new InvalidOperationException("Cannot resolve approved settings for saving: " + path);
            foreach (var asset in settings) AssetDatabase.SaveAssetIfDirty(asset);
        }
        return new JObject { ["results"] = results, ["qualityIndex"] = QualitySettings.GetQualityLevel(),
            ["defaultPipeline"] = AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),
            ["currentPipeline"] = AssetDatabase.GetAssetPath(GraphicsSettings.currentRenderPipeline),
            ["dirtyAssetsAfter"] = DirtyAssets(), ["scenesAfter"] = Scenes() };
    }
}
