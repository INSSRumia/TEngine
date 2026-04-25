## ADDED Requirements

### Requirement: Timed Marble Ability SHALL support pluggable timing lifecycle
系统 MUST 为 Marble Ability 提供统一的定时生命周期基类，并允许其注入可替换的时序策略对象，以描述激活、持续、生效结束、冷却和再次可触发的状态流转。

#### Scenario: Ability updates injected timing strategy
- **WHEN** 一个 `TimedMarbleAbility` 已挂载并进入常规逻辑帧更新
- **THEN** 该 Ability 必须在逻辑帧中推进其注入的时序策略，而不是在宿主外部由其他系统代为更新时间

#### Scenario: Ability can query current lifecycle state
- **WHEN** 派生 Ability 需要判断当前是否处于生效阶段或冷却阶段
- **THEN** 它必须能够通过统一接口查询当前时序状态，而不需要依赖隐式事件顺序推断状态

### Requirement: Timing strategies SHALL support multiple trigger patterns
时序策略 MUST 支持至少手动触发与自动循环两类触发模式，并允许固定时间规则与基于随机数的时间规则通过不同实现扩展，而不要求修改 `TimedMarbleAbility` 本身。

#### Scenario: Manual trigger timing waits for explicit activation
- **WHEN** 一个 Ability 使用手动触发时序策略且当前可触发
- **THEN** 它必须仅在显式调用触发入口后进入生效阶段

#### Scenario: Auto loop timing reactivates after cooldown completes
- **WHEN** 一个 Ability 使用自动循环时序策略且冷却阶段结束
- **THEN** 它必须自动重新进入下一次生效阶段，而不需要外部再次调用触发入口

#### Scenario: Randomized timing uses runtime-generated duration values
- **WHEN** 一个 Ability 使用随机时间规则的时序策略
- **THEN** 该策略必须能够在运行时生成本轮持续时间或冷却时间，并按生成结果推进状态流转
