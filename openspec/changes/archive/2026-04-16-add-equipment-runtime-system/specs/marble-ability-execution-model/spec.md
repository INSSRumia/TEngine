## MODIFIED Requirements

### Requirement: Marble state resolution SHALL support explicit trigger points
Marble 战斗域 MUST 提供显式结算触发点，用于在状态变化后触发离散逻辑，例如伤害结算、死亡判定和升级检测。此类逻辑 MUST 不依赖逐帧被动轮询才能生效。When Marble hosts attached equipment, the battle domain MUST also provide explicit trigger points for equipment initialization, slot attachment, and pre-resolution participation in combat state changes so that armor and weapons can join the owner Marble's battle chain without forcing all equipment logic into per-frame polling.

#### Scenario: Damage resolution triggers after pending values change
- **WHEN** Marble 的待处理伤害或治疗值发生变化并进入结算阶段
- **THEN** 宿主或结算入口必须显式触发对应能力完成生命与护盾结算

#### Scenario: Exp gain triggers level-up check
- **WHEN** Marble 的经验值被增加
- **THEN** 系统必须在该次经验变更后触发升级检测，而不是等待后续任意一帧轮询

#### Scenario: Equipment joins owner resolution chain
- **WHEN** Marble 挂载了防具或武器并进入相关战斗结算阶段
- **THEN** 系统必须允许装备在显式触发点参与槽位初始化、伤害拦截或主动攻击准备，而不是要求所有装备逻辑都依赖统一逐帧轮询
