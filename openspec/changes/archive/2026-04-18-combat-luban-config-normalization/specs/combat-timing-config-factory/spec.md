## MODIFIED Requirements

### Requirement: Timing config creation SHALL be centralized in a lightweight factory
战斗域中的定时能力配置解析 MUST 通过统一的轻量 Timing Factory 完成，该工厂必须是无状态的，并负责将 `AbilityTimingConfig` 及其派生配置转换为可执行的 `IAbilityTiming` 实例。Timing 相关配置字段 ALSO MUST 使用稳定的时间类型和清晰字段语义，以保证工厂可以在不依赖额外解释的情况下完成构建。

#### Scenario: Fixed timing config creates fixed timing instance
- **WHEN** 系统传入 `FixedAbilityTimingConfig`
- **THEN** Timing Factory 必须返回对应的 `FixedDurationAbilityTiming` 实例

#### Scenario: Random timing config creates random timing instance
- **WHEN** 系统传入 `RandomRangeAbilityTimingConfig`
- **THEN** Timing Factory 必须返回对应的 `RandomRangeAbilityTiming` 实例

#### Scenario: Timing config uses stable time semantics
- **WHEN** 调用方读取 timing 配置中的持续时间和冷却时间字段
- **THEN** 这些字段必须遵循统一的时间语义和数值类型约定
- **AND** Timing Factory 不应承担修补混乱字段语义的额外职责
