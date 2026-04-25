## Context

当前 Projectile 运行时已经形成了比较清晰的装配边界：`ProjectileFactory` 负责挂载移动、伤害、生命周期、追踪等固定骨架能力，`ProjectileLevelConfig.lst_ability` 预留给可选扩展能力。与此同时，`Projectile.cs` 的碰撞入口目前只委托给 `ProjectileDamageAbility.HandleCollider`，命中后的额外效果没有统一扩展点，代码里也只保留了一段被注释掉的 `AddForce` 临时代码。

这意味着“击退”虽然是一个很自然的命中特效，但如果直接塞回 `ProjectileDamageAbility`，就会把所有发射物命中都耦合成固定击退，并且后续如果要继续加减速、附着、爆炸标记等命中特效，还会重复复制目标校验、敌我判断和重复命中保护逻辑。

本次变更只需要交付最小可用版本，因此配置层面只引入一个 `force` 字段；但在运行时设计上，仍需要补齐一个可复用的命中扩展入口，避免后续继续返工。

## Goals / Non-Goals

**Goals:**
- 提供一个可挂载到 `ProjectileLevelConfig.lst_ability` 的 `ProjectileKnockbackAbility`。
- 让击退能力只依赖单个 `force` 配置字段即可工作。
- 保持现有 Projectile 固定骨架边界不变，不把击退混入 `move_ability`、`damage_ability` 或 `tracking` 固定入口。
- 为发射物命中后的扩展效果补齐统一的命中上下文与分发契约，使后续扩展能力可以复用同一条有效命中流水线。
- 维持与现有敌我判断、穿透次数、重复命中保护逻辑的一致性。

**Non-Goals:**
- 不在本次设计中引入额外字段，例如击退衰减、方向模式、仅作用 Marble、仅作用 Equipment、垂直分量或持续时间。
- 不修改 `ProjectileMoveAbility`、`ProjectileTrackingAbility` 的运动职责。
- 不重构为完整的通用 Buff / Debuff / 命中特效框架。
- 不在本次提案中直接修改 `projectile.xlsx` 表头；仅约束后续实现需要补充数据区内容。

## Decisions

### 决策 1：击退作为 `lst_ability` 可选扩展，而不是并入 `damage_ability`
- 方案选择：在 `projectile_ability.xml` 中新增 `ProjectileKnockbackConfig`，通过 `ProjectileLevelConfig.lst_ability` 选择性挂载。
- 原因：击退属于玩法扩展，不是所有发射物都必须具备的固定骨架能力。保持其处于可选能力列表，符合当前 Projectile 装配模型，也便于后续继续增加其他命中特效。
- 备选方案：在 `ProjectileDamageConfig` 中直接新增 `knockback_force`。放弃原因是会把击退绑定到所有拥有伤害能力的发射物上，削弱扩展能力边界。

### 决策 2：保留 `ProjectileDamageAbility` 作为有效命中校验入口，并在其中分发统一命中上下文
- 方案选择：新增轻量的命中接口与命中上下文，例如 `IProjectileHitHandler` 与 `ProjectileHitContext`。`Projectile.cs` 仍通过当前碰撞入口进入命中处理，但 `ProjectileDamageAbility` 在完成目标识别、敌我校验、重复命中保护前置判断后，向所有实现该接口的扩展 Ability 分发统一命中事件。
- 原因：现有有效命中判定已经集中在 `ProjectileDamageAbility` 中，若把击退判定拆到各个扩展 Ability，会导致相同的目标筛选逻辑被重复实现。保留单一命中校验入口，可以确保伤害、击退与未来命中特效共享同一套“什么算一次有效命中”的规则。
- 备选方案：把碰撞解析和命中分发完全迁移到 `Projectile.cs`。放弃原因是会让实体层承担过多战斗语义，并迫使 Projectile 直接理解具体玩法效果。

### 决策 3：`force` 表示一次性冲量强度，方向优先取发射物当前速度方向
- 方案选择：`ProjectileKnockbackAbility` 在处理命中时，优先使用 `Owner.Rigidbody.velocity.normalized` 作为击退方向；当速度过小无法可靠归一化时，再退回到“发射物位置指向受击目标位置”的方向。实际施加方式使用 `Rigidbody2D.AddForce(direction * force, ForceMode2D.Impulse)`。
- 原因：大多数发射物的视觉与手感都基于飞行方向，把当前速度方向作为击退基准最符合玩家直觉；保留位置向量兜底，可以避免低速或刚出生时出现零向量。
- 备选方案：固定使用发射源到目标的方向。放弃原因是它与曲线弹道、追踪弹或反弹弹的实际飞行方向可能不一致。

### 决策 4：击退只对当前命中流水线认可且具备刚体的目标生效
- 方案选择：`ProjectileKnockbackAbility` 仅消费已经通过有效命中判定的上下文；若目标没有 `Rigidbody2D`，则安全跳过，不中断伤害结算。
- 原因：这样可以与当前 `ProjectileDamageAbility` 已支持的 Marble / Equipment 目标集保持一致，同时避免为了首版击退能力引入额外的目标白名单字段。
- 备选方案：首版只支持 Marble。放弃原因是现有命中逻辑已经支持 Equipment，单独收窄目标集会引入额外分歧和配置心智负担。

### 决策 5：Factory 继续通过配置 creator 扩展点创建新 Ability
- 方案选择：在 `DefaultProjectileAbilityCreatorForConfig` 中增加 `ProjectileKnockbackConfig => ProjectileKnockbackAbility` 的映射，而不是在 Factory 主路径添加特殊 if/else。
- 原因：这与当前 Projectile 的追踪能力创建方式一致，能让新能力继续沿用统一的配置驱动装配模型。
- 备选方案：在 `AttachConfigAbilities` 内对击退能力做硬编码分支。放弃原因是会破坏已有 creator 扩展边界。

## Risks / Trade-offs

- [风险] `ProjectileDamageAbility` 同时负责伤害结算与命中事件分发，职责比现在更重。 → 缓解：把新增逻辑限制为“构建上下文 + 调用接口列表”，不把击退细节写回 `ProjectileDamageAbility`。
- [风险] 使用当前速度方向作为击退方向时，某些极低速或停止状态的发射物可能方向不稳定。 → 缓解：增加基于命中位置关系的兜底方向。
- [风险] 首版只有 `force` 字段，无法表达“只击退 Marble”或“不同目标不同系数”。 → 缓解：在 spec 中明确这是最小可用范围，后续如有玩法需求再通过新增字段扩展。
- [风险] `projectile-config` 现有规范文本与当前代码骨架存在一定历史偏差，新增提案时可能把旧问题一并暴露出来。 → 缓解：本次 delta 只修正与击退能力直接相关的 requirement，不借机扩大重构范围。

## Migration Plan

1. 在 `projectile_ability.xml` 中新增 `ProjectileKnockbackConfig`，仅包含 `force` 字段。
2. 由用户或后续实现阶段补充 `projectile.xlsx` 数据区中的 `lst_ability` 配置，不修改表头结构。
3. 执行 Luban 生成，确认新的配置类型与反序列化分支已生成。
4. 实现命中上下文、命中接口和 `ProjectileKnockbackAbility`，并接入 `ProjectileFactory`。
5. 通过 `dotnet build` 验证 `GameLogic.csproj`，确认新能力编译通过。

## Open Questions

- 首版是否需要把击退事件暴露给非伤害型发射物。目前设计默认复用 `ProjectileDamageAbility` 的有效命中流水线，因此要求发射物仍具备伤害入口。
- 如果后续出现“命中但不造成伤害，只产生物理效果”的发射物，是否需要把命中分发进一步上提到独立的 `ProjectileHitAbility`。本次先不展开。
