## ADDED Requirements

### Requirement: Luban 配置 SHALL 定义延迟插入节点 Effect 结构
系统 MUST 在远征 Effect 的 Luban schema 中提供延迟插入节点 Effect 配置结构。该结构 MUST 至少包含 `passed_node_count` 与 `pending_node` 两部分，并明确 `passed_node_count` 使用 `1` 表示“下一个节点插入”。

#### Scenario: 配置延迟插入节点 Effect
- **WHEN** 配置者在事件选项或 Combat 胜负 Effect 列表中声明一个延迟插入节点 Effect
- **THEN** 该配置能够声明插入前需要经过的节点数量
- **AND** 该配置能够声明要插入的简单节点信息

### Requirement: Luban 配置 SHALL 定义简单临时节点描述
系统 MUST 在远征 schema 中提供一个可复用的简单临时节点描述结构，至少包含 `node_type` 与 `id`。该结构 MUST 仅支持 `event` 与 `combat` 两种节点类型。

#### Scenario: 配置插入 event 节点
- **WHEN** 配置者把简单节点描述的 `node_type` 声明为 `event`
- **THEN** 该结构允许填写一个 `event_config_id`
- **AND** 不要求额外配置 transition 或 option_routes

#### Scenario: 配置插入 combat 节点
- **WHEN** 配置者把简单节点描述的 `node_type` 声明为 `combat`
- **THEN** 该结构允许填写一个 `combat_encounter_config_id`
- **AND** 不要求额外配置 transition 或 option_routes

### Requirement: Agent SHALL NOT 修改 xlsx 表格
实现该变更的 agent MUST NOT 创建、编辑、填充或修改任何 `xlsx` 表格。若 schema 变更需要表格新增 sheet、列或数据，agent MUST 暂停并通知用户手工修改。

#### Scenario: schema 变更需要表格配合
- **WHEN** 实现者修改 Luban xml schema 后发现需要更新 `xlsx` 内容
- **THEN** 实现者停止继续依赖生成代码的实现工作
- **AND** 实现者向用户列出需要修改的表格、sheet 和字段
- **AND** 等待用户修改表格并重新生成代码后再继续
