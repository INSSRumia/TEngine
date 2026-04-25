## ADDED Requirements

### Requirement: Combat module SHALL provide structural documentation
Combat 模块 MUST 提供结构化模块文档，解释 Marble、Equipment、Projectile、Factory、Ability、RuntimeData 和配置映射之间的关系，而不是只依赖代码命名隐式表达模块结构。

#### Scenario: Reader can understand combat architecture without reading all source files
- **WHEN** AI 或开发者首次接手 Combat 模块
- **THEN** 它必须能够通过模块文档理解主要对象、数据流和装配关系
- **AND** 不需要先通读全部源码才能建立基本心智模型

#### Scenario: Documentation explains runtime blackboard and factory roles
- **WHEN** 模块文档描述 Combat 系统
- **THEN** 文档必须解释 Runtime 黑板、Factory 装配、配置驱动能力和核心能力之间的边界
- **AND** 说明这些约定如何在代码中体现
