## ADDED Requirements

### Requirement: Combat Luban XML definitions SHALL include structured schema comments
战斗系统相关的 Luban XML 定义 MUST 包含结构化注释，用于解释 bean 职责、字段语义类别、代码消费入口和配置边界，而不是只保留零散的字段说明。

#### Scenario: Bean comment explains role and usage
- **WHEN** AI 或开发者阅读某个 combat 配置 bean
- **THEN** 注释必须说明该 bean 的职责
- **AND** 说明它属于主能力配置、扩展能力配置、定时配置或其他结构角色

#### Scenario: Field comment explains code consumption boundary
- **WHEN** AI 或开发者阅读某个关键字段
- **THEN** 注释必须帮助其理解该字段会被哪类 Factory、Ability 或转换逻辑消费
- **AND** 明确该字段与其它字段或配置列表的边界关系

### Requirement: Schema comments SHALL reduce reliance on external memory
Luban XML 注释 MUST 足够帮助阅读者从 schema 本身理解配置结构，减少对历史会话、隐性约定或外部口头说明的依赖。

#### Scenario: Behavior meaning is inferable from schema plus comments
- **WHEN** AI 需要修改某个战斗配置定义
- **THEN** 它必须能够通过 XML 结构和注释判断主要语义
- **AND** 不必完全依赖外部补充说明才能理解配置边界
