# Tasks: 实现战斗管理器核心逻辑

> **简化说明**：根据快速测试需求，采用简化方案：
> - CombatTest 组件直接创建在热更域，无需 Main 域桥接
> - 通过 Context Menu 快速操作，无需复杂 UI

## 1. 实现 CombatManager 核心逻辑

- [x] 1.1 完善 `ICombatManager` 接口，添加 `Register/Unregister`、`IsEnemy` 方法声明
- [x] 1.2 实现 `CombatManager` 类，包含活跃 Marble 列表管理
- [x] 1.3 实现 `GetNearestEnemy()`：遍历活跃单位、筛选敌对阵营、距离排序、返回最近者
- [x] 1.4 实现 `GetTarget(int instId)`：根据实例ID查找 Marble
- [x] 1.5 实现 `IsEnemy(Marble a, Marble b)`：阵营ID比较判断敌对关系
- [x] 1.6 CombatManager 单例模式（通过 Instance 属性访问）

## 2. 完善 ASC 基类注入

- [x] 2.1 在 `ASC` 基类中添加 `CombatManager` 属性
- [x] 2.2 在 ASC Awake 时获取 CombatManager.Instance 引用并赋值

## 3. 实现简化测试组件

- [x] 3.1 创建 `CombatTest` MonoBehaviour 类（热更域）
- [x] 3.2 实现 `SpawnAllSoldiers()` / `SpawnCamp1Soldiers()` / `SpawnCamp2Soldiers()` 方法
- [x] 3.3 实现 `ClearAllSoldiers()` 方法
- [x] 3.4 实现 `PrintCombatStatus()` 调试方法
- [x] 3.5 添加 Inspector 配置字段（ConfigId、Camp、Count、SpawnRadius）
- [x] 3.6 实现 Scene 视图 Gizmos 预览

## 4. 测试验证

- [ ] 4.1 在场景中挂载 CombatTest 组件并配置参数
- [ ] 4.2 在编辑器 Play 模式下验证士兵生成
- [ ] 4.3 验证两阵营士兵能正确识别敌对关系
