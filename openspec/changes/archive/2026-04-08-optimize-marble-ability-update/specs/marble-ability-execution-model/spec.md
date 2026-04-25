## ADDED Requirements

### Requirement: Marble Ability SHALL declare its execution mode
Marble 战斗域中的 Ability MUST 明确声明其执行模式，至少区分逐帧更新、物理帧更新和仅显式触发三类。宿主 MUST 基于该声明决定是否将 Ability 纳入轮询分发。

#### Scenario: Event-only ability is not polled
- **WHEN** 一个 Ability 被声明为仅显式触发能力
- **THEN** 宿主不得在常规 `Update` 或 `FixedUpdate` 分发中调用它

#### Scenario: Fixed ability participates in physics dispatch
- **WHEN** 一个 Ability 被声明需要物理帧执行
- **THEN** 宿主必须在物理帧阶段调用该 Ability 的固定更新逻辑

### Requirement: Marble host SHALL only iterate subscribed polling abilities
Marble 宿主 MUST 仅遍历声明参与对应执行阶段的 Ability 集合，不得在每一帧对所有已挂载 Ability 做统一调用后再由各 Ability 自行空返回。

#### Scenario: Update dispatch skips non-update abilities
- **WHEN** 宿主执行一帧逻辑更新
- **THEN** 仅声明参与逻辑帧更新的 Ability 会被调用

#### Scenario: Fixed dispatch skips event-only abilities
- **WHEN** 宿主执行一帧物理更新
- **THEN** 仅声明参与物理帧更新的 Ability 会被调用，事件型 Ability 不会进入该轮询

### Requirement: Marble state resolution SHALL support explicit trigger points
Marble 战斗域 MUST 提供显式结算触发点，用于在状态变化后触发离散逻辑，例如伤害结算、死亡判定和升级检测。此类逻辑 MUST 不依赖逐帧被动轮询才能生效。

#### Scenario: Damage resolution triggers after pending values change
- **WHEN** Marble 的待处理伤害或治疗值发生变化并进入结算阶段
- **THEN** 宿主或结算入口必须显式触发对应能力完成生命与护盾结算

#### Scenario: Exp gain triggers level-up check
- **WHEN** Marble 的经验值被增加
- **THEN** 系统必须在该次经验变更后触发升级检测，而不是等待后续任意一帧轮询
