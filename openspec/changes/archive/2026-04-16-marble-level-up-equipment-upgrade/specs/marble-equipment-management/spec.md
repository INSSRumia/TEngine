## ADDED Requirements

### Requirement: Marble 持有装备存储
Marble 类 SHALL 提供存储当前持有装备的能力，按 EquipmentSlot 索引。

#### Scenario: 获取空装备槽
- **WHEN** 查询未持有装备的槽位
- **THEN** 返回 null

#### Scenario: 存储新装备
- **WHEN** 装备被创建并挂载到 Marble
- **THEN** 装备按其 Slot 存储在 Marble 的装备字典中

#### Scenario: 替换已有装备
- **WHEN** 同一槽位装备被替换
- **THEN** 旧装备从字典中移除，新装备存入字典

#### Scenario: 装备移除
- **WHEN** 装备从 Marble 移除（销毁）
- **THEN** 装备从字典中移除

### Requirement: Marble 装备销毁接口
Marble 类 SHALL 提供销毁指定槽位装备的方法。

#### Scenario: 销毁指定槽位装备
- **WHEN** 调用 `DestroyEquipment(EquipmentSlot slot)`
- **THEN** 如果该槽位有装备，调用其 GameObject.Destroy() 并从字典移除

#### Scenario: 销毁所有装备
- **WHEN** 调用 `DestroyAllEquipment()`
- **THEN** 所有装备从字典中销毁并移除

### Requirement: 装备创建时自动注册
当 EquipmentFactory 创建装备时 SHALL 自动将装备注册到所属 Marble 的装备字典。

#### Scenario: 装备创建后自动注册
- **WHEN** EquipmentFactory.CreateEquipment() 创建装备
- **THEN** 装备自动添加到 OwnerMarble 的装备字典对应槽位

### Requirement: 装备注册接口
Marble 类 SHALL 提供 RegisterEquipment(Equipment equipment) 方法用于注册装备。

#### Scenario: 注册装备到正确槽位
- **WHEN** 调用 RegisterEquipment(equipment)
- **THEN** 装备根据其 RuntimeData.Slot 注册到对应字典位置

#### Scenario: 注册时替换旧装备
- **WHEN** 注册的槽位已有装备
- **THEN** 先销毁旧装备，再注册新装备
