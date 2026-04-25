## Context

当前 Marble 升级系统（`MarbleLevelUpAbility.Resolve()`）只更新基础属性（HP、Shield、Attack、Defense、Scale、Mass、Speed），没有处理装备更新。当 Marble 升级时，装备仍保留创建时的等级配置，无法体现高等级 Marble 装备更强武器的预期。

现有代码结构：
- `MarbleFactory.CreateMarble()` 创建 Marble 时，通过 `AttachEquipment()` 根据等级配置创建装备
- `Marble` 类已有 `GetEquipmentSlotPoint()` 方法获取装备挂载点，但无装备存储
- `EquipmentFactory.CreateEquipment()` 负责创建各种类型装备（Armor/Bow/Sword）

## Goals / Non-Goals

**Goals:**
- 实现 Marble 升级时自动更新装备到对应等级
- 正确释放旧装备资源，避免内存泄漏
- 复用 `MarbleFactory.AttachEquipment` 的装备创建逻辑，减少重复代码

**Non-Goals:**
- 不修改现有装备配置表结构
- 不实现装备升级 UI 或手动换装功能
- 不处理装备耐久度/损坏逻辑

## Decisions

### Decision 1: 装备存储方式

**选择**：在 `Marble` 类中添加 `Dictionary<EquipmentSlot, Equipment>` 存储当前装备

**理由**：
- 简单直接，与现有的 `_slotPointMap` 模式一致
- 可快速根据 Slot 查找和替换装备
- Equipment 类型而非 ASC，可获得类型安全

**备选方案**：
- 存储在 `MarbleRuntimeData`：优点是运行时数据可序列化，缺点是增加数据类复杂度
- 使用 List 存储：缺点是查找需要遍历

### Decision 2: 装备更新触发点

**选择**：在 `MarbleLevelUpAbility.Resolve()` 升级属性后调用装备更新

**理由**：
- 集中管理，升级相关逻辑内聚
- 升级流程中属性更新和装备更新紧邻，逻辑清晰

**备选方案**：
- 在 `Marble` 的单独方法：需要外部调用者记住调用，增加耦合

### Decision 3: 装备创建逻辑复用

**选择**：将 `MarbleFactory.AttachEquipment` 的核心逻辑提取为 `EquipmentFactory` 的静态方法

**理由**：
- 工厂类本就应该负责创建，职责清晰
- `MarbleFactory.AttachEquipment` 保留作为便捷包装
- 升级时直接调用工厂方法，无需依赖 MarbleFactory

**具体设计**：
```csharp
// EquipmentFactory 新增方法
public static Equipment CreateEquipment(Marble ownerMarble, string configId, int level, EquipmentSlot slot)

// Marble 新增方法
public void UpdateEquipmentOnLevelUp(int newLevel)
```

### Decision 4: 旧装备销毁

**选择**：调用装备的 `Destroy()` 方法销毁 GameObject，并从存储中移除

**理由**：
- Unity 生命周期管理，使用标准 Destroy
- 需要确保装备的所有 Ability/组件正确清理

**注意**：需确认 ASC 基类是否已处理销毁逻辑

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| 装备创建/销毁性能开销 | 升级时频繁创建销毁可能卡顿 | 使用对象池（后续优化） |
| 旧装备事件/引用未清理 | 可能导致内存泄漏或异常 | 检查 ASC 基类销毁逻辑 |
| 升级时装备闪烁 | 视觉效果不连贯 | 考虑同时存在新旧装备过渡 |
| 多槽位同时更新 | 多个装备同时创建可能丢帧 | 分帧创建（后续优化） |
