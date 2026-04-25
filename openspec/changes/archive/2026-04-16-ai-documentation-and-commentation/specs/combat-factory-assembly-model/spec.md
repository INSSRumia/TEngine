## MODIFIED Requirements

### Requirement: Combat Factory SHALL use a consistent ability assembly skeleton
战斗域中的 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 在装配能力时 MUST 使用一致的结构化骨架，至少明确区分固定骨架能力挂载、配置驱动能力挂载和配置 creator 扩展点三部分。该装配模型 ALSO MUST 具备对应的文档或注释说明，使阅读者能够在不追踪全部实现代码的情况下理解装配边界。

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

#### Scenario: Reader can understand factory assembly boundary from documentation
- **WHEN** AI 或开发者需要理解工厂装配规则
- **THEN** 它必须能够从模块文档或关键注释中直接理解装配边界
- **AND** 不需要仅靠反向阅读完整实现才能推断规则
