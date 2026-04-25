## ADDED Requirements

### Requirement: 局外初始状态 SHALL 来源于开局配置
系统 SHALL 在局外持久化数据为空时，通过 `InitialConfig -> CampConfig` 初始化开局资金和初始 Marble，而不是继续写死默认值。

#### Scenario: 第一次进入入口时初始化局外数据
- **WHEN** 玩家第一次进入远征入口且当前局外持久化数据为空
- **THEN** 系统根据 `InitialConfig` 解析默认 `CampConfig`
- **AND** 系统使用该阵营包的 `initial_money` 初始化局外资源
- **AND** 系统使用该阵营包的 `lst_initial_marbles` 初始化局外 Marble 列表

#### Scenario: 已有局外数据时不重复覆盖
- **WHEN** 玩家再次进入远征入口且局外持久化数据已经存在
- **THEN** 系统不重新按开局配置覆盖现有资金与 Marble 状态

### Requirement: 远征入口 SHALL 使用当前阵营包提供的可用远征列表
系统 SHALL 让远征入口和默认启动逻辑从当前 `CampConfig` 的 `lst_expedition` 读取可用远征，而不是继续依赖写死的单个远征配置标识。

#### Scenario: 从阵营包选择可启动远征
- **WHEN** 系统准备启动一次远征
- **THEN** 系统从当前 `CampConfig` 的 `lst_expedition` 读取可用远征配置标识
- **AND** 系统仅使用该列表中的远征配置作为本次可启动目标
