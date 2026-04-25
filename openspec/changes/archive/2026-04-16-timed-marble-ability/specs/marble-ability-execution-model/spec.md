## MODIFIED Requirements

### Requirement: Marble Ability SHALL declare its execution mode
Marble 战斗域中的 Ability MUST 明确声明其执行模式，至少区分逐帧更新、物理帧更新和仅显式触发三类。宿主 MUST 基于该声明决定是否将 Ability 纳入轮询分发。对于定时类 Ability，系统 MUST 允许同一个 Ability 同时参与逻辑帧更新时间状态与物理帧提交效果值两个阶段，而不要求宿主引入新的专用分发通道。

#### Scenario: Event-only ability is not polled
- **WHEN** 一个 Ability 被声明为仅显式触发能力
- **THEN** 宿主不得在常规 `Update` 或 `FixedUpdate` 分发中调用它

#### Scenario: Fixed ability participates in physics dispatch
- **WHEN** 一个 Ability 被声明需要物理帧执行
- **THEN** 宿主必须在物理帧阶段调用该 Ability 的固定更新逻辑

#### Scenario: Timed ability uses update and fixed dispatch together
- **WHEN** 一个定时 Marble Ability 同时实现逻辑帧更新接口与物理帧更新接口
- **THEN** 宿主必须在逻辑帧推进其时间状态，并在物理帧允许其提交效果值，而不需要额外的宿主级状态机支持

## ADDED Requirements

### Requirement: Timed effects SHALL continuously submit intent during active phase
对于通过 `RuntimeData` Manager 驱动移动或旋转的定时 Marble Ability，系统 MUST 要求其仅在激活阶段持续向相应 Manager 提交意图值；当激活结束后，Ability MUST 停止提交这些值，并进入后续时序阶段。

#### Scenario: Dash ability boosts speed only while active
- **WHEN** 一个冲刺类 Ability 处于激活阶段
- **THEN** 它必须在每个物理帧持续向 `TargetVelocityManager` 与 `AccelerationManager` 提交冲刺值

#### Scenario: Dash ability stops contributing after active phase
- **WHEN** 一个冲刺类 Ability 的激活阶段结束
- **THEN** 它不得继续向移动相关 Manager 提交冲刺值，并应由其时序策略控制后续冷却或再次激活
