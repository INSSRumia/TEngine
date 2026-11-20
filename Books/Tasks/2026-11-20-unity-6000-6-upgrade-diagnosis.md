# Unity 6000.6 升级后的警告与错误诊断

- ID: 20261120-unity-6000-6-upgrade-diagnosis
- 状态: 阶段完成（源码编译修复与 CLI 恢复已核实；剩余 Odin/DLL 导入器问题仅诊断，待另行授权）
- 时间说明：下方初次诊断为历史快照；用户刷新并退出 Safe Mode 后的当前状态以文末追加复查为准。Editor 重启后日志已轮换，历史日志行号不再对应当前内容。
- 负责人: AI

## 目标与验收

用户请求：“我现在用新版的unity打开了项目，目前有很多警告和报错，请你看看是什么问题”。

- [x] 确认实际 Editor 版本、进程与目标工程。
- [x] 检查 CLI 连接并保留失败证据；不可连接时不冒充取得 Console 实时查询结果。
- [x] 从当前会话的 Editor 日志定位错误，核对源码、升级改动与依赖。
- [x] 复核后按严重度给出修复顺序与未验证项。

## 范围、授权与保护

本轮仅诊断；允许读取本工程状态、源码、包清单、编译日志并写诊断记录和工具证据。
不修改源码、包依赖、项目设置、资源或个人布局；不清空 Console、不安装工具、不提交或推送。
保留工作区已有的 Python CLI 修复、Unity 升级改动及 8 个战斗源码文件的物理 API 迁移。
不读取 Pipeline 鉴权描述文件；原始 Editor 日志可能含登录凭据，不复制日志头部或全量日志作为交付。

## 当前版本与目标身份

- 工程：`D:\UnityProject\Marbles Legion\UnityProject`。
- 进程：PID `62712`，Editor 来自 `D:\SoftWare\UnityEditor\6000.6.10f1\Editor\Unity.exe`，进程的 projectPath 与目标逐字匹配。
- [ProjectVersion](../../UnityProject/ProjectSettings/ProjectVersion.txt)：`6000.6.10f1 (40e81493c67d)`；之前为 `2022.3.62f3`。
- CLI `1.0.0-beta.3`、Pipeline `0.10.0-exp.1`；[YooAsset 元数据](../../UnityProject/Packages/YooAsset/package.json#L1-L5)：内嵌 `2.3.17`。
- [manifest](../../UnityProject/Packages/manifest.json) 与 Git 差异确认：URP `14.0.12 → 17.6.0`、AI Navigation `1.1.6 → 2.0.16`、UGUI `2.0.0 → 2.6.0`，独立 TMP 依赖已移除，新增 Unity 6 内置模块等。
- [包锁](../../UnityProject/Packages/packages-lock.json#L3-L8) 中 HybridCLR 仍为 `7e74e59637055b535d50f41b69c94605da2a7a40`；不能据此声称其本地 IL2CPP 和新版本已匹配。

## 实际证据

### CLI 当前不可用，不能通过重装掩盖编译失败

执行 `python .codex/scripts/workflow.py doctor`：环境检查 passed，Editor connection blocked。
[doctor 报告](../../UnityProject/.codex/runs/20261120T103919Z-f864a923/report.md) 和 [结构化证据](../../UnityProject/.codex/runs/20261120T103919Z-f864a923/report.json#L68-L85)：原生 status 退出码 `6`、`STATUS_NO_INSTANCES`、实例数 `0`。

进程已运行，但“进程存在”和“Pipeline 服务已注册”不是同一件事。
当前 [Editor 日志](../../UnityProject/Logs/Editor.log#L6570) 明确记录 `Safe Mode: Only loading a subset of assemblies`。
编译失败及 Safe Mode 足以解释服务无法正常初始化的情境；尚不能排除服务设置等其他因素，需修复编译后再检查。
不执行 CLI 错误提示中的 `pipeline install`，工程已有内嵌包。

Unity 6000.6 当前会话的主日志位于工程内 [Logs/Editor.log](../../UnityProject/Logs/Editor.log)，用户目录的 Editor 日志仅保留启动和日志重定向信息；未拿上一次 2022 的日志冒充当前错误。

### P0：YooAsset 编辑器 UI 的旧 UXML 工厂无法编译

最新一次编译输出见 [错误区间](../../UnityProject/Logs/Editor.log#L6504-L6566)：7 个自定义控件，每个 `CS0308` 与 `CS0246` 各一条，共 **14 条独立编译错误**，另有相关 `CS0109` 警告。日志重复打印了相同错误，不代表每行都是独立故障。

| 控件 | 定位 |
|---|---|
| ToggleDisplay | [ToggleDisplay.cs:15](../../UnityProject/Packages/YooAsset/Editor/UIElements/ElementsDefine/ToggleDisplay.cs#L15) |
| ToggleFoldout | [ToggleFoldout.cs:15](../../UnityProject/Packages/YooAsset/Editor/UIElements/ElementsDefine/ToggleFoldout.cs#L15) |
| ToggleRecord | [ToggleRecord.cs:15](../../UnityProject/Packages/YooAsset/Editor/UIElements/ElementsDefine/ToggleRecord.cs#L15) |
| ReorderableListView | [ReorderableListView.cs:15](../../UnityProject/Packages/YooAsset/Editor/UIElements/ElementsDefine/ReorderableListView.cs#L15) |
| ResizeHandle | [ResizeHandle.cs:12](../../UnityProject/Packages/YooAsset/Editor/UIElements/ElementsDefine/ResizeHandle.cs#L12) |
| TableViewer | [TableViewer.cs:18](../../UnityProject/Packages/YooAsset/Editor/UIElements/TableViewer/TableViewer.cs#L18) |
| TreeViewer | [TreeViewer.cs:14](../../UnityProject/Packages/YooAsset/Editor/UIElements/TreeViewer/TreeViewer.cs#L14) |

共同使用旧的 `UxmlFactory<T, UxmlTraits>` 注册方式；当前编译中旧泛型基类/traits 不可用，产生“非泛型类型不能带类型参数”和“找不到 UxmlTraits”。不是缺少一个普通 using，也不是 CLI 把 editor 命令拆分的旧问题。

独立只读复核了本机三个 Unity 安装的 UIElements DLL PE 元数据：2022.3.62f3、6000.5.5f1 中旧泛型工厂及 Traits 存在，6000.6.10f1 中已不存在，而后者提供 `UxmlElementAttribute`。不能泛化成“所有 Unity 6 都已移除旧 API”。当前版 [本地 API 文档](D:/SoftWare/UnityEditor/6000.6.10f1/Editor/Data/Managed/UnityEngine/UnityEngine.UIElementsModule.xml#L24406-L24419) 及 [内置 PlayButton 示例](D:/SoftWare/UnityEditor/6000.6.10f1/Editor/Data/Resources/PackageManager/BuiltInPackages/com.unity.netcode/Editor/Tracing/UI/TracingToolbar/CustomElements/PlayButton.cs#L7-L8) 也明确支持 `[UxmlElement]` 与 `partial class`。

建议优先对 7 类做定点编辑器 UI 兼容适配：当前新版走 `[UxmlElement] partial class` 注册；若需保留旧 Unity 支持，则条件编译保留旧工厂分支。七个旧工厂均无自定义 Traits，当前 UXML 仅使用标准 name/style，不应批量添加不必要的 UxmlAttribute。

已找到 13 处相关 UXML 引用，需保留 `YooAsset.Editor` 命名空间、原类型名，以及 `FrameRecord`、`TopTableView`、`BottomTableView`、`BundleTableView`、`UsingTableView`、`ReferenceTableView`、`BottomTreeView` 等查询名。不能只屏蔽工厂让 C# 编译，却不补类型注册。还需实际验证 UXML 实例化、Q<T>() 结果和 Toggle 对 `unity-checkmark` 层级的依赖。这里给出修复方向，不声称修改后已经编译通过，也不把整包升级或删除控件当作默认修复。

### P1：12 个 DLL 的 PluginImporter 序列化格式过旧

[启动诊断列表](../../UnityProject/Logs/Editor.log#L317-L328) 包含 11 个 Sirenix/Odin DLL 和 1 个 Localization XCode DLL，其 `PluginImporter` 版本为 `1`，Unity 提示最低支持为 `2`，且日志多次重复输出。

源码文件核实例子：
- [Sirenix.OdinInspector.Editor.dll.meta:4–5](../../UnityProject/Assets/Plugins/Sirenix/Assemblies/Sirenix.OdinInspector.Editor.dll.meta#L4-L5)。
- [UnityEditor.iOS_I2Loc.Xcode.dll.meta:5–6](../../UnityProject/Assets/TEngine/Editor/Localization/Unity%20XCode/UnityEditor.iOS_I2Loc.Xcode.dll.meta#L5-L6)。

需要由 Unity 导入器按原 GUID、平台启用范围等配置迁移/重新保存并复查。不能手工仅改 serializedVersion，也不能仅凭元数据问题断言所有 DLL 二进制已经兼容新版。资源导入与重保存属于另行预览、确认的 Unity 写入范围。

### P2：API 弃用与既有可空警告

1. 物理 API：`velocity → linearVelocity`、`drag → linearDamping`、`angularDrag → angularDamping`。
   本会话初期日志保留弃用警告；当前 Git 差异显示 8 个战斗文件已经完成这些替换，目标战斗目录搜索旧成员无结果。例如 [EquipmentBrokenAbility.cs:29–30](../../UnityProject/Assets/GameScripts/HotFix/GameLogic/GamePlay/Combat/Equipment/Ability/Core/EquipmentBrokenAbility.cs#L29-L30)。因此不能把这些历史日志行都认定为当前仍待修改；也不能在整体编译尚未成功时宣称迁移已验收。
2. 对象查找： [GameModule.cs:14](../../UnityProject/Assets/GameScripts/HotFix/GameLogic/GameModule.cs#L14) 的 `FindObjectOfType`、[Utility.Unity.cs:330](../../UnityProject/Assets/TEngine/Runtime/Core/Utility/Utility.Unity.cs#L330) 的 `FindFirstObjectByType` 仍有 `CS0618`。应按“任意实例还是有序第一实例”的真实语义选替代 API，不机械替换后假定行为相同。
3. [UIBase.cs:24](../../UnityProject/Assets/GameScripts/HotFix/GameLogic/Module/UIModule/UIBase.cs#L24) 的 `Action<UIBase>?` 在未启用 nullable annotations 的代码中产生 `CS8632`。旧 2022.3 会话日志也有相同警告，所以不是这次升级新引入的编译阻塞。

### P2/P3：缓存与工具弃用提示

- [API Updater 提示](../../UnityProject/Logs/Editor.log#L329-L332)：旧依赖图签名与新版不符，可能为跨版本缓存。当前已存在明确源码编译错误，删除缓存不会让缺失 API 恢复；不能把删除整个 Library 当作第一步，任何清理需单独明确范围与授权。
- [VS Code 包提示](../../UnityProject/Logs/Editor.log#L313)：`com.unity.ide.vscode` 已停止支持。属于工具包维护事项，不是当前 14 条编译错误的根因；是否移除/更换依赖需另行确认。

## 建议修复顺序与未验证项

1. 首先适配 YooAsset 7 个控件的 UXML 注册，取得一次 Unity 实际编译成功结果。
2. 预览并确认 12 个 DLL 导入器的迁移范围，由 Unity 重新保存，核对 GUID 与平台配置保持不变。
3. 确认能正常退出 Safe Mode，再运行 doctor、命令发现与只读 Console/场景查询；不强制点击退出 Safe Mode、不重装内嵌 Pipeline。
4. 清理仍存在的对象查找/nullable 警告；区分已经自动迁移的物理 API 历史日志。
5. 编译和连接恢复后再验证 URP、资源构建链、HybridCLR/IL2CPP 与游戏逻辑；不能把“Editor 打开”当成整工程升级成功。

本轮未触发 Unity 编译、Unity 测试、Play Mode、导入、缓存删除或真实构建。Console API 因连接不可用而未执行，结论基于目标进程、当前会话日志、实际源码和 Git 差异；不宣称全量运行兼容已通过。

实际执行离线 `python UnityProject/.codex/scripts/workflow.py verify --profile docs`，退出码 0；[验证报告](../../UnityProject/.codex/runs/20261120T104348Z-f9ff903e/report.md#L3-L11) 记录文档/技能结构检查通过，63 个 Python 单元测试通过。这只是诊断记录和工具的离线检查，不代表 Unity 工程编译或运行通过。
收尾 `git status --short` 与诊断轮基线对比，仅新增本诊断记录及忽略目录内的运行证据，没有修改其他原有工作区文件。

## 后续处理：YooAsset 七控件兼容补丁

### 授权与验收

用户在上述诊断交付后明确请求“请你开始处理”。先实施前述表格中 7 个 C# 源文件的定点修复，保留旧版分支；不扩大为整包升级或依赖/设置改动。12 个 DLL 的导入器迁移需单独展示清单与保存范围后确认；不直接编辑 meta，不自动退出 Safe Mode 或触发资产全量重保存。

- [x] 七控件新版本使用 UxmlElement + partial，旧版本保留原 UxmlFactory 分支（旧版本未实际编译）。
- [x] 命名空间、类名、UXML 查询名、构造函数与交互逻辑保持不变。
- [x] 独立差异审查；额外 TMP 编译阻塞修复与范围已明确记录，无资源/设置写入。
- [x] 执行完整 .NET 解决方案验证并记录实际结果，失败不伪装成通过。
- [x] 核实当前 Unity 编译状态与 CLI 连接，明确 UI 实例化和交互的验收缺口。

### 实施与验证进展

开始前 `git status --short` 与诊断交付基线一致，目标七文件均无已有本地改动。实际补丁仅添加 `partial`、为 `UNITY_6000_6_OR_NEWER` 标记 UxmlElement，并将原嵌套工厂放入反向条件分支；不引入 UxmlAttribute 或新 UXML 资源。源码编辑可能由已打开的 Unity 自动导入，不手动重放刷新或编译操作。

首轮完整 code 验证消除了七控件错误，但发现 [HTML 转 UGUI 烘焙器](../../UnityProject/Assets/TEngine/Extension/HtmlToUGUI/Editor/HtmlToUGUIBaker.cs#L1532-L1539) 的七处 `TMP_Text.enableWordWrapping` 在 6000.6 被标记为错误级弃用。已告知用户并按原换行语义添加条件编译辅助方法：新版 true/false 精确映射为 Normal/NoWrap，旧版仍用原属性；七个调用点替换为辅助方法，其余布局与溢出设置不动。核对当前 TMP setter 确认映射一致，没有擅自使用 PreserveWhitespace。

- 首次 [code 验证](../../UnityProject/.codex/runs/20261120T104746Z-7540260b/report.md)：结构与 63 个 Python 用例通过，解决方案构建失败于上述七处 TMP 调用。
- 修复后 [code 验证](../../UnityProject/.codex/runs/20261120T104917Z-ffdd75cd/report.md#L7-L14)：结构、63 个 Python 用例、完整 28 工程/1507 源文件的 .NET 解决方案构建通过，77 个警告、0 个错误。不是 Unity 运行/渲染测试。
- 独立只读审查未发现必须修正的补丁缺陷；确认七控件分支边界、13 处 UXML 契约和 TMP 旧 setter 语义。八个源码文件原有 BOM 均已保留；审查过程中指出的 BOM 差异已消除。
- 用户随后说明已经手动刷新 GUI、编译通过并退出 Safe Mode。主线程只读 doctor 与编译/Console 查询交叉核实，未强制退出或另起 Editor。
- 尚未执行七控件 CloneTree/Q<T>()、Toggle 交互、文本烘焙视觉、PlayMode、旧 Unity 编译、HybridCLR/IL2CPP 或真实构建，均不能标为通过。

## 退出 Safe Mode 后的当前复查（2026-11-20 18:57，Asia/Shanghai）

### 新授权边界与身份

用户反馈“现在 editor 编译已经通过……已经退出了 safe mode，但现在日志中还存在大量警告和4个报错，请你看看是什么问题”。本阶段仅诊断剩余日志；不追加插件升级、DLL 重保存、资源/设置或包目录改动。

[当前 doctor](../../UnityProject/.codex/runs/20261120T105640Z-ffabe58b/report.json#L103-L123) 已通过：当前唯一目标实例 PID `67172`、Unity `6000.6.10f1`、projectPath 精确匹配、ready、未编译、未重载、未播放。与此前 PID `62712` 的 Safe Mode 会话不同，不复用旧日志行号作为当前证据。

[编译只读状态](../../UnityProject/.codex/runs/20261120T105711Z-25e73402/report.json#L15-L20) 为 idle，failed=false、compilationFailed=false。idle 仅表示没有 CLI 主动触发的编译任务，不将其包装为本轮主动编译成功；结合用户实际编译、无编译失败标记和可用 CLI，确认此前阻塞编译与连接的问题已解除。

### 4 个红色条目：Odin 的两处初始化失败，各报告两条

[实时错误查询](../../UnityProject/.codex/runs/20261120T105723Z-1a781f0f/report.json) 返回完整 4 条，与 Unity Console groundTruth 相符，dropped=false：

1. `Exception while executing InitializeOnLoad for ImguiElementUtils.Init`。
2. 随后的 MissingFieldException，栈指向 `Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration.ImguiElementUtils.Init`。
3. `Exception while executing InitializeOnLoad for GUITimeHelper.Init`。
4. 随后的相同字段 MissingFieldException，栈指向 `Sirenix.Utilities.Editor.GUITimeHelper.Init`。

两处都引用找不到的内部字段 `UnityEngine.UIElements.UIElementsUtility.s_BeginContainerCallback`。对应当前 [日志的两组异常](../../UnityProject/Logs/Editor.log#L1164-L1212)。所以不是四个独立业务错误，也不是七控件/TMP 修复后又出现四个 C# 编译错误，而是旧 Odin 编辑器二进制对 Unity 内部 UI API 的兼容失败，可能影响 Odin Inspector 的绘制与 GUI 时序。

独立只读 PE 元数据核查当前 Odin 的 SirenixBuildVersionAttribute 为 `4.0.1.0`、构建名 Personal，不能拿通用 FileVersion `1.0.0.0` 当插件版本。两个 DLL 的 MVID 与异常栈一致。核查本机 Unity 内部成员元数据：2022.3.62f3 的回调字段和 GetCurrentIMGUIContainer 均位于 UIElementsUtility；6000.5.5f1 均迁到 UIElementsIMGUIUtility；6000.6.10f1 的回调留在 UIElementsIMGUIUtility、方法又迁到 IMGUIContainer。不能说字段从所有类型中删除，也不能泛称仅 6000.6 才发生变化。

官方已存在对应修复记录：[Odin 4.0.1.5](https://odininspector.com/patch-notes/4-0-1-5) 提到 Unity 6.5/6.6 的内部成员迁移；[Odin 4.0.2](https://odininspector.com/patch-notes/4-0-2-0) 进一步明确修复 Unity 6.6 中 GetCurrentIMGUIContainer 移到 IMGUIContainer、回调仍在 UIElementsIMGUIUtility 的兼容性。主线程已抓取这两份官方说明核对，4.0.2 有与当前内部 API 变化直接对应的修复依据。

修复方向：优先取得官方 Odin 4.0.2 或后续包含该修复的完整匹配版本并核对项目授权与发行说明，不能只替换单个 DLL、乱配版本，也不应默认禁用整个 Odin。项目这里是已编译 DLL，之前的业务 C# 条件编译补丁不能直接修补其内部实现。升级前预览完整替换清单、GUID/配置保护与回归范围后另行确认；升级后仍须验证 Inspector、序列化及日志，不能凭发行说明宣称全项目已兼容。未执行任何插件升级。

### 124 条警告的完整分类

[实时警告查询](../../UnityProject/.codex/runs/20261120T105726Z-4d4ce532/report.json) 按 level=warn、tail=500 返回全部 128 个警告/错误条目，groundTruth 警告124、错误4、编译失败false，未丢失。按消息分组的 [只读汇总](../../UnityProject/.codex/runs/20261120T105640Z-ffabe58b/console-summary.md#L19-L81)：

| 数量 | 内容 | 结论 |
|---|---|---|
| 120 | 12 个 DLL 的 PluginImporter 版本1低于最低2，重复8或11次 | 11个 Sirenix + 1个 Localization XCode。是导入元数据格式迁移问题，不是120个不同插件；与 DLL 内部引用失效是两个层面，重存 meta 本身不能修复 MissingFieldException。 |
| 1 | 旧 Input Manager 标记弃用 | 不表示当前输入已停止工作；迁移 Input System 属于独立功能改造，不擅自切换。 |
| 1 | UniTask 内嵌目录名与包名不一致 | 目录 Packages/UniTask 对应 com.cysharp.unitask，属于包结构规范提示，不是缺依赖。 |
| 1 | YooAsset 内嵌目录名与包名不一致 | 目录 Packages/YooAsset 对应 com.tuyoogame.yooasset，同上；不盲目改名破坏引用。 |
| 1 | com.unity.ide.vscode 已停止支持 | 编辑器工具包维护，不是四条初始化异常根因。 |

12 个 importer 目标仍与最初列出的资源一致，当前 [启动列表](../../UnityProject/Logs/Editor.log#L291-L302) 可核对；尚未执行 Unity 重保存或手改 meta。应先确定 Odin 升级方案，再核对随包元数据及剩余 XCode 导入器，避免先迁移后又被升级包覆盖。

本次当前 Console 中未出现源码 CS 警告，不把完整 .NET 构建的77条警告或此前会话的源码警告混入这124条。全部检查为只读 Unity 查询，没有清空 Console、开启 PlayMode、运行测试、调用刷新/重编译或改动导入器。GUI 恢复后工作区另有 TMP 资源、图集、纹理 meta 与设置改动，均保留，未代替用户回滚；全工作区 diff --check 的现存格式提示不在本轮源码修复范围。限定本轮八个源码的 diff --check 无输出、退出0。

记录更新后执行 [docs 验证](../../UnityProject/.codex/runs/20261120T105954Z-d933b5b5/report.md)：文档/技能结构及63个 Python 用例通过，退出0。本轮完成的是编译兼容修复、CLI 恢复核实和剩余问题诊断，不代表 Odin、资源、游戏运行与跨版本兼容全部验收通过。

## 用户自行更新 Odin 后的警告复查（2026-11-20 19:16，Asia/Shanghai）

用户反馈已升级 Odin 并重启，要求说明警告原因、是否需要修复及修法。本轮仅只读诊断，未获包/设置/资源写入授权。保留本次 Odin DLL 与元数据升级、此前补丁及其余已有改动；没有进一步修改这些内容。

### 当前身份与完整 Console 快照

- [doctor](../../UnityProject/.codex/runs/20261120T111512Z-f4a3515f/report.md#L7-L11) 通过；精确目标工程的新实例 PID `62760`、Unity `6000.6.10f1`，ready、未编译、未域重载、未播放。
- [本轮命令发现](../../UnityProject/.codex/runs/20261120T111519Z-f411de03/report.md) 确认 `console get` 支持 tail/level；以 level=warn、tail=1000 读取。
- [Console 结构化证据](../../UnityProject/.codex/runs/20261120T111631Z-a8e6c19c/report.json) 完整返回15条，与 groundTruth 一致：错误0、警告15、compilationFailed=false、dropped=false。不是复用此前4错124警告的会话。
- 之前 Odin 的两处初始化异常与11个 Sirenix DLL 的旧导入器警告，本次均未出现；只能确认该启动快照，不代表已运行全部 Odin Inspector/序列化回归。

### 五种警告与处理决定

| 数量 | 原因 | 是否修复、如何修复 |
|---|---|---|
| 11 | 同一个 Localization XCode DLL 的 PluginImporter 序列化版本1低于新 Editor 最低版本2；编译、程序集验证、Rider 工程同步均会触发读取，所以重复报告 | 建议优先定点修复。由 Unity 对这个 DLL 的导入器元数据重保存/迁移；保留 GUID、Editor-only 配置及 DLL 本体，不手改 serializedVersion，不全量重存工程。 |
| 1 | 当前仍启用旧 Input Manager | 短期保留，长期单独规划 Input System 迁移。不是当前功能损坏信号；直接切成新系统可能使旧输入调用和 UI 模块失效。 |
| 1 | UniTask 文件夹名与 package.json 的 name 不一致 | 低优先级，可暂留；规范化目录为 Packages/com.cysharp.unitask，保留包内容与元数据、检查路径引用、重新解析并回归。 |
| 1 | YooAsset 文件夹名与 package.json 的 name 不一致 | 同上；规范化目录为 Packages/com.tuyoogame.yooasset，特别保留七控件本地兼容补丁，不能用重装原包覆盖。 |
| 1 | manifest 仍依赖已停止维护的 com.unity.ide.vscode 1.2.5 | 建议顺手维护。若不依赖此旧集成包，用 Package Manager 移除它，由 Unity 更新 manifest/lock；保留正在使用的 Rider/Visual Studio 集成。若用 VS Code，先确认当前受支持扩展及项目生成功能再移除。 |

### 修复范围与源码核实

1. 唯一仍需导入器迁移的目标：[XCode DLL 元数据](../../UnityProject/Assets/TEngine/Editor/Localization/Unity%20XCode/UnityEditor.iOS_I2Loc.Xcode.dll.meta#L1-L21) 明确为 PluginImporter serializedVersion=1，GUID `6bcca352fcef3034fb01650308bf47c2`，Any禁用、Editor启用、WindowsStoreApps禁用。已核实本机 Unity API 文档存在 `AssetDatabase.ForceReserializeAssets` 的 `ReserializeMetadata` 选项，可定向仅升级目标资源的 meta；路径参数应为 DLL 资源路径而非 meta。实施前还需按 Unity 写入规程预览并确认，再核对迁移前后全部平台设置与 GUID。不通过开关平台权限来强行制造脏数据，也不保证单纯点击 Reimport 必然重写元数据。
2. [iOS 构建后处理](../../UnityProject/Assets/TEngine/Editor/Localization/PostProcessBuild_IOS.cs#L1-L18) 仍使用该插件命名空间；[后处理主体](../../UnityProject/Assets/TEngine/Editor/Localization/PostProcessBuild_IOS.cs#L29-L92) 使用 PlistDocument/PBXProject 并处理本地化，不应为了去警告直接删 DLL。是否将来替换为 Unity 官方 Xcode API，属于另外的兼容改造，不在本次建议的最小修复内。
3. [Player 设置](../../UnityProject/ProjectSettings/ProjectSettings.asset#L949) activeInputHandler=0。[UI 烘焙器](../../UnityProject/Assets/TEngine/Extension/HtmlToUGUI/Editor/HtmlToUGUIBaker.cs#L533-L536) 仍生成 StandaloneInputModule；[调试器输入面板](../../UnityProject/Assets/TEngine/Runtime/Module/DebugerModule/Component/DebuggerModule.InputSummaryInformationWindow.cs#L14-L25) 仍调用 UnityEngine.Input。因此不建议只切 Player Setting 以消黄；迁移应包含事件系统、UI生成器、鼠标/触摸/键盘等调用与实际输入回归，Both最多是过渡且仍启用旧系统，不作为消警保证。
4. 包名来自 [UniTask 元数据](../../UnityProject/Packages/UniTask/package.json#L1-L6) 与 [YooAsset 元数据](../../UnityProject/Packages/YooAsset/package.json#L1-L5)，应改文件夹名而非修改包名/程序集名。当前对 Assets/YooAsset C#、Packages UXML 与 .codex Python 的定点路径搜索无匹配，不等于全工程路径依赖已审计，迁移前仍须检查其他配置/文档/工具。
5. [manifest](../../UnityProject/Packages/manifest.json#L6-L8) 已有 Rider3.1.0、Visual Studio2.0.26 以及旧 VS Code1.2.5，三者是不同包，不应为移除旧 VS Code 包而连带删除另外两个。

### 建议顺序与验证边界

先定点迁移 XCode DLL 元数据，再按实际 IDE 使用情况移除旧 VS Code 包；目录命名可以之后集中规范化，输入系统暂留并建立独立迁移范围。全部写入均未执行。没有清 Console、主动重编译、重导入、PlayMode 或构建；不以0错误快照替代功能验收。桌面语音脚本因本地39555服务拒绝连接而失败，未为此修改服务或环境，诊断不受影响。

本轮 [docs 检查](../../UnityProject/.codex/runs/20261120T111747Z-57ee771d/report.md) 通过：文档/技能结构及63个 Python 用例通过、退出0；这不是额外 Unity 功能验证。

## 两项警告定点处理（用户授权：2026-11-20）

用户明确逐项批准：1处理 XCode DLL 导入元数据，2 Input Manager先不管，3包目录不管，4移除旧VS Code包。该授权不包含更换DLL、改GUID、改平台启用配置、改输入设置、移动包目录或清 Console。

- [操作前 doctor](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/report.md) 与实时 editor status 确认同项目 ready、未编译/重载、未播放。工作区原有大量用户/Odin/Unity升级改动，均保留；基线 manifest、lock、目标meta、DLL与PlayerSettings的SHA256及工作树diff已存入该次runs目录。
- 实时发现 `asset set_import_settings`、`asset get_import_settings`、`package remove`。现有导入设置命令源码默认分支设置成员后调用 `SaveAndReimport`，可直接用于单目标重保存，不必新增脚本或修改Pipeline。
- [资源查询](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/importer-before.json) 确认目标是 PluginImporter，userData=""、assetBundleName=""。[定点重保存预览](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/importer-preview.json)：仅对原DLL路径重设 userData 为原空字符串，platform=Default，applied=[userData]、unknown=[]；不会变更任何平台开关。实际调用去除dry-run，触发SaveAndReimport；写后以meta实际版本和配置比对验收，不仅看命令success。
- [包移除预览](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/package-preview.json)：仅移除 `com.unity.ide.vscode`，status=dry_run、applied=false。实际通过原生CLI `command --project-path . "package remove" --name com.unity.ide.vscode --confirm true` 执行一次，串行观察包状态/编译；manifest与lock由Unity生成，保留Rider/VisualStudio。
- 两项命令未纳入包装器apply策略，因此按已读实现、实时schema、dry-run与本次明确授权使用原生CLI，stdout存入同一审计目录；未绕过项目或授权边界。

### 实施结果与验证（本轮已完成）

- [导入器写入结果](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/importer-apply.json) 成功，applied=[userData]、unknown=[]；Unity 实际将目标meta从PluginImporter版本1升级为3，去除旧timeCreated/licenseType，补入当前版本默认字段。没有手改meta或更换DLL。
- [范围校验](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/scope-verification.json) 七项全部通过：GUID相同，导入器已升级，旧导入器全部既有设置（除版本号）逐字段相同，DLL与PlayerSettings SHA256相同；manifest及lock结构化差异均精确等于仅删除com.unity.ide.vscode。
- [包移除起始响应](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/package-apply.json) 为in_progress，随后 [package status](../../UnityProject/.codex/runs/20261120T112656Z-37a7b059/report.md) 确认completed、success=true、error=null，保留Rider3.1.0和VisualStudio2.0.26。
- [编译终态](../../UnityProject/.codex/runs/20261120T112659Z-05b7678b/report.md) 为completed、failed=false、errors=[]、compilationFailed=false。[Editor状态](../../UnityProject/.codex/runs/20261120T112702Z-3a3f5a3a/report.md) ready、未编译/重载、codeLoadCount=3、Play停止。
- [写后资源查询](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/importer-after.json) 确认PluginImporter仍能读取，userData与assetBundleName均保持空。
- [完整code验证](../../UnityProject/.codex/runs/20261120T112727Z-d2e803f5/report.md) 通过：文档/技能结构、63个Python测试、28工程/1507源文件完整.NET解决方案构建成功（88警告、0错误）。.NET构建警告不等于Unity Console计数，本轮不扩大修改范围以追求零警告。
- 审阅目标meta差异、操作前后manifest/lock差异；目标三文件git diff --check通过。注意manifest/lock相对HEAD还有此前Unity升级改动，本轮对比使用操作前快照，不能把HEAD总差异归为本轮改动。PlayerSettings虽原已处于修改状态，但本轮hash完全不变。UniTask/YooAsset目录保留原名。

### Console计数与剩余范围

[当前Console汇总](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/console-summary.json) 的groundTruth为错误0、警告33、compilationFailed=false；捕获缓冲是36条warn、dropped=false。两者不是同一计数口径，不把缓冲36当作GUI警告数量。

没有清除历史日志，所以不能声称当前Console只剩原来要求保留的3种提示。[逐条时间证据](../../UnityProject/.codex/runs/20261120T112250Z-a0f69926/warning-times.json) 显示：旧XCode导入器警告共捕获13次，最后一次为11:26:18Z（读旧导入器及写入前读取期间），VS Code弃用记录仍为11:13:12Z的历史启动提示；写后读取与包重新解析没有新增这两类警告。包目录提示在重新解析时再次报告，属用户明确暂留范围。

此外，包移除引发的正常重新编译于11:26:44Z暴露17条现有源码警告：ReferenceFinder的BinaryFormatter弃用10条、序列化分析2条、AppDomain.GetAssemblies分析2条、热更Assembly.Load(byte[])分析1条、HtmlToUGUI旧对象查找API2条。本轮未修改这些C#。这些提示不妨碍本次编译通过，但尤其热更程序集加载兼容性不能凭0错误推定无风险，后续可单独诊断；不未经授权自动修复。

没有启动PlayMode、切换平台、运行iOS/IL2CPP构建或重启Editor；不宣称实际iOS本地化导出或热更运行已验收。桌面语音脚本仍因39555服务拒绝连接而失败，未改服务配置。两项授权工作已完成，其余项目保留。


