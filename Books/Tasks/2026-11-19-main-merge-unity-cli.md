# main 合并及 Unity CLI 接入检查

- ID: 20261119-main-merge-unity-cli
- 状态: complete（合并与安装方案调查；运行验证仍有下述阻塞）
- 负责人: lead

## 目标与验收

- 将 fork 已同步的 main 合并到 Marbles_Legion，保留游戏业务、URP 和导航依赖。
- 无未解决冲突；生成用户明确授权的本地合并提交，不推送。
- 核对合并后的 CLI 安装路径、版本和前置条件；本次不自动安装工具。

## 基线与授权

- 合并前 HEAD: 747e00a84814cea40e6e4ac0bb64e6ba4cf1a815。
- main: 656daac76a44c5a9f78059f4d14fb27dc50b1316；通过 git fetch origin main:main 从用户 fork 获取。
- 回退参考分支: backup/marbles-before-main-20261119-1655。
- 用户要求合并 main，并在提问确认中选择“按建议合并并提交”，明确允许 main 的 HybridCLR、2D/IDE/JSON 更新及移除旧 MCP，同时保留 URP 14.0.12、AI Navigation 1.1.6；不推送、暂不安装 CLI。
- 用户已保存并关闭 Unity Editor。关闭时产生的个人布局改动保留在工作区，不纳入提交。

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

### 阻塞 / 未验证

- `verify --profile code`：文档和 58 个 unittest 通过；solution-build 为 blocked。新增 SpriteResourceLifecycleTests.cs、SpriteTestSceneState.cs 未在旧的 Unity 生成工程中，需要 Editor 导入并重新生成工程后验证。
  证据：[code 报告](../../UnityProject/.codex/runs/20261119T090224Z-a30c0761/report.md)。
- `doctor`：blocked，缺少 UnityProject/Tools/unity.exe；没有执行 Unity 连接、真实 C# 编译或 Unity 生命周期测试。
  证据：[doctor 报告](../../UnityProject/.codex/runs/20261119T090040Z-92817a2b/report.md)。
- UPM 实际解析、Editor 导入、游戏场景回归均未执行，不能表述为 Unity 验收通过。

## CLI 安装建议（尚未执行，需另行授权）

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

## 文档差异与剩余风险

- 工作流硬编码 CLI 为 UnityProject/Tools/unity.exe；仓库合并不等于该二进制已经安装。
- 包 README 仍写 Unity 6+，工作流记录 Editor f1；实际工程版本为 2022.3.62f3，嵌入 Pipeline 元数据和兼容源码以 2022.3 为目标。
- 不执行 unity pipeline install，以免覆盖 TEngine 的 2022 兼容嵌入包。
- main 包含大量框架和包更新，静态合并完成不能替代真实导入、编译和游戏回归。
