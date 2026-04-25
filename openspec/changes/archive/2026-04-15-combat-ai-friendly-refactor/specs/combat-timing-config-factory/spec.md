## ADDED Requirements

### Requirement: Timing config creation SHALL be centralized in a lightweight factory
战斗域中的定时能力配置解析 MUST 通过统一的轻量 Timing Factory 完成，该工厂必须是无状态的，并负责将 `AbilityTimingConfig` 及其派生配置转换为可执行的 `IAbilityTiming` 实例。

#### Scenario: Fixed timing config creates fixed timing instance
- **WHEN** 系统传入 `FixedAbilityTimingConfig`
- **THEN** Timing Factory 必须返回对应的 `FixedDurationAbilityTiming` 实例

#### Scenario: Random timing config creates random timing instance
- **WHEN** 系统传入 `RandomRangeAbilityTimingConfig`
- **THEN** Timing Factory 必须返回对应的 `RandomRangeAbilityTiming` 实例

### Requirement: Timing config creation SHALL remain reusable across marble timed abilities
Timing Factory MUST 服务于所有需要定时配置解析的 Marble 能力，而不是绑定到某一个具体能力 creator 或某一个 Marble 专属接口。

#### Scenario: Multiple timed abilities share the same timing factory
- **WHEN** 不同 Marble 定时能力需要从配置中构建 timing
- **THEN** 它们必须通过同一个 Timing Factory 创建 timing
- **AND** 不得在各自 creator 内重复实现同类 timing 解析逻辑

#### Scenario: Unsupported timing config fails predictably
- **WHEN** 传入当前 Timing Factory 不支持的 timing 配置类型
- **THEN** 工厂必须返回空结果或等价的失败信号
- **AND** 调用方必须能够据此做出显式处理
