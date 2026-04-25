## MODIFIED Requirements

### Requirement: Config-driven abilities SHALL be attached through a single explicit path
每个 Combat Factory 对于配置驱动能力 MUST 提供单一且显式的挂载路径，配置能力的创建、判空、优先级设置和最终挂载顺序必须可预测。工厂所依赖的配置结构 ALSO MUST 稳定区分主能力字段与扩展能力字段，以避免工厂层继续承担“猜测配置语义”的职责。

#### Scenario: Config ability attachment is deterministic
- **WHEN** 工厂读取某个配置能力列表
- **THEN** 系统必须按列表顺序遍历配置项
- **AND** 每个配置项先创建能力实例
- **AND** 仅在创建成功后设置优先级并挂载到宿主

#### Scenario: Unknown config creator fails explicitly
- **WHEN** 某个配置能力在所有已注册 creator 中都无法创建
- **THEN** 工厂必须记录显式错误
- **AND** 不得静默忽略导致装配结果不可追踪

#### Scenario: Factory can distinguish main capability config from extension config
- **WHEN** 工厂读取对象配置
- **THEN** 它必须能稳定识别哪些字段属于主能力参数
- **AND** 哪些字段属于扩展能力列表
- **AND** 不应依赖长注释或隐式约定来推断装配边界
