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
