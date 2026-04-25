## Why

当前 `ProjectileDamageAbility` 只有伤害与穿透处理，命中后的击退效果仍停留在注释掉的临时代码里，既不能按配置选择启用，也无法复用到不同发射物。现在补上一个最小可用的 Projectile 击退扩展 Ability，可以让发射物玩法继续沿用现有的配置驱动装配方式，同时避免把所有投射物命中都绑定成固定击退。

## What Changes

- 新增一个可挂载到 `ProjectileLevelConfig.lst_ability` 的 `ProjectileKnockbackConfig`，首版仅包含 `force` 字段。
- 新增 `ProjectileKnockbackAbility`，在发射物命中有效目标时对目标 `Rigidbody2D` 施加击退冲量。
- 调整发射物碰撞处理链路，使可选扩展 Ability 能接收到统一的命中上下文，而不必把击退逻辑硬编码进 `ProjectileDamageAbility`。
- 扩展 `ProjectileFactory` 的配置映射，让 `ProjectileKnockbackConfig` 能按现有模式实例化为运行时 Ability。
- 补充 `projectile.xlsx` 使用约束，使配置表可以通过 `lst_ability` 选择性启用击退玩法。

## Capabilities

### New Capabilities
- `projectile-knockback-ability`: 定义发射物命中后按配置施加击退冲量的可选扩展能力。

### Modified Capabilities
- `projectile-config`: 扩展发射物可选 Ability schema，使 `lst_ability` 支持带 `force` 字段的击退配置。
- `projectile-core`: 调整发射物命中事件转发方式，使可选 Ability 可以消费统一的命中上下文。
- `projectile-factory`: 扩展发射物 Ability 创建逻辑，使配置驱动的击退 Ability 能被正确装配。

## Impact

- **配置定义**：
  - `Configs/GameConfig/Defines/projectile_ability.xml`
  - `Configs/GameConfig/Datas/projectile.xlsx`
- **运行时代码**：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Projectile.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/ProjectileFactory.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/Core/ProjectileDamageAbility.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/` 下新的击退 Ability 与命中接口/上下文类型
- **生成与验证**：
  - 需要重新执行 Luban 代码生成
  - 需要执行 `GameLogic.csproj` 构建验证
