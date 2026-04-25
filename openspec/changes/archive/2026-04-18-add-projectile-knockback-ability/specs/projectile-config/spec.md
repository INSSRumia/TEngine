## MODIFIED Requirements

### Requirement: 发射物能力配置
系统 SHALL 通过 `ProjectileLevelConfig.lst_ability` 定义发射物附加的可选能力组合，并允许使用多态 Ability Config 表达不同玩法扩展。每个可选能力配置 MUST 至少包含 `priority` 与该能力自己的参数；当配置项声明为击退能力时，系统 MUST 能识别并解析对应的击退参数。

#### Scenario: 能力组合配置
- **WHEN** 定义 `ProjectileLevelConfig.lst_ability`
- **THEN** 每个能力配置必须包含：
  - `priority`
  - 对应能力的具体参数
- **AND** 系统必须支持在同一个能力列表中挂载击退等可选扩展能力

## ADDED Requirements

### Requirement: 发射物击退配置
系统 SHALL 提供 `ProjectileKnockbackConfig` 作为 `ProjectileAbilityConfig` 的一个可选派生配置，并使用单一 `force` 字段描述一次命中的击退强度。

#### Scenario: Knockback config exposes force only
- **WHEN** 配置表声明一个击退 Ability
- **THEN** 该配置必须包含 `force` 字段
- **AND** 首版不得要求额外的方向、时长或目标过滤字段
