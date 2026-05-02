## Why

当前远征调试信息分散在 `ExpeditionRunState.DebugLogs`、`ExpeditionNodeRecord.LstRouteDecisionLog`、控制台 `Log.*` 以及部分 `Summary` 字段之间，职责边界不清，导致运行态类被调试字符串污染，也让后续阅读、排查和扩展都变得混乱。现在远征最小流程已经跑通，正适合在继续堆功能之前先把调试追踪结构收口，否则后续分支路由、环境、随机事件和 Effect 扩展都会继续放大这类混杂。

## What Changes

- 新增一套远征会话级调试追踪能力，用独立调试类承载运行时 trace，替代 `ExpeditionRunState` 直接持有 `DebugLogs` 字符串列表。
- 明确区分“会话级调试追踪”和“节点级执行记录”：
  - 会话级 trace 负责环境切换、随机池激活/移除、队列异常、Combat 构建异常等开发期追踪。
  - 节点级记录继续由 `ExpeditionNodeRecord` 承担，保留节点摘要、路由决定、插入节点记录等业务运行记录。
- 统一远征调试记录入口，禁止继续在远征运行态和流程代码中散落 `DebugLogs.Add(...)`。
- 将当前重复写入 `DebugLogs` 与 `LstRouteDecisionLog` 的诊断信息重新分流，避免相同信息同时落入会话级 trace 和节点级记录。
- 为后续进一步整理 `Summary`、`Reason`、控制台 `Log.*` 提供明确边界，但本次 change 不直接重构所有日志体系，也不修改玩家可见结算文本。

## Capabilities

### New Capabilities
- `expedition-debug-trace`: 定义远征会话级调试追踪的结构、记录边界和与节点记录的职责分层。

### Modified Capabilities
- 无

## Impact

- 主要影响远征运行态、流程控制器、Effect 执行上下文及相关调试记录调用点。
- 受影响代码预计集中在 `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/**`。
- 不涉及 Luban schema、xlsx、配置生成代码和玩家可见 UI 文案协议变更。
