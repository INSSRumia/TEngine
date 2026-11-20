# Unity 6000.6：Built-in → URP 迁移

- ID: 20261120-built-in-to-urp
- 状态: done（官方转换器完成设置与材质迁移，主包相机堆栈补齐；编译、测试、.NET 构建与运行截图均已留证）
- 负责人: AI / 用户确认与视觉验收

## 目标与验收

- 将实际活动管线迁移到已安装的 URP 17.6.0，保持启动、跨场景 UI、战斗 Sprite、字体和遮罩可用。
- [x] 确认实际管线、依赖、场景和 Shader / 材质基线。
- [x] 核对本地 URP / Core 官方转换器实现，而非根据旧版文档猜测 API。
- [ ] 经确认后，由官方 Rendering Settings 转换 Graphics 默认管线和六档 Quality，保留平台默认质量档。
- [ ] 由官方 Material Reference Converter 转换选中的内置材质引用；普通材质仅转换扫描确认的官方支持项。
- [ ] 按需补齐官方通用转换未覆盖的跨场景 UI Camera 合成，使用 URP 官方 Camera Stacking API。
- [ ] .NET / Unity 编译、相关自动测试、运行截图检查分别留证。未执行的视觉或设备验证不得视为通过。

## 工程事实与基线

- Unity `6000.6.10f1`；CLI `1.0.0-beta.3` / Pipeline `0.10.0-exp.1`。
- 最近只读查询：Editor ready、Play stopped；打开启动场景且 isDirty=false。执行写入前须重新查询，不能沿用此状态保证。
- Graphics 默认 / 当前管线均 null，六档 Quality override 均 null，实际是 Built-in。
- [manifest.json](../../UnityProject/Packages/manifest.json) 已有 URP 17.6.0，无需安装、升级或修改依赖。
- 已有 [URP Global Settings](../../UnityProject/Assets/UniversalRenderPipelineGlobalSettings.asset)，复用而非新建第二份；若官方初始化改变它，须检查差异。
- 构建场景为 [main.unity](../../UnityProject/Assets/Scenes/main.unity) 和 [Test.unity](../../UnityProject/Assets/AssetRaw/Scenes/Test.unity)。不更改场景列表、平台、PlayerSettings 或图形 API。
- [UIRoot.prefab](../../UnityProject/Assets/TEngine/Settings/Prefab/UIRoot.prefab) 的 UI Camera 为 Depth-only、Mask=32，没有 URP Camera Data；UIRoot 被持久化，启动流程随后 Single 加载 Test。
- 两场景 Main Camera 当前 Mask=全部。仅设置静态相机堆栈不能保证跨场景后持续合成 UI。
- Assets / 嵌入 Packages 的 31 个 Shader 相关源码均属 TMP 或 Sirenix；未发现游戏自定义 Surface / GrabPass Shader。
- 7 个 Prefab、10 个 SpriteRenderer 使用内置 Sprites/Default；另有 Cube 使用内置 Default-Material / Standard。只读内置资源应转换引用，不原地改写。
- 七个序列化 Material 对象均属 TMP；未确认可批量转换的独立 Standard 材质文件。导入器字体材质和 ShaderGraph 预览材质不算待转换游戏材质。
- 未发现游戏 PPv2 / OnRenderImage / Graphics.Blit 后处理链，不新增 Volume 或调色效果。

## 官方流程核对

- 官方菜单为 `Window > Rendering > Render Pipeline Converter`。
- 当前版本界面提供 `Source Pipeline` / `Target Pipeline`，操作按钮为 `Scan` → `Convert Assets`；不是照搬旧版的 Initialize / Convert All 教程。
- 拟选择 `Built-in` → `Universal Render Pipeline (Universal Renderer)`。这是通用渲染器，不因项目有 Sprite 就强制引入 2D Renderer / 2D 灯光。
- 官方 Rendering Settings 会创建 Graphics 默认资产及各 Quality 独立资产，并按旧 Quality 与活动平台 Tier3 映射灯光、阴影、MSAA、HDR、反射探针和 Renderer。恢复原当前质量档；仍需逐档复核。
- 官方 Material Shader Converter 升级支持的普通材质 Shader；不自动修复任意自定义 Shader。
- 官方 Material Reference Converter 替换只读内置材质引用：Default-Material → URP 默认 Lit；Sprites-Default → Renderer2DResources 默认 Lit。后者的 Sprite-Lit-Default Shader 在本版还包含 UniversalForward 的 Unlit Pass，因此不能仅凭“2D/Lit”名称断言通用 Renderer 不支持；最终以 Sprite 视觉验收为准。
- 通用设置/材质转换没有本项目持久化 UI Camera 的跨场景重绑逻辑；PPv2 相机转换不适用于这里。
- 官方窗口和批处理入口均可能调用 AssetDatabase.SaveAssets。转换前必须检查所有未保存资产/场景；存在无关脏内容即停止，不能靠只勾选目标来保证其他脏资产不被保存。
- 官方批处理实现自身警告可能有不完整/不可预期结果并推荐窗口。不直接调用全项目 RunInBatchMode。自动化若必需，只使用审查后的薄封装驱动相同官方扫描/选中项转换，不重写映射规则；入口和异步终态另行验证。

### 本地实现依据

- [官方 Rendering Settings 实现](../../UnityProject/Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Editor/Tools/Converters/BuiltInToURP3DRenderSettingsConverter.cs)。
- [官方 Graphics / Quality 转换基类](../../UnityProject/Library/PackageCache/com.unity.render-pipelines.core@edeb2f5104db/Editor-PrivateShared/Tools/Converter/RenderSettingsConverter.cs)。
- [官方内置材质映射](../../UnityProject/Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Editor/Tools/Converters/BuiltInToURP3DReadonlyMaterialConverter.cs)。
- [官方窗口操作与保存行为](../../UnityProject/Library/PackageCache/com.unity.render-pipelines.core@edeb2f5104db/Editor-PrivateShared/Tools/Converter/Window/RenderPipelineConvertersEditor.cs)。
- [官方批处理入口及警告](../../UnityProject/Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Editor/Tools/Converters/Converters.cs)。

## 已授权的分阶段写入范围与执行记录

### 1. 官方 Rendering Settings

默认输出目录配置为 `Assets/URPDefaultResources/`；尚未发现项目 URPProjectSettings 覆盖。执行前核对实际路径与冲突，发现同名文件先停，不让官方重用路径后覆盖旧值。

```text
Assets/URPDefaultResources/DefaultRenderPipeline.asset
Assets/URPDefaultResources/Very Low.asset
Assets/URPDefaultResources/Low.asset
Assets/URPDefaultResources/Medium.asset
Assets/URPDefaultResources/High.asset
Assets/URPDefaultResources/Very High.asset
Assets/URPDefaultResources/Ultra.asset
Assets/URPDefaultResources/Default_<官方映射的渲染模式>_Renderer.asset
```

- 官方写入 [GraphicsSettings.asset](../../UnityProject/ProjectSettings/GraphicsSettings.asset) 与 [QualitySettings.asset](../../UnityProject/ProjectSettings/QualitySettings.asset)，共预计七个管线资产及共用 Renderer，Unity 生成 meta。
- 不再采用初版自建六个 MarblesLegion_URP 资产、手工映射和另建 Cube 材质的路线。
- 预计旧质量值：Very Low / Low / Medium / High 的 MSAA 0→URP 1（关闭），Very High / Ultra 均为 2；阴影距离 15/20/20/40/70/150，阴影分辨率映射 1024/1024/1024/2048/4096/4096。最终依据实际结果逐档核验，不承诺性能等同。
- 保留当前档位 5，Android/iPhone/tvOS=2、WebGL=3、Standalone=5 等平台默认值。

### 2. 官方材质扫描与限定转换

- 先完成并核验设置转换，再启用依赖 URP 的材质转换器。
- Material Shader Converter：仅扫描并核对候选；当前预期无普通游戏材质需要升级，不批量改 TMP / Sirenix。新发现候选超出本清单则先报告。
- Material Reference Converter：拟纳入下列八个 Prefab 的已确认内置引用，使用官方映射而非新建自定义替代材质。扫描若涉及其他资产/对象，先报告再确认。

```text
Assets/AssetRaw/Actor/Cube.prefab
Assets/AssetRaw/Actor/Prefabs/Battlefield/普通森林_1.prefab
Assets/AssetRaw/Actor/Prefabs/Equipment/bow.prefab
Assets/AssetRaw/Actor/Prefabs/Equipment/shield.prefab
Assets/AssetRaw/Actor/Prefabs/Equipment/spear.prefab
Assets/AssetRaw/Actor/Prefabs/Equipment/sword.prefab
Assets/AssetRaw/Actor/Prefabs/Marbles/Marble.prefab
Assets/AssetRaw/Actor/Prefabs/Projectiles/arrow.prefab
```

- 不勾选无依据的动画、PPv2、2D 灯光等转换项。注意只读材质转换按 GameObject 及子层级操作，不把一条 UI 候选误认为只写一个材质槽。

### 3. 项目特有相机补齐（以迁移后检查为准）

- 拟限定到两个既有场景及 UIRoot：Main Camera 使用 Base、UI Camera 使用 Overlay，必要时排除主相机的 UI 层以免重复绘制；保留投影、位置、Canvas Camera 引用和事件系统。
- 只有确认通用转换后仍缺少持久化 UI 跨场景合成支持，才在主包 `Assets/Launcher/Rendering/` 增加小型堆栈绑定组件，并给 [Launcher.asmdef](../../UnityProject/Assets/Launcher/Launcher.asmdef) 增加必要 URP Runtime 引用及回归测试。不引入热更程序集依赖。
- 使用 Unity API / CLI 保存，不编辑 Scene/Prefab YAML、GUID 或 meta；不强行保存用户无关脏内容。

### 4. 验证与交付

- 检查 Graphics 实际当前管线、七个资产的引用/每档参数、材质 Shader 和编译日志。
- .NET + Unity 编译与相关测试分别留证，不以 .NET 编译代替 Shader / 渲染验收。
- 已获运行验证许可：由 main 进入 Play，检查启动 → Test → 战斗 UI / Sprite / 文字 / 遮罩 / 排序并留截图；退出并恢复原打开场景集合，不保存 Play 修改。
- 不构建 Player / YooAsset 资源包，不切平台、不提交推送或发布；设备性能和打包不宣称已验收。

## 授权

- 用户原始请求为迁移到 URP。
- 用户随后明确要求：“请先告诉我你打算如何迁移，请确认unity的urp包是否提供了迁移流程，尽量使用官方的迁移流程”。这不是对初版自定义写入方案的批准。
- 用户已单独选择“允许运行验证（推荐）”。原先待确认运行验证的状态已更正。
- 用户通过确认问题 approve-official-urp-migration 选择“批准上述限定迁移（推荐）”：批准上述 Graphics/六档 Quality、官方生成 URP 资源、列出的 8 个材质 Prefab、两个场景/UIRoot 必要相机适配与主包代码；清单外改动或无关未保存内容先停，不升级依赖、不构建。运行验证按此前许可。
- 现进入实施；若官方扫描结果超出限定清单，另行预览并确认。
- 补充授权 approve-urp-first-activation：用户选择“允许官方首次初始化（推荐）”，允许官方首次启用时重新导入项目材质、SaveAssets 及新建 `ProjectSettings/URPProjectSettings.asset` 记录版本。仍不批量替换 TMP Shader、不升级包；额外内容性修改先停。实现依据为 URP MaterialReimporter / MaterialPostprocessor：首次版本为 -1，当前有 10 个 upgrader，自动初始化在 Editor update 触发。
- 两次预检：官方输出目录无现有资产、七项均 enabled；活动平台 StandaloneWindows64，Tier3 Forward / HDR=false；main 是唯一打开且干净场景。脏对象只有已授权 QualitySettings 与 non-native 导入型 Shader/TTF 字体纹理，相关 importer 均不脏，没有发现无关可保存原生资产脏改动。

## 实施与证据

- [只读审计完整输出](../../UnityProject/.codex/runs/20261120T113530Z-urp-audit/editor-audit.json)：外层/内层 success=true，diagnostics=[]，compileMs=998、executeMs=60。
- [只读范围复核](../../UnityProject/.codex/runs/20261120T113530Z-urp-audit/read-only-scope-check.json)：九个重点文件 SHA256 与审计前一致。
- [已有工作树差异](../../UnityProject/.codex/runs/urp-preexisting-20261120.diff)、[状态基线](../../UnityProject/.codex/runs/urp-preexisting-20261120.status)、[哈希基线](../../UnityProject/.codex/runs/urp-preexisting-20261120.hashes.json)。保留所有 Unity 升级、Odin、物理 API、TMP 等已有改动，不当作迁移成果。
- 本轮仅补充读取已安装官方包源码及查找当前手册，没有调用转换器、设置写入、资产保存或 Play。
- 方案文档门禁：`python UnityProject/.codex/scripts/workflow.py verify --profile docs` 通过；skill-and-document-structure、workflow-unit-tests 均 passed。[报告](../../UnityProject/.codex/runs/20261120T114602Z-541afcc3/report.md)。这不是 URP 编译或渲染验收。
- [Unity 6.6 官方迁移入口](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/upgrading-from-birp.html)：搜索结果明确 Built-in deprecated 但至少支持完整 Unity 6.7 LTS 生命周期，弃用不等于已移除。网页正文获取遇到跨源重定向，未声称已成功抓取全文；版本具体行为依据本地源码。

### 执行结果（本轮）

- 完整写入前基线：[哈希基线](../../UnityProject/.codex/runs/urp-preactivation-20261120T115653Z/hashes.json)（Assets/ProjectSettings/Packages 全量 SHA256）。
- 官方 Rendering Settings 七项转换：[结果](../../UnityProject/.codex/runs/urp-settings-conversion-20261120T115817Z/result.json)，七项均 Success/Warning（官方对每档固定返回“Settings differ”提示），写出 `Assets/URPDefaultResources/` 七个资产 + `Default_Forward_Renderer.asset`；`GraphicsSettings` 默认管线=DefaultRenderPipeline，六档 Quality 依次指向 Very Low→Ultra，当前档仍为 5；平台默认档 Android/iPhone/tvOS=2、WebGL=3、Standalone=5 未变。
- 逐档实际值（读自资产）：MSAA 1/1/1/1/2/2、阴影距离 15/20/20/40/70/150、阴影分辨率 1024/1024/1024/2048/4096/4096、级联 1/1/1/2/2/4、逐像素灯 4/4/1/2/3/4（pixelLight=0 的档官方映射为 per-vertex 4 并关闭主光阴影）；HDR 因 Tier3 HDR=false 全部关闭（与原 Built-in 一致）。
- 官方首次初始化副作用：`ProjectSettings/URPProjectSettings.asset` 生成，`m_LastMaterialVersion: 10`。
- 官方 Material Reference Converter：[转换结果](../../UnityProject/.codex/runs/urp-material-conversion-20261120T120534Z/result.json)，11 个对象全部 Success（扫描命中 10 个 Sprite 对象；Cube 未被官方搜索索引命中，按其官方 item 构造方式补入，仍由同一官方 Convert 执行）。转换后 Sprite 引用官方 `Sprite-Lit-Default.mat`（guid `a97c1056…`），Cube 引用官方 `Lit.mat`（guid `31321ba1…`）。
- 官方 Material Shader Converter 扫描仅命中 2 个 TMP 材质，但官方 `GetUpgrader` 判定 **无对应 upgrader**（`hasOfficialUpgrader=false`），按授权不批量替换 TMP Shader，未执行该项。
- 相机适配：[应用记录](../../UnityProject/.codex/runs/urp-camera-apply-20261120T121204Z/result.json)：UIRoot/UICamera 增加 `UniversalAdditionalCameraData`（Overlay）与主包组件 `PersistentUICameraStack`；main、Test 的 Main Camera 增加 Base 数据并保存场景；运行期两次绑定确认堆栈内容为 `[UICamera]`。
- 范围复核：[设置写入](../../UnityProject/.codex/runs/urp-settings-scope-20261120T115923Z/changes.json)、[材质写入](../../UnityProject/.codex/runs/urp-material-scope-20261120T120713Z/changes.json)、[相机写入](../../UnityProject/.codex/runs/urp-camera-scope-20261120T121210Z/changes.json)。相对基线只有批准的设置、URP 资产、8 个 Prefab、2 个场景、UIRoot、Launcher 源码/asmdef 及 Unity 生成的 meta，无清单外改动。
- 编译与测试：Editor 编译 0 error；[EditMode 4/4 通过](../../UnityProject/.codex/runs/urp-tests-editmode-20261120T121400Z/status.json)、[PlayMode 2/2 通过](../../UnityProject/.codex/runs/urp-tests-playmode-20261120T121409Z/status.json)。EditMode 使用测试运行器的隔离未命名场景，不新建/保存项目场景。
- .NET 构建：`verify --profile code` 通过（skill-and-document-structure、workflow-unit-tests、solution-build）。
- `verify --profile unity` 未带筛选时以默认 `TEngine.Workflow` 程序集跑测试，结果 `editor-capabilities` / `editor-compilation` 通过，而两个测试项因“该程序集零用例”阻塞——这是筛选参数问题，不是迁移失败；相关用例已用 `--filter Launcher.Rendering.EditMode.Tests` 与 `--filter Launcher.Rendering.PlayMode.Tests` 分别跑出 4/4、2/2。[报告](../../UnityProject/.codex/runs/20261120T121755Z-bc00f270/report.md)。
- 运行验证（Play，main 启动）：启动后 Single 加载 Test，UICamera 持久于 DontDestroyOnLoad 并进入 Test 主相机堆栈；UI 文字/按钮/遮罩正常，射线点击成功推进远征事件；临时摆放 Marble/sword/Cube 后截图确认 Sprite 用 `Universal Render Pipeline/2D/Sprite-Lit-Default`、Cube 用 `Universal Render Pipeline/Lit` 且正常出图（[展示图](../../UnityProject/.codex/runs/urp-play-20261120T121432Z/12-showcase-noui.png)，全部截图见同目录 01–12）。退出 Play 后恢复为仅 main 打开且未修改，`runInBackground` 恢复为关闭。

## 剩余风险

- 官方转换不保证画质/性能完全相同；本次只逐档核对了 URP 资产值与运行截图，未做设备性能或打包验收。
- HDR 因原 Tier3 设置为关闭而保持关闭；如后续需要 HDR，需单独调整管线资产并重新验收。
- Cube 使用 URP Lit，在无光照场景中呈暗色属光照缺失，不是材质转换失败；业务场景需有灯光才有预期明暗。
- 调试战斗入口（ExpeditionMainUI 的 Debug）会出现既有业务异常 `Equipment.cs:24` / `Marble.cs:28`（`RuntimeData` 为空），与本迁移无关，未修改该业务代码；正式远征流程（Start）可正常完成。后续如有需要应单独修复该调试路径。
- Play 期间使用 `editor.playmode set_run_in_background` 临时覆盖，已恢复；未修改 ProjectSettings 中的实际值。
