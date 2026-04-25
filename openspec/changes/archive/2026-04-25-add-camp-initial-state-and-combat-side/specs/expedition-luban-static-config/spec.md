## MODIFIED Requirements

### Requirement: Combat 遭遇配置 SHALL 提供敌方编队与胜负效果列表
系统 MUST 让远征 Combat 遭遇配置显式提供敌方 Marble 列表、标题描述以及两组结算效果列表：战斗胜利时触发的 `LstVictoryEffect` 与战斗失败时触发的 `LstDefeatEffect`。敌方 Marble 列表 MUST 直接复用 `Gameplay.Combat.MarbleSpawnConfig`，而不是继续依赖远征域专用的敌方 Marble bean。首版固定胜利奖励字段不再作为唯一表达方式。

#### Scenario: 从遭遇配置构建 Combat 输入与胜负结果
- **WHEN** 远征流程进入一个 Combat 节点
- **THEN** 系统根据节点引用读取对应的 Combat 遭遇配置
- **AND** 该配置通过 `MarbleSpawnConfig` 列表提供敌方 Marble 编队
- **AND** 每个敌方 Marble 条目能够提供 `marble_config_id`、`level` 和 `camp_config_id`
- **AND** 该配置能够根据 Combat 结算结果提供对应的胜利或失败 Effect 列表
