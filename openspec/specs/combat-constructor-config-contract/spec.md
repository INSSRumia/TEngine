## ADDED Requirements

### Requirement: Combat RuntimeData SHALL accept config objects as primary constructor input
战斗域中的 `RuntimeData` 在需要从配置初始化运行字段时 MUST 以对应配置对象作为主要构造输入，而不是要求 Factory 先将配置字段展开为大量离散参数。

#### Scenario: Marble runtime data consumes level config directly
- **WHEN** 系统为 Marble 创建运行态数据
- **THEN** `MarbleRuntimeData` 必须直接接收对应的 Marble 配置对象或等级配置对象
- **AND** 运行态对象内部必须自行读取攻击、生命、护盾、防御、速度、质量、缩放等配置字段完成初始化

#### Scenario: Equipment runtime data keeps only minimal non-config context outside config
- **WHEN** 系统为 Equipment 创建运行态数据
- **THEN** 装备运行态对象必须通过配置对象获取其可配置初始化数据
- **AND** 像 slot、owner、实例态标记等非配置上下文才允许由 Factory 单独补充

### Requirement: Combat abilities SHALL own their config-reading responsibility
战斗域中的 Ability 在依赖配置字段初始化时 MUST 通过对应配置类构造，并在 Ability 内部读取配置字段，不得继续要求 Factory 直接拆解并传入大量标量参数。

#### Scenario: Config-driven ability constructor takes config object
- **WHEN** Factory 或 creator 基于某个 Ability 配置创建能力实例
- **THEN** Ability 构造函数必须直接接收对应的配置对象
- **AND** Ability 内部必须负责读取并保存该配置对象中的所需字段

#### Scenario: Factory does not expand config fields for ability construction
- **WHEN** Factory 创建一个依赖配置字段的 Ability
- **THEN** Factory 不得继续出现成组的 `config.xxx` 字段展开后传给 Ability 构造函数的主路径
- **AND** 除宿主引用、source/target、结算结果等运行时上下文外，不得把配置字段拆成多个离散参数传递

### Requirement: Each assembled combat ability SHALL have an explicit config type
每一个通过 Combat Factory 或 creator 装配的能力 MUST 具备显式配置类型，以便构造契约统一为“配置对象 + 最小上下文参数”。

#### Scenario: Core ability always has generated config type
- **WHEN** 某个固定骨架能力会被 Factory 装配
- **THEN** 系统必须为该能力提供独立 XML 配置定义
- **AND** 该配置类必须通过 Luban 生成

#### Scenario: Parameterless ability still uses explicit config bean
- **WHEN** 某个能力当前没有初始化参数
- **THEN** 它仍然必须拥有显式配置类型
- **AND** 可以使用空配置 bean，但不得省略配置类本身
