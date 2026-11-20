# main 合并及 Unity CLI 接入检查

- ID: 20261119-main-merge-unity-cli
- 状态: complete（合并、安装与包装器修复完成；离线测试和当前工程只读验收通过）
- 负责人: lead

## 目标与验收

- 将 fork 已同步的 main 合并到 Marbles_Legion，保留游戏业务、URP 和导航依赖。
- 无未解决冲突；生成用户明确授权的本地合并提交，不推送。
- 合并阶段仅核对 CLI 安装路径、版本和前置条件；用户后续明确授权安装后，执行项目内安装、完整性校验及只读连接验收。

## 基线与授权

- 合并前 HEAD: 747e00a84814cea40e6e4ac0bb64e6ba4cf1a815。
- main: 656daac76a44c5a9f78059f4d14fb27dc50b1316；通过 git fetch origin main:main 从用户 fork 获取。
- 回退参考分支: backup/marbles-before-main-20261119-1655。
- 用户要求合并 main，并在提问确认中选择“按建议合并并提交”，明确允许 main 的 HybridCLR、2D/IDE/JSON 更新及移除旧 MCP，同时保留 URP 14.0.12、AI Navigation 1.1.6；不推送、暂不安装 CLI。
- 合并时用户已保存并关闭 Unity Editor。关闭时产生的个人布局改动保留在工作区，不纳入提交。
- 合并已提交为 c1d2da145efae74d9dfc0a52f6c9417aae5df0d7，双亲为原 HEAD 和 main，未推送。
- 后续用户明确要求“将本次合并提交，提交后开始安装unitycli”，允许必要时使用系统代理 7890 端口。
  据此安装项目内固定版本并作只读验收；不修改全局 PATH/技能，不替换 Pipeline，不覆盖已有文件，不擅自修改 Unity 工程。
- 安装开始时检测到 Unity 进程 PID 51588；不假定是本工程，须通过 CLI 核对项目路径。

## 冲突取舍

- 根目录旧技能有 53 个仅迁移目录、内容未改的冲突文件：按 main 删除过期副本，项目维护技能以 UnityProject/.codex/skills 为准。
- 根目录四个有本地文本修改的旧 OpenSpec 技能保留，不作为本项目的执行依赖，不运行旧工作流。其他游戏专用技能保留。
- 依赖清单采用 main 版本，额外保留原项目 URP 和导航包；锁文件合并两者的依赖图，避免将 main 的非 URP 项目依赖图整体覆盖进游戏。
- 资源扩展冲突由三方源码审查确认：main 包含 HEAD 的取消、过期请求、资源回收修复，同时增加子图资源与销毁保护，使用完整 main 实现避免接口混用。
- 不修改游戏业务源码、场景、Prefab 或资源 GUID；不自动启动 Editor 触发导入，不安装/更新工具。

## 验证记录

### 通过

- 未解决冲突清零；完整暂存差异按目录、依赖、项目设置和业务范围审查，Assets/GameScripts 与 Configs 没有暂存变更。
- 包 JSON 无重复键，manifest 与 lock 的直接依赖/版本一致，61 个锁定包依赖节点齐全且 depth 与最短依赖路径一致。
- 导航、URP/Core/Config、ShaderGraph、Searcher 的锁条目与原 HEAD 完全相同；Pipeline 为 embedded，MCP 已移除。
- 两个资源冲突文件与 main blob 完全一致；关联共八个文件经 C# 9 Roslyn 语法解析无错误（不是类型检查或运行测试）。
- `python UnityProject/.codex/scripts/workflow.py check`：39 份文档、4 个维护技能、21 个行为场景及 58 个 unittest 通过。
  证据：[check 报告](../../UnityProject/.codex/runs/20261119T090223Z-5d0ad831/report.md)（本机忽略的运行产物，不纳入 Git）。
- 全量 `git diff --cached --check` 返回 2，有上游空白告警；逐一比较这 60 个文件的 index/main blob，全部完全一致，本轮不全仓格式化。
  最初按 diff 文本比较受重命名显示影响，产生误报；改为上述 blob 比较后确认均来自 main。
- 修正 main 带来的两处断链和资源说明漂移：引用真实 HtmlToUGUIBaker，明确资产校验自定义命令缺实现，并删除子图仍使用旧请求机制的过时结论。

### 合并阶段的阻塞 / 未验证（后续 CLI 安装结果见下节）

- `verify --profile code`：文档和 58 个 unittest 通过；solution-build 为 blocked。新增 SpriteResourceLifecycleTests.cs、SpriteTestSceneState.cs 未在旧的 Unity 生成工程中，需要 Editor 导入并重新生成工程后验证。
  证据：[code 报告](../../UnityProject/.codex/runs/20261119T090224Z-a30c0761/report.md)。
- `doctor`：blocked，缺少 UnityProject/Tools/unity.exe；没有执行 Unity 连接、真实 C# 编译或 Unity 生命周期测试。
  证据：[doctor 报告](../../UnityProject/.codex/runs/20261119T090040Z-92817a2b/report.md)。
- UPM 实际解析、Editor 导入、游戏场景回归均未执行，不能表述为 Unity 验收通过。

## CLI 安装方案（合并后已另行取得安装授权）

- 为复现工作流文档基线，先使用官方固定 1.0.0-beta.3，而不是浮动 beta；这不代表协议已通过本机连接验证。
- [官方固定版本清单](https://public-cdn.cloud.unity3d.com/hub/prod/cli/1.0.0-beta.3/latest.json)
- [Windows x64 单文件下载](https://public-cdn.cloud.unity3d.com/hub/prod/cli/1.0.0-beta.3/unity-windows-x64.exe)
- 文件大小 106366048 字节；SHA256：`ff9ef81ade1063041d25e2c549cc7ed14e96d446f4204400bf101b389f7b8502`。
- 下载后校验，再放到 `UnityProject/Tools/unity.exe`；已有同名文件时先停下，不能盲目覆盖。
- 不编辑用户 PATH，不安装全局技能，不替换现有嵌入 Pipeline。
- [官方安装器](https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1) 会修改用户 PATH、写 UnityHub 安装记录，默认还会刷新插件及执行全局 skill 安装；仅指定项目 HOME 不能避免这些副作用。
- 审计时 stable/beta 的 latest manifest 都指向 1.0.1，因此旧文档的 beta 安装命令不再固定到 beta3。
- 本机已有 Python 3.13.1、Git 2.47.1、.NET SDK 9.0.321/10.0.404，无需为下载 CLI 额外安装这些环境。

安装授权后的验收顺序（以下从 UnityProject 运行）：

```powershell
./Tools/unity.exe --version
./Tools/unity.exe --help
python .codex/scripts/workflow.py doctor
python .codex/scripts/workflow.py unity list
python .codex/scripts/workflow.py unity describe "scene list_open"
python .codex/scripts/workflow.py unity query "scene list_open"
```

先由用户打开本工程，等待包解析/导入完成；确认唯一实例与 Editor 状态。重新生成工程后再进行 code 验证，
编译和资源生命周期测试需串行执行。当前缺失的 `tengine_validate_assets`、`tengine_bake_ugui` 属于项目自定义命令，
安装 CLI 本身不会补出这些 C# 实现；不能把整个 full 工作流描述为已就绪。

## 后续安装实绩（2026-11-19 17:09—17:13，Asia/Shanghai）

- 已复核合并提交 c1d2da14 的父提交和 main 祖先关系，不重复提交、不推送。
- 直连官方下载成功（HTTP 200，106366048 字节，约 4.63 秒），未使用代理；若需要代理，仅限用户指定的 127.0.0.1:7890。
- 下载先存入被忽略的 `.codex/runs/20261119-cli-install-beta3`，官方 SHA256 校验一致后才移动到 `UnityProject/Tools/unity.exe`；安装前目标不存在，未覆盖。
- `--version` 输出 `1.0.0-beta.3`，`--help` 正常；没有运行官方安装器、pipeline install、全局 skill 安装，也未更改 PATH、代理持久配置或兼容 Pipeline。
- `.git/info/exclude` 仅增加本机 `UnityProject/Tools/unity.exe` 排除项，避免将约 106 MB 工具提交到 Git；不改变共享 gitignore。
- 原生链路验收通过：[只读验收报告](../../UnityProject/.codex/runs/20261119T091257Z-52880d1d/report.md)。
  - 完整性校验、唯一工程实例绑定、editor status、命令目录与 scene schema、场景查询共 5 项通过。
  - 精确匹配本工程 `D:\UnityProject\Marbles Legion\UnityProject`，PID 51588，Editor 2022.3.62f3，服务端口 7800。
  - Editor ready，compiling=false，domainReloadInProgress=false，Play Mode stopped。
  - 实际发现 179 个命令；当前 main 场景已加载且激活、无未保存修改、3 个根对象；未执行任何资源/场景写入或测试。
- 包装器验收未通过，不混同为安装失败：
  - [doctor 失败证据](../../UnityProject/.codex/runs/20261119T091122Z-7635792c/report.md)：environment 通过，但拆分 `editor status` 导致命令不存在。
  - [后置选项失败证据](../../UnityProject/.codex/runs/20261119T091232Z-30df2101/report.md)：即使全名作为单参数，包装器后置的全局选项仍被错误转发为业务参数。
  - beta3 必须用 `command "editor status"`，且 --json/--non-interactive/--no-log-proxy 必须位于 command 之前。
  - 本任务未扩展为 Python 包装器改造；已修正文档的错误调用方式并提示实际限制。
- 未运行 .NET 构建、Unity 编译、资源生命周期测试或游戏回归；本次只完成 CLI 安装和只读链路验收。

当前可用调用（从 UnityProject 执行；先绑定唯一工程实例）：

```powershell
./Tools/unity.exe --json --non-interactive --no-log-proxy status --project-path .
./Tools/unity.exe --json --non-interactive --no-log-proxy command --project-path . "editor status"
./Tools/unity.exe --json --non-interactive --no-log-proxy command --project-path . "pipeline list_commands" --detail groups
./Tools/unity.exe --json --non-interactive --no-log-proxy command --project-path . "scene list_open"
```

## 包装器修复（用户后续请求）

- 授权：用户要求“请你修复”，并提供 Editor 中 ExecuteCommandByName 找不到 editor 命令的错误堆栈。
- 目标：doctor、命令发现、schema 和已审核只读查询通过固定版 CLI 正常执行，不再发送只有 editor 的错误命令。
- 范围：修复 Unity.raw 的全局选项位置，ready/discover/call 及 Player runtime_status 的完整命令单参数传递；补充实际 argv 回归测试，更新安装阶段的暂时故障说明。
- 不修改：嵌入 Pipeline C#、依赖、业务/场景/Prefab、个人布局和全局环境；不清空 Console、不启动编译测试/Player、不提交推送。
- 旧测试错误地断言必须拆分 group/name；本次改为验证 CLI 实际要求的完整命令单参数和全局选项前置。
- 编辑器堆栈位置 CommandNotFound 会主动 Debug.LogError，解释了错误请求为何留在 Console；不应通过删除/降级这条日志来掩盖错误。
- 实现：Unity.raw 前置 --format json / --non-interactive / --no-log-proxy；ready、discover、call、runtime_status 全部使用单参数完整命令，业务参数仍保留原 schema 键名和 JSON 序列化。未放宽白名单、工程绑定、审批或超时不重放规则。
- 红绿验证：新增/修正用例先针对旧实现执行，Pipeline 契约测试 60 个中 9 个按预期失败；实施修复后，check 的总计 63 个 unittest 全部通过（比原来新增 5 个），39 份文档与 21 个行为场景检查通过。
  证据：[修复后 check](../../UnityProject/.codex/runs/20261119T091900Z-a4389e63/report.md)、[文档更新后的最终 check](../../UnityProject/.codex/runs/20261119T092120Z-521172a2/report.md)。
- 当前工程实测（均为包装器入口，串行执行）：
  - [doctor](../../UnityProject/.codex/runs/20261119T091900Z-16aa3ba3/report.md)：environment/editor-connection 均 passed，精确绑定本工程、PID 51588、Unity 2022.3.62f3、CLI beta.3、Pipeline 0.10.0-exp.1。
  - [unity list](../../UnityProject/.codex/runs/20261119T091924Z-04b2120d/report.md)：179 个命令。
  - [scene schema](../../UnityProject/.codex/runs/20261119T091925Z-1d3e5a3e/report.md) 和 [console schema](../../UnityProject/.codex/runs/20261119T091926Z-b5030191/report.md)：均 passed。
  - [editor status](../../UnityProject/.codex/runs/20261119T091927Z-6f4b14fa/report.md)、[scene list_open](../../UnityProject/.codex/runs/20261119T091930Z-6392b906/report.md)、[console status](../../UnityProject/.codex/runs/20261119T091933Z-59f84e6b/report.md)：均 passed，场景仍未修改、Editor 空闲且未在 Play。
- [Console 游标验收](../../UnityProject/.codex/runs/20261119T092019Z-8019385b/report.md)：
  - 唯一保留的 error 即用户所贴错误，seq=3、时间为 2026-11-19 17:11:24.142（Asia/Shanghai），与安装阶段旧 doctor 的失败请求对应。
  - 再执行 editor/scene 查询后，以同 session 和 since=3 查询 error，entries=[]、returned=0、reset=false、dropped=false：此验证区间没有新增 Console 错误；没有清空历史日志。
  - 此实测同时覆盖带 level/tail/since/since-session 业务参数的传递。
- 独立只读审查未发现遗漏调用或新增安全回归；手工复核最终 diff，范围内 git diff --check 通过。
- 限制：未连接真实 Player（runtime_status 仅离线回归）、未触发 C# 编译/资源写入/构建或游戏回归；历史失败报告保留，不重新标记为通过。本次修复未提交或推送。

## 文档差异与剩余风险

- 工作流硬编码 CLI 为 UnityProject/Tools/unity.exe；仓库合并不等于该二进制已经安装。
- 包 README 仍写 Unity 6+，工作流记录 Editor f1；实际工程版本为 2022.3.62f3，嵌入 Pipeline 元数据和兼容源码以 2022.3 为目标。
- 不执行 unity pipeline install，以免覆盖 TEngine 的 2022 兼容嵌入包。
- main 包含大量框架和包更新，静态合并完成不能替代真实导入、编译和游戏回归。
