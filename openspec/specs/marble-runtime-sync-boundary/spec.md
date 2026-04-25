## ADDED Requirements

### Requirement: Marble runtime data SHALL define an explicit sync-friendly boundary
Marble Runtime 黑板 MUST 明确区分可作为长期状态理解的数据与仅用于本地帧内运算的数据，以便未来网络同步时能够优先围绕状态数据进行同步。

#### Scenario: Runtime state remains sync-oriented
- **WHEN** 系统识别 Marble 的生命、护盾、经验、目标和存活状态等关键运行态
- **THEN** 这些数据必须位于明确的运行时状态区域
- **AND** 未来同步层可以优先围绕该区域设计同步协议

#### Scenario: Frame data is excluded from primary sync state
- **WHEN** 系统处理 Marble 的帧临时控制器和中间计算数据
- **THEN** 这些数据不得被视为主要同步态
- **AND** 应当被视为可本地重建或按需推导的数据

### Requirement: Config data SHALL remain available without being treated as transient frame state
从配置初始化得到的基础值 MUST 与帧临时数据明确分离，既不能继续混入帧临时层，也不应被误判为纯即时状态。

#### Scenario: Config-derived values remain stable across frames
- **WHEN** Marble 在多个帧之间运行
- **THEN** 配置数据区域中的基础值必须以稳定方式保留
- **AND** 不得像帧临时控制数据一样在每帧中被视为短生命周期对象
