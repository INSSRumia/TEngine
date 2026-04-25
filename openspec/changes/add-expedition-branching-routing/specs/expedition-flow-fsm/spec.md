## MODIFIED Requirements

### Requirement: 远征流程 SHALL 明确区分主要阶段状态
系统 SHALL 将远征流程划分为明确的阶段状态，至少覆盖准备、进入节点、事件、Combat、应用节点结果、结算和结束。FSM 在节点结算后 MUST 根据当前节点的路由策略和运行时待执行节点队列决定进入哪个后续节点，而不是默认以固定索引推进到下一个节点。

#### Scenario: 进入事件节点
- **WHEN** FSM 推进到一个 `EventNode`
- **THEN** 流程状态切换到事件处理阶段
- **AND** 系统打开事件界面并等待玩家输入

#### Scenario: 进入 Combat 节点
- **WHEN** FSM 推进到一个 `CombatNode`
- **THEN** 流程状态切换到 Combat 阶段
- **AND** 系统发起一次 Combat 会话

#### Scenario: 远征进入结算
- **WHEN** 待执行节点队列已经没有剩余节点且当前流程满足结束条件
- **THEN** 流程状态切换到远征结算阶段
- **AND** 结算完成后切换到结束阶段

### Requirement: 状态迁移 SHALL 由明确事件触发
系统 SHALL 只在明确的流程事件到达后推进状态迁移，不允许 UI 或外部模块绕过流程控制器直接跳状态。对于按选项出口或按条件出口的节点，FSM MUST 在收到当前节点结果后解析对应路由决策，再决定下一步迁移目标。

#### Scenario: 事件选项驱动迁移
- **WHEN** 玩家在事件阶段提交一个合法选项
- **THEN** 流程控制器接收该输入
- **AND** FSM 从事件阶段迁移到应用节点结果阶段
- **AND** 节点结果应用完成后根据当前节点的路由策略确定后续节点

#### Scenario: Combat 结果驱动迁移
- **WHEN** Combat 模块返回一次 `CombatSessionResult`
- **THEN** 流程控制器接收该结果
- **AND** FSM 从 Combat 阶段迁移到应用节点结果阶段
- **AND** 节点结果应用完成后根据当前节点的路由策略确定后续节点
