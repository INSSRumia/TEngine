## ADDED Requirements

### Requirement: 远征 SHALL 使用统一的 Expedition Effect 执行接口与上下文
系统 MUST 为远征效果定义统一的 `IExpeditionEffect` 接口，并通过单一的 `ExpeditionEffectExecutionContext` 向所有 Effect 提供执行所需的远征领域状态。

#### Scenario: Effect 通过统一上下文执行
- **WHEN** 运行时执行任意一个远征 Effect
- **THEN** 该 Effect 通过统一的 `Execute(ExpeditionEffectExecutionContext context)` 入口执行
- **AND** 不要求为不同 Effect 提供彼此不同的方法签名

### Requirement: 远征 SHALL 通过配置列表创建并顺序执行多个 Effect
系统 MUST 支持从 Luban 配置中的 Effect 列表创建多个 Expedition Effect，并按配置顺序依次执行。

#### Scenario: 事件选项触发多个 Effect
- **WHEN** 玩家选择一个配置了多个 Effect 的事件选项
- **THEN** 系统根据该选项的 `LstEffect` 创建对应的 Expedition Effect 实例
- **AND** 系统按配置顺序依次执行这些 Effect

#### Scenario: Combat 胜负触发不同的 Effect 列表
- **WHEN** 一场 Combat 节点结算为胜利或失败
- **THEN** 系统根据结算结果选择 `LstVictoryEffect` 或 `LstDefeatEffect`
- **AND** 仅执行与该结算结果对应的那一组 Effect

### Requirement: 系统 SHALL 支持首批 3 种基础远征 Effect
系统 MUST 至少支持添加金钱、为玩家全队添加经验、为玩家全队修改血量这 3 种基础远征 Effect。

#### Scenario: 执行添加金钱 Effect
- **WHEN** 系统执行一个添加金钱的远征 Effect
- **THEN** 该 Effect 更新远征或局外状态中的 `money` 相关字段

#### Scenario: 执行为玩家全队添加经验 Effect
- **WHEN** 系统执行一个为玩家全队添加经验的远征 Effect
- **THEN** 该 Effect 对当前远征中的玩家 Marble 快照统一增加经验值

#### Scenario: 执行为玩家全队修改血量 Effect
- **WHEN** 系统执行一个为玩家全队修改血量的远征 Effect
- **THEN** 该 Effect 对当前远征中的玩家 Marble 快照统一修改生命值
- **AND** 系统同步更新对应的死亡状态

### Requirement: 远征内部资源命名 SHALL 统一使用 money
系统 MUST 在远征相关代码与配置的内部命名中统一使用 `money` 表示金钱资源；玩家可见文案可以继续使用“晶体”，但内部结构不得继续混用 `crystal` 与 `money` 表示同一概念。

#### Scenario: 代码内部读取远征资源字段
- **WHEN** 开发者或运行时代码读取远征相关资源字段
- **THEN** 该字段名称使用 `money` 语义
- **AND** 不要求从字段名推断 `crystal` 与 `money` 是否表示同一资源

### Requirement: 远征 Effect SHALL 支持改变环境
系统 MUST 支持一种改变当前环境的 Expedition Effect。该 Effect MUST 通过配置指定目标环境，并通过统一的 `IExpeditionEffect` 执行入口修改远征运行态。

#### Scenario: 创建改变环境 Effect
- **WHEN** Effect 工厂读取到改变环境的 Effect 配置
- **THEN** 系统创建对应的 Expedition Effect 实例
- **AND** 该实例持有目标环境配置 Id

#### Scenario: 执行改变环境 Effect
- **WHEN** 改变环境 Effect 被执行
- **THEN** 系统更新远征运行态的当前环境
- **AND** 系统触发环境随机事件池的移除与添加规则
