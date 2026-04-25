# Proposal: 实现战斗管理器核心逻辑

## Why

当前项目已构建了完整的 Ability Component System (ASC) 战斗实体架构，包括 Marble（弹珠单位）、Damage Pipeline、事件系统等基础设施。然而 `ICombatManager` 接口的两个核心方法 `GetNearestEnemy()` 和 `GetTarget()` 均为空实现，导致战斗系统无法正常运转——单位无法识别敌人、无法寻敌、无法触发战斗流程。同时项目缺乏可运行的测试工具，无法验证战斗逻辑。

## What Changes

- **实现 CombatManager**：完成 `ICombatManager` 接口，将 `CombatManager` 注册为 GameModule，提供基于阵营的敌对单位搜索功能
- **完善 ASC.CombatManager 注入**：在 ASC 基类中注入 CombatManager 引用，使所有战斗实体能访问战斗管理服务
- **创建战斗测试组件**：编写 `CombatTestAuthoring`（Main 域）与 `CombatTestAuthoringBridge`（热更域）MonoBehaviour，允许在编辑器中配置士兵类型、阵营和数量，快速验证战斗逻辑

## Capabilities

### New Capabilities

- `combat-manager`: 战斗管理器核心功能，提供敌对单位搜索、目标锁定等基础服务
- `combat-test-bridge`: 热更域测试桥接组件，负责在热更层创建测试士兵并管理战斗测试流程
- `combat-test-authoring`: Main 域测试编辑器组件，提供可视化配置界面和场景中士兵的生成预览

## Impact

- **受影响模块**：`Assets\GameScripts\HotFix\GameLogic\GamePlay\Combat\CombatManager.cs`
- **依赖关系**：依赖现有的 ASC 实体系统、MarbleRuntimeData 阵营系统
- **影响范围**：战斗系统核心逻辑、编辑器测试工具
