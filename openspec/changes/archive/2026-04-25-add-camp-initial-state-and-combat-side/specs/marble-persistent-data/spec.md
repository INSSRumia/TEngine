## ADDED Requirements

### Requirement: Marble 局外数据 SHALL 从配置化种子初始化
系统 SHALL 在局外 Marble 持久化数据为空时，根据开局阵营包中的 `MarbleSpawnConfig` 列表生成初始持久化 Marble 数据，而不是继续写死默认 Marble。

#### Scenario: 通过 MarbleSpawnConfig 生成持久化 Marble
- **WHEN** 系统根据 `CampConfig` 初始化局外 Marble 数据
- **THEN** 系统为每个 `MarbleSpawnConfig` 条目生成对应的 Marble 持久化数据
- **AND** 每条持久化数据都包含稳定的 Marble 实例标识、配置标识、等级和初始生命值

### Requirement: Marble 持久化数据 SHALL 保留配置阵营标识
系统 SHALL 在 Marble 持久化数据中保留 `camp_config_id`，以便后续表现层和运行态可以读取该 Marble 的配置阵营来源。

#### Scenario: 持久化数据保存配置阵营
- **WHEN** 系统创建或回写某个 Marble 的局外持久化数据
- **THEN** 该数据中包含对应的 `camp_config_id`
- **AND** 该字段用于表示 Marble 的配置阵营来源，而不是战斗内敌我归属
