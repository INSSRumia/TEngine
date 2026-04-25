## MODIFIED Requirements

### Requirement: 碰撞事件转发
系统 SHALL 在发射物碰撞时通过统一的命中流水线转发事件给 Ability 系统。该流水线 MUST 在识别有效目标后构建统一的命中上下文，并将其提供给伤害处理与其他实现命中接口的可选扩展 Ability，而不是让每个扩展 Ability 各自重复解析碰撞对象。

#### Scenario: Valid collision is forwarded as hit context
- **WHEN** Projectile 的 Collider 与一个有效敌对目标发生碰撞
- **THEN** 系统必须构建本次命中的统一上下文
- **AND** 将该上下文转发给负责伤害和命中特效的相关 Ability

#### Scenario: Invalid collision is not forwarded as a valid hit
- **WHEN** Projectile 的 Collider 与友军、重复命中目标或无效对象发生碰撞
- **THEN** 系统不得把该碰撞作为有效命中转发给命中特效 Ability
