## Why

当 Marble 升级时，当前只更新了基础属性（HP、防御、速度等），但没有同步更新装备。不同等级应该对应不同等级配置的装备，目前升级后仍然使用创建时的装备，导致高级 Marble 仍然装备低级武器，削弱了升级的价值感。

## What Changes

- **新增**：Marble 持有装备列表管理
  - 在 `Marble` 类中添加装备存储机制（`Dictionary<EquipmentSlot, ASC>` 或类似结构）
  - 升级时自动替换装备到新等级配置

- **新增**：Marble 升级时装备更新逻辑
  - `MarbleLevelUpAbility.Resolve()` 在升级后触发装备更新
  - 移除旧装备，创建并挂载新等级对应的装备

- **重构**：提取装备创建/销毁公共逻辑
  - `MarbleFactory.AttachEquipment` 和升级时的装备更新复用同一套逻辑
  - 可考虑在 `EquipmentFactory` 添加 `CreateEquipment` 重载或在 `Marble` 添加装备管理方法

## Capabilities

### New Capabilities

- `marble-equipment-management`: 管理 Marble 持有的装备生命周期
  - 存储当前装备（按插槽）
  - 提供装备添加/移除接口
  - 提供装备销毁方法
- `marble-level-upgrade-equipments`: 升级时自动更新装备
  - 升级时检测当前装备槽位
  - 销毁旧装备，创建新等级装备
  - 复用 `MarbleFactory.AttachEquipment` 的装备创建逻辑

### Modified Capabilities

- （无）

## Impact

### 受影响代码

- `Marble.cs`：新增装备存储和管理方法
- `MarbleRuntimeData.cs`：可能需要存储装备配置引用（用于升级时获取新配置）
- `MarbleLevelUpAbility.cs`：升级时触发装备更新
- `EquipmentFactory.cs`：可能需要新增销毁接口

### 系统影响

- 升级流程增加装备替换开销，需注意资源释放
- 装备实例数量增加，需确保正确释放旧装备资源
