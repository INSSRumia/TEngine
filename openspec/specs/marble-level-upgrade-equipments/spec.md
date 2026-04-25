## ADDED Requirements

### Requirement: 升级时自动更新装备
当 Marble 升级成功时 SHALL 自动将所有装备更新为新等级对应的配置。

#### Scenario: 升级成功时更新装备
- **WHEN** MarbleLevelUpAbility.Resolve() 检测到升级条件满足并完成升级
- **THEN** 调用 UpdateEquipmentOnLevelUp(newLevel) 更新所有装备

#### Scenario: 获取新等级装备配置
- **WHEN** UpdateEquipmentOnLevelUp 被调用
- **THEN** 通过 MarbleFactory.GetMarbleLevelConfig() 获取新等级的配置

#### Scenario: 逐槽位替换装备
- **WHEN** 更新装备时
- **THEN** 遍历当前持有的所有装备槽位，销毁旧装备，创建新等级装备

### Requirement: 升级时装备更新复用创建逻辑
MarbleFactory.AttachEquipment 的装备创建逻辑 SHALL 被升级流程复用，避免代码重复。

#### Scenario: 复用装备创建逻辑
- **WHEN** 升级时需要创建新装备
- **THEN** 调用统一的装备创建接口，确保与创建时的行为一致

#### Scenario: 装备创建方法解耦
- **WHEN** 需要创建装备时（无论是首次创建还是升级）
- **THEN** 都能调用同一个工厂方法，确保配置读取和初始化逻辑一致

### Requirement: 升级完成后同步装备状态
装备更新完成后 SHALL 同步相关状态（如装备位置、父子关系）。

#### Scenario: 新装备正确挂载
- **WHEN** 升级后创建新装备
- **THEN** 装备自动挂载到正确的 Slot 点（通过 EquipmentMountAbility.OnAdd）

### Requirement: 升级失败时不更新装备
当升级条件不满足时 SHALL 不触发装备更新。

#### Scenario: 经验不足不升级
- **WHEN** 当前经验小于升级所需经验
- **THEN** 不调用装备更新方法

#### Scenario: 已达最高等级
- **WHEN** 新等级配置不存在（已达最高级）
- **THEN** 重置升级经验为 0，不调用装备更新方法
