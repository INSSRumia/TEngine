# combat-test-authoring Specification

## Purpose
TBD - created by archiving change implement-combat-manager. Update Purpose after archive.
## Requirements
### Requirement: 编辑器可视化配置界面

`CombatTestAuthoring` SHALL 作为 MonoBehaviour 组件挂载在场景 GameObject 上，提供 `[Header]` 配置字段，允许设计师在 Inspector 面板中配置：

- `Camp1ConfigId`: 阵营1的士兵配置ID
- `Camp1Count`: 阵营1的士兵数量
- `Camp2ConfigId`: 阵营2的士兵配置ID
- `Camp2Count`: 阵营2的士兵数量
- `SpawnAreaCenter`: 生成区域中心点（Transform 或 Vector3）
- `SpawnAreaRadius`: 生成区域半径
- `SpawnInterval`: 生成间隔（秒），0 表示同时生成

#### Scenario: Inspector 配置
- **WHEN** 设计师在 Inspector 中设置 `Camp1Count = 5`、`Camp2Count = 3`
- **THEN** 配置被序列化保存
- **AND** 可在编辑器运行时触发测试

### Requirement: 编辑器内士兵预览

组件 SHALL 在编辑器模式下（`[ExecuteInEditMode]`）提供预览功能，在 Scene 视图中用 Gizmos 绘制生成区域，并显示预计生成的士兵位置。

#### Scenario: Scene 视图中预览
- **WHEN** 组件存在于场景中
- **AND** 设计师在 Inspector 中调整 `SpawnAreaRadius`
- **THEN** Scene 视图中实时显示生成区域的圆形范围

### Requirement: 开始战斗测试

组件 SHALL 提供 `StartTest()` 公开方法，根据当前配置通过热更域桥接组件创建士兵并开始战斗测试。

#### Scenario: 启动测试
- **WHEN** 在编辑器运行时点击 "Start Test" 按钮或调用 `StartTest()`
- **THEN** 系统向热更域发送配置数据
- **AND** 桥接组件创建对应数量和阵营的士兵
- **AND** 士兵在指定的生成区域内随机分布

### Requirement: 停止战斗测试

组件 SHALL 提供 `StopTest()` 公开方法，清理所有测试士兵并重置测试状态。

#### Scenario: 停止测试
- **WHEN** 调用 `StopTest()`
- **THEN** 桥接组件的 `ClearAllSoldiers()` 被调用
- **AND** 场景中所有测试士兵被销毁
- **AND** 测试状态重置为待命

### Requirement: 测试状态显示

组件 SHALL 在 Inspector 中显示当前测试状态（待命/运行中）和活跃士兵数量。

#### Scenario: 运行时状态显示
- **WHEN** 测试运行中
- **THEN** Inspector 显示 "Running" 状态
- **AND** 显示当前存活的士兵数量（按阵营分组）

### Requirement: 生成位置随机化

士兵 SHALL 在指定的生成区域（圆形区域）内随机分布生成，位置通过 `Random.insideUnitCircle * SpawnAreaRadius + SpawnAreaCenter` 计算。

#### Scenario: 士兵随机分布
- **WHEN** `SpawnAreaRadius = 5`、`SpawnAreaCenter = (0, 0)`
- **THEN** 士兵位置均匀分布在半径为 5 的圆内
- **AND** 阵营1和阵营2的士兵在各自区域生成（可通过子区域配置区分）

