# Projectile Module 设计文档

## Context

### 背景

当前战斗系统中，武器（如弓）已具备瞄准能力（`BowAimAbility`），但缺少发射机制。`BowAimAbility` 只负责旋转武器朝向目标并设置 `CanFire` 状态，但没有实际发射箭矢的逻辑。

发射物（箭矢、火球、魔法弹等）是远程攻击的核心载体，需要：
- 独立于 Marble 的生命周期管理
- 多种移动模式（直线、追踪、抛物线）
- 碰撞检测与伤害应用
- 对象池优化

### 现有架构

```
CombatManager     - 战场管理，获取最近敌人、敌我判断
     │
     ├── Marble (ASC<MarbleRuntimeData>)
     │    ├── Ability (Core/Optional/Dynamic)
     │    └── Equipment (挂载在槽位)
     │         ├── WeaponEquipment
     │         │    └── BowEquipment + BowAimAbility
     │         └── ArmorEquipment
```

### 约束

- Projectile **不注册**到 CombatManager（用户明确需求）
- 使用现有 `ASC<T>` 基类
- 使用现有 Ability 系统
- 通过 SourceCamp 进行敌我判断，而非依赖 CombatManager

## Goals / Non-Goals

**Goals:**
- 提供统一的发射物创建、销毁、生命周期管理
- 支持多种追踪模式（直线、追踪目标、追踪点）
- 支持穿透机制
- 支持连射和散射模式
- 通过 Luban 配置表驱动数据
- **通过 Ability 组合实现不同效果的发射物，不使用具体子类继承**

**Non-Goals:**
- 不实现复杂物理（抛物线弹道可后续扩展）
- 不实现对象池（本阶段简化实现）
- 不实现碰撞特效/音效（本阶段聚焦核心逻辑）
- 不实现爆炸范围伤害（本阶段仅单体）

## Decisions

### Decision 1: 单一 Projectile 类 + Ability 组合

**选择**：只有 `Projectile` 一个类，通过挂载不同的 Ability 实现不同效果

**理由**：
- 与现有 Marble/Equipment 架构保持一致
- 避免创建 ArrowProjectile、FireballProjectile 等具体子类
- 不同发射物通过组合 `ProjectileMoveAbility`、`ProjectileDamageAbility`、`ProjectileLifetimeAbility` 等实现差异化
- 配置表中定义 lst_ability，Factory 根据配置组合 Ability
- 未来扩展新效果只需新增 Ability，无需修改 Projectile

**备选**：
- 为每种发射物创建独立类（如 ArrowProjectile、FireballProjectile）：类爆炸，难以维护

### Decision 3: Projectile 继承 ASC<T>

**选择**：Projectile 继承 `ASC<ProjectileRuntimeData>`

**理由**：
- 复用现有的 Ability 容器和生命周期管理机制
- 保持与 Marble/Equipment 一致的架构模式
- 可在未来为 Projectile 添加 Ability（如爆炸 Ability、减速 Ability）

**备选**：
- 直接继承 MonoBehaviour：需要自行实现 Ability 调度，增加重复代码

### Decision 4: WeaponFireAbility 作为发射能力基类

**选择**：新增 `WeaponFireAbility` 基类，封装冷却和射击间隔逻辑

**理由**：
- 冷却逻辑已在 `WeaponCooldownAbility` 中实现，但它是更新冷却，本质是准备阶段
- 发射逻辑需要组合：检查冷却 → 检查瞄准状态 → 执行发射
- 抽取基类可复用于未来法杖、魔杖等远程武器

**备选**：
- 在 BowFireAbility 中直接实现：代码重复，难以扩展

### Decision 4: 追踪模式通过枚举控制

**选择**：`EnumProjectileTrackingMode` 枚举控制追踪行为

```csharp
public enum EnumProjectileTrackingMode
{
    None,   // 直线飞行
    Target, // 追踪目标 Marble
    Point,  // 追踪固定坐标点
}
```

**理由**：
- 简单清晰，易于扩展新模式
- 每个模式对应一种 FixedUpdate 行为
- 配置表中直接存储枚举值

**备选**：
- 策略模式：为每种追踪模式创建独立类
- 过度设计，当前场景不需要

### Decision 5: 敌我判断通过 SourceCamp

**选择**：ProjectileRuntimeData 保存 SourceCamp，碰撞时判断

```csharp
// 碰撞检测
if (target.RuntimeData.Camp == RuntimeData.SourceCamp)
    return; // 同阵营不造成伤害
```

**理由**：
- Projectile 不注册 CombatManager，需要独立判断敌我
- SourceCamp 在创建时从发射者 Marble 获取

### Decision 6: 穿透通过 HitTargets HashSet 实现

**选择**：使用 HashSet<int> 记录已命中的目标 InstId

```csharp
public bool TryMarkHit(int targetInstId)
{
    if (_hitTargets.Contains(targetInstId))
        return false;
    _hitTargets.Add(targetInstId);
    return true;
}
```

**理由**：
- HashSet 查找 O(1)，性能好
- 每次创建 Projectile 新建 HashSet，简洁

**备选**：
- 全局命中记录表：需要清理逻辑，增加复杂度

### Decision 7: ProjectileFactory 封装创建逻辑

**选择**：静态工厂类 `ProjectileFactory` 负责创建和销毁

**理由**：
- 封装 Prefab 加载逻辑
- 封装 RuntimeData 创建逻辑
- 未来可方便扩展对象池

## Risks / Trade-offs

**[风险] 发射物销毁时机不明确**
→ **缓解**：通过 MaxLifetime 和碰撞/穿透次数控制生命周期

**[风险] 大量发射物性能问题**
→ **缓解**：本阶段先简化实现，后续可扩展对象池

**[风险] 弓和发射物配置分离可能导致不一致**
→ **缓解**：BowLevelConfig 中明确引用 ProjectileConfigId，创建时校验

## Migration Plan

### 阶段一：基础框架
1. 创建 `projectile.xml` Luban 配置表（包含 ProjectileLevelConfig 和各 Ability 配置）
2. 扩展 `equip.xml` 中 BowLevelConfig
3. 实现 `ProjectileRuntimeData`
4. 实现 `Projectile` 核心类
5. 实现 `ProjectileFactory`（通过配置组合 Ability）

### 阶段二：发射物 Ability
1. 实现 `ProjectileMoveAbility`（移动能力）
2. 实现 `ProjectileDamageAbility`（伤害能力）
3. 实现 `ProjectileLifetimeAbility`（生命周期能力）

### 阶段三：武器发射能力
1. 实现 `WeaponFireAbility` 基类
2. 实现 `BowFireAbility`

### 阶段四：能力绑定
1. 在 `EquipmentFactory` 中为 `BowEquipment` 添加 `BowFireAbility`
2. 在 `ProjectileFactory` 中根据配置为 Projectile 挂载 Core Abilities
3. 验证发射流程

## Open Questions

1. **发射物 Prefab 如何管理？**
   - 暂定通过配置表 PrefabPath 加载
   - 是否需要 VFX/SFX 挂载点？

2. **法杖等远程武器是否复用 WeaponFireAbility？**
   - 基本结构可复用，但可能需要差异化参数
   - 暂定支持，后续扩展

3. **对象池何时引入？**
   - 本阶段先简化实现
   - 性能测试后决定是否引入
