## 1. 分支远征 schema 设计

- [x] 1.1 为远征节点新增路由策略配置，至少支持固定出口、按选项出口、按条件出口
- [x] 1.2 为远征节点新增 Transition、OptionId 到 TransitionId 的映射配置
- [x] 1.3 为远征分支新增条件配置类型，并定义黑板可读取的状态字段
- [x] 1.4 保持 Event 只定义内容和选项，不在 Event 模板中写节点级出口语义

## 2. 运行态结构升级

- [x] 2.1 在 `ExpeditionRunState` 中新增远征黑板结构，用于记录 flags、道具、历史选择和计数器
- [x] 2.2 将远征推进从固定索引改为待执行节点队列
- [x] 2.3 补充节点记录与调试信息，确保分支决策、黑板变化和队列变化可追踪

## 3. 节点路由与流程控制实现

- [x] 3.1 在远征流程控制器中接入节点级路由策略解析
- [x] 3.2 实现按选项出口的路由决策，使用稳定的 `OptionId` 映射，不使用 option index
- [x] 3.3 实现按条件出口的路由决策，从远征黑板读取条件状态
- [x] 3.4 调整 FSM 推进逻辑，使节点结算后根据队列和路由决策进入后续节点

## 4. 动态插入节点能力

- [x] 4.1 为运行时增加向待执行节点队列插入事件或支线节点的能力
- [x] 4.2 明确动态插入与普通路由的执行顺序，避免插入逻辑与出口选择相互冲突
- [ ] 4.3 验证前序选择触发后段插入事件的典型场景可被稳定表达

## 5. 配置与数据协作边界

- [x] 5.1 梳理本次 schema 变更需要用户手工修改的 `xlsx` 表结构和数据清单
- [x] 5.2 明确通知用户手工修改相关 `xlsx` 与注册项，agent 不自行编辑任何 `xlsx`
- [x] 5.3 在用户完成改表后再执行配置生成和联调步骤

### 手工改表清单

- `Configs/GameConfig/Datas/expedition.xlsx`
  - 将线性 `route` 数据改为节点图数据，补充 `route_policy`、`default_transition_id`、`transitions`、`option_routes`
  - 为每个节点补齐 `transition_id -> target_node_id` 关系，以及按 `OptionId` 的出口映射
  - 为条件分支补充黑板条件数据，至少覆盖 `flag`、`item`、`chosen_option`、`counter_at_least`
- `Configs/GameConfig/Datas/expedition_event.xlsx`
  - 保持 Event 只定义内容和选项，不新增节点级出口字段
  - 校对 `option_id`，确保与 `expedition.xlsx` 中的 `option_routes` 引用一致
- `Configs/GameConfig/Datas/expedition_combat_encounter.xlsx`
  - 当前分支路由 schema 无必需结构变更
  - 如主路/支线节点引用新的战斗遭遇，由用户手工补齐对应数据
- `Configs/GameConfig/Datas/__tables__.xlsx`
  - 本次 schema 没有新增 Expedition 主表，通常不需要新增注册项
  - 若你的本地 Luban 流程仍要求显式维护相关 sheet 注册，请由用户手工核对

### 当前阻塞说明

- 用户已完成 `xlsx` 手工修改，Luban 配置生成已执行完成
- 当前不再受 `xlsx` 步骤阻塞；剩余未勾选项主要是运行时验证与回归验证任务

## 6. 验证与回归

- [ ] 6.1 验证同一个 Event 在不同 Node 中复用时可以拥有不同出口策略
- [ ] 6.2 验证按选项分支、按黑板条件分支和固定出口三种模式都可正确推进
- [ ] 6.3 验证动态插入事件不会破坏最小远征闭环
- [ ] 6.4 进行编译与基础流程验证，确认分支远征模型与现有远征系统兼容
