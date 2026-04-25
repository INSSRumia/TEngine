## ADDED Requirements

### Requirement: Behavior-selecting config fields SHALL use enums instead of magic numbers
战斗系统配置中用于选择行为分支、模式或策略的字段 MUST 使用显式枚举类型表达，而不是直接使用缺乏语义的整数值。

#### Scenario: Bow shoot type is represented as enum
- **WHEN** 配置定义弓的射击模式
- **THEN** 射击模式字段必须使用显式枚举类型
- **AND** 代码消费方不得再依赖 `0/1/...` 之类的 magic number 判断射击模式

#### Scenario: Enum values remain self-descriptive
- **WHEN** 开发者或 AI 读取行为型配置字段
- **THEN** 枚举值名称必须能直接表达对应行为模式
- **AND** 不应要求额外翻阅注释才能理解枚举值含义

### Requirement: Enum-based behavior config SHALL remain compatible with factory mapping
行为型配置字段在改为枚举后，相关工厂和能力构造逻辑 MUST 能直接消费枚举语义，而不是重新退化为不透明数字判断。

#### Scenario: Factory maps enum config without semantic loss
- **WHEN** 工厂读取枚举化的行为配置字段
- **THEN** 工厂必须基于该枚举语义进行能力构造或流程分支
- **AND** 不得在中间层再将其还原为无法解释的裸整数规则
