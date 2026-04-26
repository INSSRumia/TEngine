## ADDED Requirements

### Requirement: 系统 SHALL 支持延迟插入节点 Expedition Effect
系统 MUST 支持一种新的 Expedition Effect，用于在经过指定数量的节点后，向本次远征运行时插入一个临时节点。该 Effect 的配置 MUST 至少包含 `passed_node_count` 与 `pending_node` 两部分，并且 `passed_node_count` 使用 `1` 表示“下一个节点就是插入节点”。

#### Scenario: Event 选项创建延迟插入节点请求
- **WHEN** 玩家选择了一个包含延迟插入节点 Effect 的事件选项
- **THEN** 系统执行该 Effect
- **AND** 系统向本次远征运行态登记一条新的待插入节点请求
- **AND** 该请求记录插入目标与剩余经过节点数

#### Scenario: Combat 胜负结果创建延迟插入节点请求
- **WHEN** 一场 Combat 节点的胜利或失败 Effect 列表中包含延迟插入节点 Effect
- **THEN** 系统执行该 Effect
- **AND** 系统向本次远征运行态登记一条新的待插入节点请求

### Requirement: 延迟插入节点 Effect SHALL 只接受 event 与 combat 两类临时节点
系统 MUST 让延迟插入节点 Effect 的 `pending_node.node_type` 仅支持 `event` 与 `combat`。当 `node_type = event` 时，`id` MUST 表示一个 `event_config_id`；当 `node_type = combat` 时，`id` MUST 表示一个 `combat_encounter_config_id`。

#### Scenario: 配置插入 event 临时节点
- **WHEN** 配置者把 `pending_node.node_type` 声明为 `event`
- **THEN** 系统将 `pending_node.id` 解释为 `event_config_id`
- **AND** 运行时生成的临时节点按事件节点方式执行

#### Scenario: 配置插入 combat 临时节点
- **WHEN** 配置者把 `pending_node.node_type` 声明为 `combat`
- **THEN** 系统将 `pending_node.id` 解释为 `combat_encounter_config_id`
- **AND** 运行时生成的临时节点按战斗节点方式执行
