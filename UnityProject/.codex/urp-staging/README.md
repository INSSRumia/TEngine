# 持久化 UI Camera 堆栈候选（待父代理审核、集成和执行验证）

## 授权边界与当前结果

本次仅写入本 staging 目录，未修改 Assets、Packages、ProjectSettings，未生成 meta，未执行 Unity、编译、导入、测试或构建命令。所有 C# 是候选，**尚无编译/运行通过证据**。父代理正在串行执行官方转换器，本代理没有连接 Editor。

## 仓库事实与实现依据

- [UIModule](../../Assets/GameScripts/HotFix/GameLogic/Module/UIModule/UIModule.cs#L49-L65) 找到 UIRoot 的 Canvas/Camera，然后持久化其父节点；[OnRelease](../../Assets/GameScripts/HotFix/GameLogic/Module/UIModule/UIModule.cs#L102-L114) 关闭窗口并 Destroy 根节点。
- [GameApp](../../Assets/GameScripts/HotFix/GameLogic/GameApp.cs#L36-L41) 随后加载 Test；[SceneModule](../../Assets/TEngine/Runtime/Module/SceneModule/SceneModule.cs#L140-L143) 默认 Single，故入口 MainCamera 被替换，而 UI Camera 存活。
- [Restart](../../Assets/GameScripts/HotFix/GameLogic/SingletonSystem/SingletonSystem.cs#L271-L279) 先禁用 Camera.main 的 GameObject，再 Release，再加载 scene 0。因此不能缓存一次 Camera.main，也不能仅在 sceneLoaded 绑定。
- [UIRoot Canvas](../../Assets/TEngine/Settings/Prefab/UIRoot.prefab#L80-L90) 是 ScreenSpaceCamera 并显式引用 UICamera；[UICamera](../../Assets/TEngine/Settings/Prefab/UIRoot.prefab#L140-L205) 是 layer 5、mask 32、Depth clear、depth 2，非 MainCamera 标签。
- [main](../../Assets/Scenes/main.unity#L137-L190) 和 [Test](../../Assets/AssetRaw/Scenes/Test.unity#L137-L190) 各自有 MainCamera 标签的场景相机、depth -1。[main 的 UIRoot 实例](../../Assets/Scenes/main.unity#L269-L289) 来自上述 UIRoot prefab；不要修改它现有的分辨率、Raycaster 等覆盖。
- 本机包缓存 [package.json](../../Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/package.json#L1-L9) 实际是 URP 17.6.0。
- 官方 [GetUniversalAdditionalCameraData](../../Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Runtime/UniversalAdditionalCameraData.cs#L174-L190) 在不存在组件时会 AddComponent，故不能对所有被枚举相机调用。
- 官方 [renderType / cameraStack](../../Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Runtime/UniversalAdditionalCameraData.cs#L589-L618)：只有 Base 且支持堆栈时才能读 cameraStack；否则警告/null。renderer 为 null 时也不应调用该 getter。
- 官方 [clearDepth](../../Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Runtime/UniversalAdditionalCameraData.cs#L654-L660) **只有 getter**；默认序列化字段为 true（同文件 L506），不能写 `uiData.clearDepth = true`。

## 候选文件及建议目标

| staging 候选 | 审核后目标（Unity 项目内） |
| --- | --- |
| [PersistentUICameraStack.cs](PersistentUICameraStack.cs) | `Assets/Launcher/Rendering/PersistentUICameraStack.cs` |
| [Editor 测试 asmdef](Launcher.Rendering.EditMode.Tests.asmdef) | `Assets/Launcher/Tests/Editor/Launcher.Rendering.EditMode.Tests.asmdef` |
| [Editor 测试](PersistentUICameraStackEditModeTests.cs) | `Assets/Launcher/Tests/Editor/PersistentUICameraStackEditModeTests.cs` |
| [PlayMode 测试 asmdef](Launcher.Rendering.PlayMode.Tests.asmdef) | `Assets/Launcher/Tests/PlayMode/Launcher.Rendering.PlayMode.Tests.asmdef` |
| [PlayMode 测试](PersistentUICameraStackPlayModeTests.cs) | `Assets/Launcher/Tests/PlayMode/PersistentUICameraStackPlayModeTests.cs` |

### 程序集组织

当前 [Launcher.asmdef](../../Assets/Launcher/Launcher.asmdef#L1-L17) 只有两个已有 GUID 引用，不应替换或重写；在 references **追加**名称引用 `Unity.RenderPipelines.Universal.Runtime` 即可。API 源于这个实际存在的 [URP runtime asmdef](../../Library/PackageCache/com.unity.render-pipelines.universal@bb605c4bc710/Runtime/Unity.RenderPipelines.Universal.Runtime.asmdef#L1-L4)。本组件无 GameLogic、TEngine、UniTask、YooAsset 依赖，也不反向引用热更。

当前只有 [TEngine.Resource.Tests.asmdef](../../Assets/TEngine/Tests/Editor/TEngine.Resource.Tests.asmdef#L1-L19)：Editor 限定、UNITY_INCLUDE_TESTS、引用 TEngine/UniTask/YooAsset/UI，没有 Launcher/URP。现有 [SpriteResourceLifecycleTests](../../Assets/TEngine/Tests/Editor/SpriteResourceLifecycleTests.cs#L18-L59) 属于此程序集，并以 EnterPlayMode/ExitPlayMode 承载异步场景。

不要把新测试直接塞入 Launcher 根目录（会让 NUnit 进入生产程序集），也不必扩大 TEngine.Resource.Tests 的主包依赖。两个候选测试 asmdef 都只引用 Launcher 和 URP runtime，TestAssemblies、UNITY_INCLUDE_TESTS、autoReferenced=false。Editor 集是同步 EditMode；PlayMode 集是独立、无 Editor 依赖的 UnityTest。源文件用到的其余类型属于 Unity 引擎模块；若本项目实际编译器报告额外引用缺失，以编译证据补充，不能把本候选当成已编译结论。

### 必要资产集成（只能由父代理在已批准范围内用 Editor/CLI 做）

1. 导入源码、追加 Launcher 的 URP runtime 引用，等待编译终态。
2. 向 UIRoot prefab 的 **UICamera GameObject** 添加 PersistentUICameraStack。不要挂在 MainCamera（会随 Single 卸载），也不要挂到普通 Canvas 上。
3. UICamera 的 UniversalAdditionalCameraData 设为 Overlay，Universal Renderer，Clear Depth=true；保留 Camera mask 32、layer 5、Canvas.worldCamera、布局与所有业务组件。clearDepth 只能经 Inspector/SerializedObject 配置，不要运行时代码强写只读 API。
4. 入口 main 和 Test 的 MainCamera 应是 Base，Universal Renderer；候选不会把一个已标记 Overlay 的主相机强制改回 Base。
5. 新 meta 交 Unity 生成。不要手写 Scene/Prefab YAML/GUID。prefab 已由入口场景引用，不需要静态序列化跨场景 cameraStack 引用。

## 运行时契约

- OnEnable、sceneLoaded、activeSceneChanged 立即尝试绑定；每帧 LateUpdate 在大部分相机控制之后校正，覆盖启停和运行时重建，不依赖 Camera.main 是否存在或缓存。
- 只接受启用、Game 类型、MainCamera 标签、非 RenderTexture、与 UI 相同 display 的候选。优先活动场景，其次 depth 较大，再用 instance id 稳定决策；没有候选时只解除自身绑定，不猜测无标签相机。
- 只把本组件所在的显式 UI Camera 设为 Overlay。只为最终选择的主相机补 AdditionalCameraData；既有主相机必须已是 Base。仅支持本次指定 Universal Renderer，不兼容 Renderer2D 或其他 renderer 时安全解除，不修改 renderer 设置。
- 新绑定默认在 stack 末尾追加 UI。若已有 UI 条目，保留第一个位置并去重，其余 Overlay 的元素和顺序不变。不会调用 Clear，不会删除别的 Overlay/null。
- 保存实际的旧 stack 列表，以便 Base 被禁用、销毁、换模式或 AdditionalData 被移除后仍清理自身引用，避免再调用失效的旧 cameraStack getter。用 ReferenceEquals 删除精确相机引用，避免 Unity 的 destroyed-object 伪 null 比较误删其他已销毁 Overlay。
- 禁用组件/禁用其 GameObject/销毁组件时立即撤销自身绑定；单独禁用 Camera 或 Base 时在下一次 LateUpdate 清理；恢复时重新绑定。无静态全局服务，旧 UI 被销毁后不会拦截新的重启 UI。
- 本组件拥有 UICamera 的 Overlay 模式：禁用组件后不恢复 Base（防止把 UI 突然变成覆盖整个屏幕的独立输出）。禁用组件意味着 UI 不再通过本组件输出；其他系统如需接管应显式配置。

## 测试内容及验收入口建议

候选一共 **4 个 EditMode Test + 2 个 PlayMode UnityTest**（不是已运行结果）：

- 初始绑定与仅为选中主相机增加 AdditionalData；无关无标签相机保持不变。
- 多次 Refresh 与重复条目清理，同时保留其他 Overlay 的顺序。
- 活动场景变更时迁移绑定并清理仍存活旧 Base。
- 主相机已改 Overlay 时不会被强制转 Base，旧列表上的 UI 被移除。
- 真正运行帧中的 UI Camera.enabled、binder.enabled、Base.enabled、UI GameObject 启停、旧 Base 销毁/替换、UI 销毁，以及销毁后的缓存 stack 清理。
- DDOL UI 跨合成场景卸载绑定，以及不引用热更的 Restart 顺序模拟（禁用旧 Base → 销毁旧根 → 新场景/新 UI）。

测试创建内存场景和 GameObject，不改资源、不切管线；使用 display 7 与本项目 display 0 相机隔离，**只断言拓扑，不证明画面**。前提是当前实际启用 URP + Universal Renderer；缺少前提必须失败而非跳过。建议父代理通过实时发现的 Pipeline 测试命令分别选择两个精确程序集和对应模式，确认运行数为 4/2 且零失败、零跳过。现有默认工作流测试集未必自动包含它们。

另需父代理串行执行：完整 .NET 构建 + Editor 编译（确认新源确实已导入和列入输入）；从 main 真正启动至 Test，检查 UI 持久化、MainCamera.stack 恰有一个 UICamera、可点击、无深度遮挡，Restart 后仍正确。合成测试有意不调用业务入口、不触发 YooAsset，不能取代真实 Single 场景加载和重启验收。

## 仍需关注的风险

1. 官方转换器不解决持久化 UI 的动态 rebind。只配一条入口场景的 stack 引用仍会在 Test 断链。
2. 当前只支持一个活动主输出与一个持久 UI；多 Display、分屏、RT UI、无 MainCamera 标签、多个竞争的 UIRoot 不在本次约定内。若它们未来是需求，必须加显式路由和额外测试，不能简单给所有 Base 追加同一 UI。
3. 轮询是为捕捉无事件的 Camera.enabled/销毁而保留的最小机制，O(启用 Camera 数) 每帧；缓存数组只在增长时分配。极大量相机需另行性能评估。
4. 在本组件 LateUpdate 之后才由其他极晚执行脚本切换 Camera.enabled，最多需下一帧才能校正；若未来要求该时机零帧延迟，应加渲染前契约/显式 Refresh 调用并单独验证，不能宣称已覆盖所有任意执行顺序。
5. PlayMode 测试走 CreateScene + 切 active + 卸载旧场景来复现持久关系，**不是实际 YooAsset Single load**。真实 main→Test 与 Restart 必须补充。
6. 当前无执行通过证据。若 Editor/PlayMode 下 Camera.GetAllCameras、display 7 或测试发现行为与预期不同，以实际失败修正测试隔离方式，不能改成忽略用例。
