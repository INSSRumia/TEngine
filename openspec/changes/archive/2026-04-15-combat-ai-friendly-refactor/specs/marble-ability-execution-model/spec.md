## MODIFIED Requirements

### Requirement: Marble state resolution SHALL support explicit trigger points
Marble 战斗域 MUST 提供显式结算触发点，用于在状态变化后触发离散逻辑，例如伤害结算、死亡判定和升级检测。此类逻辑 MUST 不依赖逐帧被动轮询才能生效。Marble 的配置驱动能力装配 ALSO MUST 通过统一的配置挂载入口完成，不得将配置解析逻辑分散到多个彼此无关的装配路径中。

#### Scenario: Damage resolution triggers after pending values change
- **WHEN** Marble 的待处理伤害或治疗值发生变化并进入结算阶段
- **THEN** 宿主或结算入口必须显式触发对应能力完成生命与护盾结算

#### Scenario: Exp gain triggers level-up check
- **WHEN** Marble 的经验值被增加
- **THEN** 系统必须在该次经验变更后触发升级检测，而不是等待后续任意一帧轮询

#### Scenario: Marble config abilities use a single attachment path
- **WHEN** MarbleFactory 根据等级配置读取 `lst_ability`
- **THEN** 所有配置能力必须通过统一的配置挂载入口创建并挂载
- **AND** 定时类能力的 timing 构建必须通过共享的 Timing Factory 完成
