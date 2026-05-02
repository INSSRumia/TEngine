## Context

当前远征代码把会话级调试追踪直接挂在 `ExpeditionRunState.DebugLogs` 上，并以 `List<string>` 的形式在多个文件中直接追加字符串。与此同时，`ExpeditionNodeRecord.LstRouteDecisionLog` 也在承担部分节点级诊断说明，导致同一条诊断信息可能同时写入运行态字符串列表和节点记录。  

这带来三个直接问题：

- `ExpeditionRunState` 同时承担业务运行态与调试缓存职责，类边界被持续污染。
- 会话级调试信息与节点级执行记录没有清晰分层，后续继续加环境、随机事件、分支路由时会进一步混乱。
- 裸字符串日志缺少分类、级别和上下文，后续无法稳定地筛选、导出或在 UI 中按层级展示。

当前已经确认的约束如下：

- 本次 change 采用“方案 2”：`ExpeditionRunState` 持有独立的 `ExpeditionDebugTrace` 类型。
- `ExpeditionNodeRecord` 继续负责节点级运行记录，不被整体迁入调试类。
- 本次 change 只处理远征内部调试追踪分层，不重构全部 `Summary`、`Reason` 或控制台 `Log.*` 体系。
- 不涉及 Luban schema、xlsx、配置表生成或玩家可见结算行为变更。

## Goals / Non-Goals

**Goals:**

- 将 `ExpeditionRunState.DebugLogs` 从字符串列表重构为独立的会话级调试追踪对象。
- 为远征调试追踪建立统一的记录入口，停止在流程代码中散落 `DebugLogs.Add(...)`。
- 明确分离会话级 trace 与节点级 record，避免同类诊断在两个体系中重复落盘。
- 让调试追踪结构具备基础的结构化字段，为后续筛选、展示和导出预留空间。

**Non-Goals:**

- 不重命名或移除全部 `Summary` 字段。
- 不把 `ExpeditionNodeRecord` 改造成纯调试对象。
- 不统一整个项目的 `TEngine.Log.*` 策略，也不处理 Combat 全局日志收口。
- 不新增配置表、不开启新的持久化格式、不修改 xlsx。

## Decisions

### 决策 1：采用 `ExpeditionRunState -> ExpeditionDebugTrace` 的持有关系

`ExpeditionRunState` 将继续持有一份与本次远征生命周期一致的调试追踪对象，但不再直接暴露 `List<string> DebugLogs`。

原因：

- 调试信息仍然天然属于“本次远征实例”的上下文，不适合抽成跨实例全局单例。
- 继续挂在 `RunState` 下可以保留现有调用路径中的上下文获取便利性，同时把实现细节收口到独立类。

备选方案：

- 全局 `ExpeditionDebugService`：过重，需要额外管理实例边界、上下文绑定和清理时机，不适合当前阶段。
- 保留 `List<string>` 仅改名：无法解决结构化和职责混乱问题。

### 决策 2：会话级 trace 与节点级 record 明确分层

`ExpeditionDebugTrace` 只负责会话级追踪，例如：

- 环境切换
- 随机池激活 / 移除
- 队列异常
- Combat 构建失败
- Effect 诊断缺失

`ExpeditionNodeRecord` 继续负责节点级运行记录，例如：

- 节点 `Summary`
- `ChosenOptionId`
- `ResolvedTransitionId`
- `LstRouteDecisionLog`
- `LstInsertedNodeId`

原因：

- 节点记录本身已经服务于结算摘要与节点执行回顾，不能因为“带解释文字”就整体视为调试对象。
- 会话级 trace 和节点级 record 的消费方不同；前者偏开发追踪，后者偏运行结果。

备选方案：

- 所有调试与解释信息统一迁入 Debug 类：会破坏节点记录的业务用途，并提高阅读与结算组装成本。

### 决策 3：`ExpeditionDebugTrace` 内部使用结构化条目，而不是新的字符串列表

调试追踪条目至少应包含以下信息：

- `Category`
- `Severity`
- `Message`
- `NodeConfigId`
- `QueueEntryInstId`
- `Phase`

原因：

- 当前字符串前缀如 `[环境]`、`[随机事件池]`、`[延迟插入]` 已经在隐式表达分类，只是没有正式结构。
- 如果仍然只是 `List<string>`，只是把污染从 `RunState` 挪到新类，无法形成真正可扩展边界。

备选方案：

- 新类里继续只存 `List<string>`：实现最省事，但会把问题整体延后，不值得专门开 change。

### 决策 4：统一记录入口，但不强行做成一个万能方法

调试类应提供清晰的记录入口，例如：

- `RecordEnvironment(...)`
- `RecordRandomEventPool(...)`
- `RecordPendingInsert(...)`
- `RecordCombat(...)`
- `RecordWarning(...)`

而不是只提供一个 `Add(string message)`。

原因：

- 从调用点名字就能看出信息属于哪一类，会显著提高流程代码可读性。
- 统一入口的同时保留语义边界，避免新类退化为“另一个字符串垃圾袋”。

备选方案：

- 只提供 `Add(...)`：虽然表面统一了入口，但语义仍然散乱，后续很快会失控。

### 决策 5：重复诊断按“归属唯一”原则分流

本次 change 将对当前重复写入两处的信息做边界重分配：

- 会话级异常进入 `ExpeditionDebugTrace`
- 节点执行解释进入 `ExpeditionNodeRecord`

只有当一条信息同时对“会话诊断”和“节点回顾”都不可或缺时，才允许双写，并且必须有明确规则。

原因：

- 当前 `Summary token 缺失` 一类信息同时写入两套容器，后续阅读很难判断哪一处才是权威来源。

## Risks / Trade-offs

- [风险] 调试类抽出后，调用点可能短期内变多，看起来“更啰嗦”  
  → Mitigation：使用语义化方法名，把啰嗦换成可读性；必要时再在类内做轻量封装。

- [风险] 如果结构化字段设计过度，会让当前简单日志场景实现成本上升  
  → Mitigation：只保留最小必要字段，不一次性引入导出、过滤器、UI 展示等高级能力。

- [风险] 会话级 trace 和节点级 record 的边界仍可能被误用  
  → Mitigation：在 design、spec 和 tasks 中都明确“哪些信息属于哪个容器”，实现时按清单逐项迁移。

- [风险] 本次只整理远征内部 trace，用户仍然会在项目其他模块看到旧式 `Log.*`  
  → Mitigation：明确本 change 范围只覆盖远征调试追踪，为后续全局日志治理保留独立 change 空间。

## Migration Plan

- 第一步：新增 `ExpeditionDebugTrace` 及其最小结构化条目类型。
- 第二步：将 `ExpeditionRunState` 从 `DebugLogs` 切换为 `DebugTrace` 持有关系。
- 第三步：逐步替换远征内部 `DebugLogs.Add(...)` 调用为统一记录入口。
- 第四步：清理当前重复写入 `DebugLogs` 与 `LstRouteDecisionLog` 的诊断信息，按归属重新分流。
- 第五步：完成编译验证，并人工检查远征主流程、随机事件、延迟插入、Combat 桥接几条主要路径的 trace 是否仍可追踪。

本次 change 为纯代码内重构，不涉及线上数据迁移；若实现中发现旧调用点暂未迁完，可在同一 change 内继续完成，不需要额外兼容层。

## Open Questions

- 当前阶段是否需要让 `ExpeditionDebugTrace` 直接支持 UI 展示，还是只作为内部运行时调试容器保留？
- `Severity` 是否只保留 `Info / Warning / Error` 三档即可，还是需要和 `TEngine.Log` 的级别进一步对齐？
