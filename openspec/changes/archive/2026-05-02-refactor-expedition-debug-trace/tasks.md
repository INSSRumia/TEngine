## 1. 调试追踪模型整理

- [x] 1.1 新增 `ExpeditionDebugTrace` 及最小结构化 trace 条目类型，定义会话级调试信息所需的分类、级别、消息和上下文字段
- [x] 1.2 将 `ExpeditionRunState` 从 `List<string> DebugLogs` 重构为持有独立的 `ExpeditionDebugTrace`
- [x] 1.3 为 `ExpeditionDebugTrace` 设计统一记录入口，覆盖环境、随机池、延迟插入、Combat 和通用警告等主要会话级 trace 场景

## 2. 远征调用点迁移

- [x] 2.1 替换 `ExpeditionRunState`、`ExpeditionRunState.Queue`、`ExpeditionRunState.PendingInsert` 中直接 `DebugLogs.Add(...)` 的调用
- [x] 2.2 替换 `ExpeditionFlowController.Flow`、`ExpeditionFlowController.Combat` 中直接写入 `DebugLogs` 的调用，并按会话级 / 节点级边界重新分流
- [x] 2.3 替换 `ExpeditionEffectExecutionContext`、`ExpeditionEffectFactory` 中直接写入 `DebugLogs` 的调用，确保 Effect 诊断走统一追踪入口

## 3. 记录边界清理

- [x] 3.1 梳理当前同时写入 `DebugLogs` 与 `ExpeditionNodeRecord.LstRouteDecisionLog` 的诊断信息，删除无规则双写
- [x] 3.2 保持 `ExpeditionNodeRecord` 继续负责节点级结果记录，确认节点摘要、路由说明和插入节点记录不被迁入会话级调试类
- [x] 3.3 为调试追踪相关字段和方法补充必要注释，明确“会话级 trace”与“节点级 record”的职责边界

## 4. 验证与收尾

- [x] 4.1 编译验证远征相关代码，确认 `DebugLogs` 移除后无遗留调用和空引用问题
- [x] 4.2 手工检查远征主流程、随机事件、延迟插入、Combat 桥接几条关键路径的 trace 是否仍可完整追踪
- [x] 4.3 复核本次 change 范围，确认未误改玩家可见 `Summary`、结算显示逻辑、Luban schema 或 xlsx
