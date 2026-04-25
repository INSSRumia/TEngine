## Why

当前项目已经逐步形成了战斗系统、Runtime 黑板、Factory 装配和 Luban 配置等多条内部约定，但这些信息仍然主要分散在代码、少量注释和会话记忆中，不足以支撑后续 AI 稳定接手开发。现在需要系统性补充 AI 可读的项目文档、模块说明和关键注释，让 AI 在进入项目时能先读到正确的结构信息，而不是被迫从源码碎片中猜测规则。

## What Changes

- 为项目建立专门面向 AI 和开发者的入口级文档，说明项目结构、核心模块和关键约定。
- 为 Combat、RuntimeData、Factory、Luban 配置等关键区域补充结构化模块文档。
- 为 `equip.xml`、`marble.xml`、`projectile.xml` 等 Luban 配置定义补充可维护、可读、能表达语义边界的注释。
- 为关键代码文件补充必要源码注释，重点解释职责、边界、黑板分区和复杂结算流程，而不是机械注释简单代码。
- 形成一套“AI 优先阅读路径”，明确先看哪些文档、再看哪些代码和配置。

## Capabilities

### New Capabilities
- `ai-project-reading-guide`: 提供面向 AI 的项目入口文档与推荐阅读路径。
- `combat-module-documentation`: 提供 Combat 模块的结构文档，解释 RuntimeData、Factory、Ability、配置映射和黑板模型。
- `luban-schema-commentation`: 为战斗相关 Luban XML schema 增补结构化注释，使字段与 bean 的语义更自解释。

### Modified Capabilities
- `combat-factory-assembly-model`: 增补与工厂装配结构相关的文档说明要求，使工厂装配规则不仅体现在代码里，也体现在文档中。
- `marble-runtime-blackboard-model`: 增补对 Marble Runtime 黑板结构的文档化要求，使 `State / Config / Frame` 结构对 AI 可直接理解。

## Impact

- 主要影响文档与注释：
  - 项目级文档（AI 入口说明、模块说明）
  - `Configs/GameConfig/Defines/*.xml`
  - Combat 相关核心代码文件中的关键注释
- 主要影响区域：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/`
  - `Configs/GameConfig/Defines/`
  - 项目根或模块目录下的 AI 辅助文档
- 影响后续 AI 开发流程：后续 agent 可优先阅读文档和 schema 注释，再进入实现细节。
