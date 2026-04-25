## ADDED Requirements

### Requirement: Combat Factory SHALL use a consistent ability assembly skeleton
战斗域中的 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 在装配能力时 MUST 使用一致的结构化骨架，至少明确区分固定骨架能力挂载、配置驱动能力挂载和配置 creator 扩展点三部分。

#### Scenario: Marble factory follows the common skeleton
- **WHEN** 系统为 Marble 创建战斗对象
- **THEN** 工厂必须先挂载固定骨架能力
- **AND** 再通过统一的配置驱动入口挂载由等级配置声明的能力

#### Scenario: Projectile factory follows the common skeleton
- **WHEN** 系统为 Projectile 创建战斗对象
- **THEN** 工厂必须先挂载固定骨架能力
- **AND** 再通过统一的配置驱动入口挂载由配置声明的追踪等扩展能力

#### Scenario: Equipment factory follows the common skeleton
- **WHEN** 系统为 Equipment 创建战斗对象
- **THEN** 工厂必须明确区分装备固定骨架能力与配置驱动扩展能力
- **AND** 不得在多个分散层级重复挂载同一类配置扩展能力

### Requirement: Config-driven abilities SHALL be attached through a single explicit path
每个 Combat Factory 对于配置驱动能力 MUST 提供单一且显式的挂载路径，配置能力的创建、判空、优先级设置和最终挂载顺序必须可预测。

#### Scenario: Config ability attachment is deterministic
- **WHEN** 工厂读取某个配置能力列表
- **THEN** 系统必须按列表顺序遍历配置项
- **AND** 每个配置项先创建能力实例
- **AND** 仅在创建成功后设置优先级并挂载到宿主

#### Scenario: Unknown config creator fails explicitly
- **WHEN** 某个配置能力在所有已注册 creator 中都无法创建
- **THEN** 工厂必须记录显式错误
- **AND** 不得静默忽略导致装配结果不可追踪

### Requirement: Creator extension points SHALL remain registration-based
Combat Factory 的配置扩展点 MUST 继续基于显式注册的 `CreatorForConfig` 列表工作，不得依赖隐式反射扫描或隐藏式副作用装配。

#### Scenario: External module registers a config creator
- **WHEN** 外部模块调用 `RegisterAbilityCreatorForConfig(...)`
- **THEN** 新 creator 必须进入对应工厂的 creator 列表
- **AND** 后续配置能力创建流程必须按优先级参与分发

#### Scenario: Creator priority resolves override order
- **WHEN** 多个 creator 都可能处理同一配置类型
- **THEN** 工厂必须按 creator 优先级降序尝试创建
- **AND** 首个成功创建的结果即为最终能力实例
