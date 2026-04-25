## Why

当前战斗系统在 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 三套工厂中的装配结构、配置映射边界和扩展入口存在不一致，已经开始影响代码可维护性，也容易让 AI 在继续开发时误判默认能力、配置能力和扩展点职责。现在需要把这些约定正式沉淀为统一变更，避免后续继续在局部修补中积累复杂度。

## What Changes

- 统一三套 Combat Factory 的能力装配骨架，明确默认能力、配置驱动能力、扩展 creator 的职责边界。
- 收敛 `CreateAbilityFromConfig`、`AttachDefaultAbilities`、配置能力挂载入口等关键方法的结构和命名，减少同类逻辑的多种写法。
- 将 Marble 的 Timing 创建逻辑独立为轻量 `AbilityTimingFactory`，避免配置解析逻辑散落在具体 ability creator 内。
- 清理误导性结构和中间态接口，移除不必要的抽象，降低 AI 和开发者的理解成本。
- 修正工厂层中已经暴露出的明显错误，包括配置映射错误、作用域错误、重复职责和扩展点不清晰问题。
- 重新梳理默认能力与配置能力的归属，让代码层清楚表达“固定骨架”和“配置驱动扩展”。

## Capabilities

### New Capabilities
- `combat-factory-assembly-model`: 统一战斗工厂的能力装配模型，定义默认能力、配置能力和 creator 扩展点的协作方式。
- `combat-timing-config-factory`: 提供通用的 Timing 配置创建能力，统一 Marble 定时类 ability 的 timing 构建入口。

### Modified Capabilities
- `marble-ability-execution-model`: 调整 Marble ability 的配置装配边界，使配置能力装配流程与其他 combat factory 保持一致。
- `combat-manager`: 更新战斗系统工厂侧的结构约定和扩展点组织方式，确保 manager 相关战斗对象创建流程更稳定一致。

## Impact

- 影响代码路径：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Equipment/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/`
- 影响配置定义理解方式：
  - `Configs/GameConfig/Defines/marble.xml`
  - `Configs/GameConfig/Defines/equip.xml`
  - `Configs/GameConfig/Defines/projectile.xml`
- 影响后续 AI 开发路径：新增能力、调整配置、扩展工厂装配时将依赖统一的 factory/creator 模型。
