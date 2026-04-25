## ADDED Requirements

### Requirement: Combat Luban config SHALL use explicit semantic field conventions
战斗系统的 Luban 配置定义 MUST 对高频语义字段使用稳定的命名和类型约定，使字段自身能够表达其用途，而不是依赖额外上下文猜测。

#### Scenario: Time-like fields use a consistent numeric type
- **WHEN** 某个字段表示持续时间、冷却时间、存活时间或其他时间长度
- **THEN** 该字段必须使用统一的时间数值类型约定
- **AND** 不得在语义相同的时间字段之间混用彼此不兼容的整数和浮点表达

#### Scenario: Motion fields use stable naming
- **WHEN** 某个字段表示速度、角速度、加速度或角度
- **THEN** 该字段必须遵循统一命名约定
- **AND** 调用方必须能仅凭字段名推断其大致语义类别

### Requirement: Main capability fields and extension capability fields SHALL be clearly separated
战斗配置 MUST 明确区分对象运行所需的主能力配置字段与用于扩展玩法的能力配置列表字段，不得让二者边界模糊。

#### Scenario: Main capability fields remain explicit
- **WHEN** 某个对象存在运行所必需的主能力参数
- **THEN** 这些参数必须保留为显式字段或显式子 bean
- **AND** 不得与扩展能力列表混杂为同一层级的隐式规则

#### Scenario: Extension abilities use a dedicated list
- **WHEN** 某个能力属于玩法扩展能力而非固定骨架能力
- **THEN** 该能力必须通过专门的扩展能力配置列表表达
- **AND** 调用方必须能稳定识别它是扩展路径而不是主能力骨架

### Requirement: Comments SHALL supplement schema rather than define core behavior
战斗配置中的注释 MUST 仅作为说明和补充，核心行为语义必须优先通过字段、枚举和 bean 结构表达。

#### Scenario: Behavior meaning is visible without reading long comments
- **WHEN** 开发者或 AI 阅读某个行为配置字段
- **THEN** 应当能够从字段类型、枚举值或 bean 结构中直接理解其主要语义
- **AND** 不需要依赖长注释才能知道基础行为分支
