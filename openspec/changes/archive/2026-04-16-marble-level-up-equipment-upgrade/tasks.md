## 1. Marble 装备存储基础

- [x] 1.1 在 `Marble.cs` 中添加 `Dictionary<EquipmentSlot, Equipment> _equipmentMap` 字段
- [x] 1.2 在 `Marble.cs` 中添加 `GetEquipment(EquipmentSlot slot)` 方法
- [x] 1.3 在 `Marble.cs` 中添加 `RegisterEquipment(Equipment equipment)` 方法
- [x] 1.4 在 `Marble.cs` 中添加 `UnregisterEquipment(EquipmentSlot slot)` 方法
- [x] 1.5 在 `Marble.cs` 中添加 `DestroyEquipment(EquipmentSlot slot)` 方法
- [x] 1.6 在 `Marble.cs` 中添加 `DestroyAllEquipment()` 方法

## 2. EquipmentFactory 改造

- [x] 2.1 在 `EquipmentFactory.cs` 中添加 `CreateEquipment()` 静态方法（返回 Equipment 类型）
- [x] 2.2 保留现有的 `CreateEquipment()` 重载（返回 ASC 类型）以保持兼容性
- [x] 2.3 在新创建方法中调用 `ownerMarble.RegisterEquipment()` 注册装备

## 3. MarbleFactory 重构

- [x] 3.1 重构 `AttachEquipment()` 方法，使用新的 `EquipmentFactory.CreateEquipment()` 方法
- [x] 3.2 确保重构后行为与原有逻辑一致

## 4. Marble 升级装备更新

- [x] 4.1 在 `MarbleLevelUpAbility.cs` 中添加 `UpdateEquipmentOnLevelUp(int newLevel)` 方法
- [x] 4.2 在 `MarbleLevelUpAbility.Resolve()` 升级成功后调用 `UpdateEquipmentOnLevelUp()`
- [x] 4.3 实现遍历当前装备槽位、销毁旧装备、创建新等级装备的逻辑

## 5. 验证和测试

- [x] 5.1 验证 Marble 创建时装备正确挂载和存储
- [x] 5.2 验证 Marble 升级时装备正确更新到新等级
- [x] 5.3 验证旧装备正确销毁（无内存泄漏）
- [x] 5.4 验证装备挂载点位置正确
