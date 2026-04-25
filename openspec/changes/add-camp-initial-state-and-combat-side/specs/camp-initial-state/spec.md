## ADDED Requirements

### Requirement: 系统开局 SHALL 由 InitialConfig 选择默认阵营包
系统 SHALL 通过单例 `InitialConfig` 指定当前游戏默认使用的 `CampConfig`，并以该阵营包作为局外初始状态的唯一配置入口。

#### Scenario: 读取默认开局阵营
- **WHEN** 系统需要初始化局外持久化数据且当前数据为空
- **THEN** 系统读取 `InitialConfig`
- **AND** 系统根据其中的 `camp_config_id` 解析对应的 `CampConfig`

### Requirement: CampConfig SHALL 定义开局资金、初始 Marble 与初始可用远征
系统 MUST 让 `CampConfig` 同时提供开局资金、初始 Marble 列表和初始可用远征配置列表，使单个阵营包可以完整描述最小循环的开局状态。

#### Scenario: 通过阵营包构建开局状态
- **WHEN** 系统成功解析某个 `CampConfig`
- **THEN** 该配置能够提供 `initial_money`
- **AND** 该配置能够提供 `lst_initial_marbles`
- **AND** 该配置能够提供 `lst_expedition`

### Requirement: MarbleSpawnConfig SHALL 作为可复用的 Marble 静态生成条目
系统 SHALL 使用 `Gameplay.Combat.MarbleSpawnConfig` 作为玩家初始 Marble 和远征敌方 Marble 的通用静态生成条目。该条目 MUST 包含 `marble_config_id`、`level` 和 `camp_config_id`。

#### Scenario: 用通用条目表达玩家初始 Marble
- **WHEN** 某个 `CampConfig` 定义开局 Marble 列表
- **THEN** 列表中的每个条目都使用 `MarbleSpawnConfig`
- **AND** 每个条目都能提供 Marble 配置标识、等级和配置阵营标识
