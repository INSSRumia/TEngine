## ADDED Requirements

### Requirement: Projectile 击退能力 SHALL 按配置施加命中冲量
系统 SHALL 提供 `ProjectileKnockbackAbility` 作为发射物的可选扩展 Ability。当发射物产生一次有效命中时，该 Ability MUST 按配置的 `force` 值对目标 `Rigidbody2D` 施加一次击退冲量。

#### Scenario: Valid hit applies knockback impulse
- **WHEN** 一个带有 `ProjectileKnockbackAbility` 的发射物命中有效敌对目标
- **THEN** 系统必须按 `force` 配置对目标刚体施加一次 `ForceMode2D.Impulse`
- **AND** 击退方向必须优先使用发射物当前飞行方向

### Requirement: Projectile 击退能力 SHALL 复用有效命中结果
`ProjectileKnockbackAbility` MUST 仅在 Projectile 命中流水线已经确认本次碰撞为有效命中后生效，并复用同一次命中的目标选择与重复命中保护结果。

#### Scenario: Invalid or ignored hit does not trigger knockback
- **WHEN** 发射物碰撞到友军、重复命中的目标，或未被命中流水线认定为有效目标
- **THEN** `ProjectileKnockbackAbility` 不得施加击退冲量

### Requirement: Projectile 击退能力 SHALL 安全跳过无刚体目标
当有效命中的目标不具备可施力的 `Rigidbody2D` 时，系统 MUST 安全跳过击退施加，而不影响该次命中的其他处理。

#### Scenario: Hit target without rigidbody
- **WHEN** 一次有效命中的目标没有可用的 `Rigidbody2D`
- **THEN** 系统不得因击退能力抛出错误或中断命中处理
- **AND** 该次命中的伤害与生命周期处理仍按原有规则继续执行
