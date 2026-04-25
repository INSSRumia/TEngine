## 1. 远征数据骨架

- [x] 1.1 创建 `Gameplay/Expedition/` 目录下的基础运行时数据类型，包括 `ExpeditionRunState`、`ExpeditionNodeConfig`、`ExpeditionNodeRecord`
- [x] 1.2 创建 Marble 局外持久化数据类型，包括 `MarblePersistentData` 与 `MarblePersistentDataSnapshot`
- [x] 1.3 定义最小远征所需的枚举与基础值对象，包括节点类型、流程阶段、远征结束原因和节点处理状态
- [x] 1.4 建立一条最小静态远征路线定义，至少支持 `EventNode -> CombatNode`

## 2. 远征流程状态机

- [x] 2.1 创建 `ExpeditionFlowController`，作为 TEngine FSM 的 owner，负责持有当前 `ExpeditionRunState`
- [x] 2.2 实现远征流程基础状态类与状态集合，包括 Prepare、EnterNode、Event、Combat、ApplyNodeResult、Settlement、Finished
- [x] 2.3 实现从入口启动远征并创建 FSM 的流程
- [x] 2.4 实现基于事件选择和 Combat 结果驱动的状态迁移，确保流程只能通过控制器推进

## 3. Combat 会话桥接

- [x] 3.1 定义 `CombatSessionRequest` 与 `CombatSessionResult` 数据包
- [x] 3.2 实现从远征内 Marble 快照和节点配置生成 `CombatSessionRequest` 的逻辑
- [x] 3.3 实现 Combat 结束后将 `CombatSessionResult` 回写到 `ExpeditionNodeRecord` 的逻辑
- [x] 3.4 实现使用 `CombatSessionResult` 更新远征内 Marble 快照的逻辑，避免直接依赖 Combat 场景对象结算

## 4. 事件与结算处理

- [x] 4.1 实现最小事件节点的数据结构与选项结果应用逻辑
- [x] 4.2 实现事件节点结果写入 `ExpeditionNodeRecord` 的逻辑
- [x] 4.3 实现远征结算逻辑，将 Marble 快照最终状态和资源收益回写到局外持久化数据
- [x] 4.4 实现远征结束后的结果摘要对象，供入口界面和结算界面展示

## 5. UI 串联

- [x] 5.1 按 TEngine `UIWindow/UIModule` 流程为 `ExpeditionMainUI` 提供最小入口与出征准备能力，使用标准生命周期与节点绑定方式
- [x] 5.2 按 TEngine `UIWindow/UIModule` 流程为 `EventCardUI` 提供事件输入能力，并将选项结果提交给 `ExpeditionFlowController`
- [x] 5.3 按 TEngine `UIWindow/UIModule` 流程为 `ExpeditionResultUI` 提供远征完成后的结果展示能力
- [x] 5.4 为当前阶段准备功能型占位 UI，保证节点命名、交互与流程可验证，不以美术资源齐备为前提
- [x] 5.5 如需创建或修改 Unity 中的 UI Prefab、Canvas 节点、组件挂载或场景层级，统一通过 Unity MCP 完成
- [x] 5.6 将现有测试入口替换为“进入最小远征流程”的入口，同时保留 Combat 调试后门

## 6. 验证与收尾

- [ ] 6.1 验证玩家可以从入口发起一次最小远征并完成 `EventNode -> CombatNode -> Settlement -> Entry` 闭环
- [ ] 6.2 验证 `ExpeditionNodeRecord` 会正确记录事件选择结果与 Combat 结果
- [ ] 6.3 验证远征结算后 Marble 局外持久化数据被正确回写，未参战 Marble 不受影响
- [ ] 6.4 验证远征 UI 符合 TEngine `UIWindow` 接入方式，未引入绕开 UIModule 的临时 UI 控制模式
- [x] 6.5 更新相关 AI/开发文档，补充远征流程、命名约定、TE UI 约束、Unity MCP 约束与 Combat 边界说明
