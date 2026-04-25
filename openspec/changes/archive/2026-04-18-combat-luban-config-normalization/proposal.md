## Why

当前战斗系统的 Luban 配置已经承载了大量行为参数，但部分关键语义仍依赖 `int + 注释`、不稳定的字段类型和不够清晰的字段分层来表达，这会同时增加人类维护成本和 AI 生成/修改配置代码时的误判概率。现在需要把战斗配置进一步规范化，让配置结构本身就能表达规则，而不是继续依赖注释补充真实语义。

## What Changes

- 规范战斗系统 Luban 配置中行为型字段的表达方式，优先将 magic number 型行为字段改为显式枚举。
- 统一时间、角度、速度等高频数值字段的命名和类型约定，减少配置与代码之间的语义漂移。
- 重新梳理主能力配置字段与扩展能力配置字段的边界，让配置结构明确表达“固定主能力参数”和“扩展玩法能力参数”。
- 降低配置对长注释规则的依赖，推动核心语义逐步由 `enum`、字段命名和 bean 层级来表达。
- 为后续 AI 辅助开发建立更稳定的配置约定，使新增能力和调整玩法参数时更容易自动推断正确修改点。

## Capabilities

### New Capabilities
- `combat-luban-config-conventions`: 定义战斗系统 Luban 配置的命名、类型和语义表达规范。
- `combat-behavior-enum-config`: 为战斗配置中的行为型字段提供枚举化表达能力，替代 magic number 写法。

### Modified Capabilities
- `combat-factory-assembly-model`: 调整工厂装配与配置结构之间的契约，使主能力配置字段和扩展能力字段边界更稳定。
- `combat-timing-config-factory`: 明确定时类配置字段的类型和结构约定，确保 Timing 工厂可依赖稳定配置结构。

## Impact

- 主要影响配置定义文件：
  - `Configs/GameConfig/Defines/equip.xml`
  - `Configs/GameConfig/Defines/marble.xml`
  - `Configs/GameConfig/Defines/projectile.xml`
- 可能影响由 Luban 生成的配置访问代码以及对应的工厂映射逻辑：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Equipment/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/`
- 影响后续 AI 开发路径：配置变更、字段搜索、枚举判断和能力映射将基于更稳定的 schema 约定。
