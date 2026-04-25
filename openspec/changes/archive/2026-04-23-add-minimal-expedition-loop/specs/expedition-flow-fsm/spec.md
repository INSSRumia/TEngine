## ADDED Requirements

### Requirement: 远征流程 SHALL 由独立 FSM 驱动
系统 SHALL 使用 TEngine FSM 为一次远征创建独立流程控制器，并由该流程控制器驱动远征各阶段状态迁移。

#### Scenario: 创建远征流程状态机
- **WHEN** 系统开始一次新的远征
- **THEN** 系统创建一个以 `ExpeditionFlowController` 为 owner 的 FSM
- **AND** 该 FSM 负责管理本次远征的阶段状态

### Requirement: 远征流程 SHALL 明确区分主要阶段状态
系统 SHALL 将最小远征流程划分为明确的阶段状态，至少覆盖准备、进入节点、事件、Combat、应用节点结果、结算和结束。

#### Scenario: 进入事件节点
- **WHEN** FSM 推进到一个 `EventNode`
- **THEN** 流程状态切换到事件处理阶段
- **AND** 系统打开事件界面并等待玩家输入

#### Scenario: 进入 Combat 节点
- **WHEN** FSM 推进到一个 `CombatNode`
- **THEN** 流程状态切换到 Combat 阶段
- **AND** 系统发起一次 Combat 会话

#### Scenario: 远征进入结算
- **WHEN** 最后一个节点处理完成
- **THEN** 流程状态切换到远征结算阶段
- **AND** 结算完成后切换到结束阶段

### Requirement: 状态迁移 SHALL 由明确事件触发
系统 SHALL 只在明确的流程事件到达后推进状态迁移，不允许 UI 或外部模块绕过流程控制器直接跳状态。

#### Scenario: 事件选项驱动迁移
- **WHEN** 玩家在事件阶段提交一个合法选项
- **THEN** 流程控制器接收该输入
- **AND** FSM 从事件阶段迁移到应用节点结果阶段

#### Scenario: Combat 结果驱动迁移
- **WHEN** Combat 模块返回一次 `CombatSessionResult`
- **THEN** 流程控制器接收该结果
- **AND** FSM 从 Combat 阶段迁移到应用节点结果阶段
