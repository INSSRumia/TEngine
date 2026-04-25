# Design: 实现战斗管理器核心逻辑

## Context

项目已构建了基于 ASC (Ability Component System) 的战斗实体架构：

- **MarbleRuntimeData**：弹珠单位数据，包含 `Camp`（阵营ID）、生命值、攻击力、防御力等
- **ASC<T>**：实体基类，通过泛型管理 Ability 生命周期调度
- **16 个 Ability 接口**：覆盖伤害/治疗/护盾管线，支持事件钩子
- **Damage Pipeline**：完整的四阶段伤害计算（Receive → Calculate → Apply → Completed）
- **MarbleFactory**：提供 `CreateMarble()` 静态方法创建测试士兵

**当前问题**：`ICombatManager` 的两个方法均为空实现，ASC 无法获取 CombatManager 引用，导致战斗系统无法运行。

## Goals / Non-Goals

**Goals:**
- 实现 `CombatManager` 并注册为 GameModule，提供基于阵营的敌对单位搜索
- 在 ASC 基类中注入 CombatManager 引用
- 提供可运行的编辑器测试工具（Main 域 + 热更域桥接）

**Non-Goals:**
- 不实现复杂的 AI 寻路系统
- 不实现配置化的阵营关系表（阵营敌对关系硬编码）
- 不实现完整的战斗流程循环（回合制/即时制）
- 不实现 UI 展示（血条、伤害数字等）

## Decisions

### Decision 1: 阵营敌对关系如何判断？

**选择方案**：简单阵营 ID 直接比较

当前 `MarbleRuntimeData.Camp` 为 int 类型，最简单的方案是在 `CombatManager` 中硬编码阵营判断逻辑：

```csharp
// 同阵营 = 友军，不同阵营 = 敌军
if (a.Camp == b.Camp) { /* 友军 */ }
else { /* 敌军 */ }
```

**备选方案**：
- 阵营关系配置表（阵营矩阵）：可配置任意阵营间的敌对/中立/友好关系，灵活但复杂度过高

**理由**：MVP 阶段阵营数量有限（2-3 个），直接比较足够满足需求。

---

### Decision 2: 敌对单位搜索策略

**选择方案**：全场景遍历 + 距离排序

```csharp
public Marble GetNearestEnemy(Marble marble)
{
    // 1. 获取所有存活的敌方 Marble
    // 2. 按距离排序
    // 3. 返回最近的
}
```

**备选方案**：
- 空间分区（QuadTree/Grid）：适合大量单位，减少搜索复杂度
- 维护活跃单位列表：需要额外管理生命周期

**理由**：当前为 MVP，不需要处理大量单位，全遍历性能可接受。

---

### Decision 3: CombatManager 如何注入到 ASC

**选择方案**：静态实例注入 + 属性访问

在 `GameApp.Awake()` 中创建 CombatManager 单例，ASC 通过 `ASC.CombatManager` 属性访问：

```csharp
public class ASC : MonoBehaviour
{
    public ICombatManager CombatManager { get; private set; }
}
```

**备选方案**：
- 构造函数注入：需要修改 ASC 创建流程
- Service Locator：`GameModule.GetModule<ICombatManager>()`

**理由**：与现有 `GameModule` 访问模式一致，改动最小。

---

### Decision 4: 测试组件架构（Main + HotFix 分离）

**选择方案**：Editor 组件（Main 域）负责配置和预览，Bridge 组件（热更域）负责实际逻辑

- **CombatTestAuthoring**（Main 域）：挂载在场景 GameObject 上，提供 `[Header]` 配置字段，在编辑器中预览士兵布局
- **CombatTestAuthoringBridge**（热更域）：通过 `SendMessage` 或接口调用接收配置，创建 Marble 并管理测试流程

**备选方案**：
- 纯热更域测试：无法利用 Unity 编辑器可视化配置
- 纯 Main 域测试：违反 TEngine 热更架构

**理由**：TEngine 强制 Main/HotFix 分离，测试工具需要跨越两个域工作。

## Risks / Trade-offs

| 风险 | 描述 | 缓解措施 |
|------|------|---------|
| 阵营扩展性 | 后续需要多阵营支持时，硬编码判断需重构 | 预留 `IsEnemy()` 方法，后续可替换为阵营关系表 |
| 性能 | 全场景遍历在单位数量多时性能下降 | MVP 阶段可接受，后续按需优化为空间分区 |
| 测试工具与正式逻辑耦合 | Bridge 组件可能被误用于正式游戏 | Bridge 标记 `[ExecuteInEditMode]`，发布前检查 |

## Open Questions

1. **Marble 的寻敌触发条件**？是每帧检测，还是基于碰撞事件？
   - 当前 `SwordCollisionAttackAbility` 使用碰撞检测，可复用此模式
2. **战斗测试场景的边界**？士兵出界是否销毁？
   - MVP 阶段不处理，边界由场景 Layout 保证
3. **MarbleFactory.CreateMarble() 的参数**：需要哪些配置参数？
   - 基于现有 `MarbleConfig` 数据表，需要提供 `ConfigId` 和 `Camp`
