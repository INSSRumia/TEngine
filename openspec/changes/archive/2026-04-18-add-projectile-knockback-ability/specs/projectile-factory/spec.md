## MODIFIED Requirements

### Requirement: 能力组合
系统 SHALL 按 Projectile 的既有装配骨架挂载能力：固定骨架能力由 Factory 统一挂载，`ProjectileLevelConfig.lst_ability` 中声明的扩展能力则 MUST 通过配置 creator 入口创建并附加到 Projectile。该规则同样适用于新的击退扩展 Ability。

#### Scenario: Config-driven optional ability is attached through creator
- **WHEN** `ProjectileLevelConfig.lst_ability` 中包含一个 `ProjectileKnockbackConfig`
- **THEN** `ProjectileFactory` 必须通过配置 creator 创建 `ProjectileKnockbackAbility`
- **AND** 再按配置优先级将其挂载到 Projectile

#### Scenario: Fixed core abilities remain outside optional knockback config
- **WHEN** 系统创建一个带击退扩展能力的 Projectile
- **THEN** 移动、伤害、生命周期和追踪等固定骨架能力仍必须按既有入口挂载
- **AND** 击退能力不得替代或混入这些固定骨架字段
