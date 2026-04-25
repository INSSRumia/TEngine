## MODIFIED Requirements

### Requirement: 事件配置 SHALL 使用 Effect 列表
系统 MUST 让远征事件选项的效果通过配置化的 `LstEffect` 表达，而不是继续依赖固定的 `crystal_delta`、`exp_delta`、`hp_delta` 这类字段。首版至少能够通过 Effect 列表表达添加金钱、为玩家全队添加经验和为玩家全队修改血量这 3 类结果。

#### Scenario: 事件选项配置多个 Effect
- **WHEN** 运行时读取某个事件选项的配置
- **THEN** 该选项通过 `LstEffect` 提供一个或多个 Expedition Effect 配置
- **AND** 不要求通过固定数值字段才能表达首版远征事件结果

### Requirement: Combat 遭遇配置 SHALL 提供敌方编队与胜负效果列表
系统 MUST 让远征 Combat 遭遇配置显式提供敌方 Marble 列表、标题描述以及两组结算效果列表：战斗胜利时触发的 `LstVictoryEffect` 与战斗失败时触发的 `LstDefeatEffect`。首版固定胜利奖励字段不再作为唯一表达方式。

#### Scenario: 从遭遇配置构建 Combat 输入与胜负结果
- **WHEN** 远征流程进入一个 Combat 节点
- **THEN** 系统根据节点引用读取对应的 Combat 遭遇配置
- **AND** 该配置能够提供敌方 Marble 列表
- **AND** 该配置能够根据 Combat 结算结果提供对应的胜利或失败 Effect 列表

## ADDED Requirements

### Requirement: Luban 配置 SHALL 定义随机事件池
系统 MUST 在 Luban schema 中提供随机事件池配置结构，用于声明随机事件池 Id、说明信息和带权重的事件条目列表。

#### Scenario: 配置随机事件池条目
- **WHEN** 配置者定义一个随机事件池
- **THEN** 该池能够配置多个事件条目
- **AND** 每个事件条目能够引用一个 Event 配置并声明权重

### Requirement: Luban 配置 SHALL 定义环境
系统 MUST 在 Luban schema 中提供环境配置结构，用于声明环境 Id、说明信息、环境随机事件池列表和环境场地候选列表。

#### Scenario: 配置环境内容
- **WHEN** 配置者定义一个环境
- **THEN** 该环境能够引用多个随机事件池
- **AND** 该环境能够配置多个带权重的场地候选

### Requirement: Luban 配置 SHALL 定义 Combat 场地
系统 MUST 在 Luban schema 的 `Gameplay.Combat` 命名空间中提供 Combat 场地配置结构，用于声明场地 Id、名称和描述。Combat 场地 prefab 路径不在表格中配置，而是由 Combat 层按 `battlefield_config_id` 在 `Assets/AssetRaw/Actor/Prefabs/Battlefield` 目录下查找同名 prefab。远征 schema 只引用场地配置 Id。

#### Scenario: 配置 Combat 场地资源
- **WHEN** 配置者定义一个 Combat 场地
- **THEN** 该场地能够提供 `battlefield_config_id`
- **AND** 不要求配置 prefab 地址字段

### Requirement: 远征主配置 SHALL 支持初始环境和基础随机事件池
系统 MUST 允许远征主配置声明初始环境，并声明不依赖当前环境的基础随机事件池列表。

#### Scenario: 配置远征初始环境
- **WHEN** 配置者定义一条远征
- **THEN** 该远征能够指定初始环境配置 Id

#### Scenario: 配置远征基础随机事件池
- **WHEN** 配置者定义一条远征
- **THEN** 该远征能够指定多个基础随机事件池
- **AND** 这些池不因环境切换而被移除

### Requirement: 远征节点配置 SHALL 支持 RandomEvent 节点类型
系统 MUST 允许远征节点配置声明 `RandomEvent` 节点类型。该节点类型 MUST 能复用现有节点级路由配置。

#### Scenario: 配置随机事件节点
- **WHEN** 配置者定义一个 `RandomEvent` 节点
- **THEN** 该节点能够声明节点级路由策略
- **AND** 不要求在节点上固定写死一个 Event 配置 Id

### Requirement: Combat 遭遇配置 SHALL 支持可选场地引用
系统 MUST 允许远征 Combat 遭遇配置声明可选场地配置 Id。未配置或配置为空时，运行时从当前环境选择场地。

#### Scenario: 配置遭遇场地
- **WHEN** 配置者定义一个 Combat 遭遇
- **THEN** 该遭遇能够选择性引用一个 Combat 场地配置
- **AND** 该字段为空时不阻止配置生成

### Requirement: Agent SHALL NOT 修改 xlsx 表格
实现该变更的 agent MUST NOT 创建、编辑、填充或修改任何 xlsx 表格。若 schema 变更需要表格新增 sheet、列或数据，agent MUST 暂停并通知用户手工修改。

#### Scenario: schema 变更需要表格配合
- **WHEN** 实现者修改 Luban xml schema 后发现需要更新 xlsx 内容
- **THEN** 实现者停止继续依赖生成代码的实现工作
- **AND** 实现者向用户列出需要修改的表格、sheet 和字段
- **AND** 等待用户修改表格并重新生成代码后再继续
