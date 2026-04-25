# Projectile Module 实现任务

## 1. 配置表

- [x] 1.1 创建 `Configs/GameConfig/Defines/projectile.xml`，定义发射物配置表结构
- [x] 1.2 在 `equip.xml` 的 `BowLevelConfig` 中添加 `projectile_config_id` 和 `projectile_level` 字段
- [x] 1.3 运行 Luban 生成配置表代码

## 2. 运行时数据

- [x] 2.1 创建 `EnumProjectileTrackingMode` 枚举类
- [x] 2.2 创建 `ProjectileRuntimeData` 运行时数据类，包含所有发射物属性
- [x] 2.3 实现 `TryMarkHit()` 方法防止重复命中

## 3. Projectile 核心实体

- [x] 3.1 创建 `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Projectile.cs`
- [x] 3.2 继承 `ASC<ProjectileRuntimeData>`
- [x] 3.3 实现 `OnTriggerEnter2D()` 碰撞检测逻辑
- [x] 3.4 实现 `Despawn()` 销毁方法
- [x] 3.5 添加 `OnDrawGizmos()` 调试可视化

## 4. ProjectileFactory 工厂

- [x] 4.1 创建 `ProjectileFactory.cs` 静态工厂类
- [x] 4.2 实现 `Spawn()` 泛型方法，通过 Ability 组合创建发射物
- [x] 4.3 实现 `Despawn()` 方法
- [x] 4.4 添加实例 ID 计数器

## 5. 发射物移动能力

- [x] 5.1 创建 `ProjectileMoveAbility.cs` 移动能力类
- [x] 5.2 实现 `IAbilityFixedUpdate` 接口
- [x] 5.3 实现直线飞行模式（TrackingMode = None）
- [x] 5.4 实现追踪目标模式（TrackingMode = Target）
- [x] 5.5 实现追踪点模式（TrackingMode = Point）
- [x] 5.6 实现转向速率限制

## 6. 发射物伤害能力

- [x] 6.1 创建 `ProjectileDamageAbility.cs` 伤害能力类
- [x] 6.2 实现 `IOnTriggerEnter` 接口处理碰撞伤害
- [x] 6.3 实现穿透计数逻辑

## 7. 发射物生命周期能力

- [x] 7.1 创建 `ProjectileLifetimeAbility.cs` 生命周期能力类
- [x] 7.2 实现 `IAbilityUpdate` 接口
- [x] 7.3 实现 MaxLifetime 倒计时
- [x] 7.4 实现超时时自动 Despawn

## 8. WeaponFireAbility 基类

- [x] 8.1 创建 `WeaponFireAbility.cs` 基类
- [x] 8.2 继承 `EquipmentAbility`，实现 `IAbilityUpdate`
- [x] 8.3 实现冷却管理逻辑
- [x] 8.4 实现射击间隔管理逻辑
- [x] 8.5 声明 `CanFire()` 抽象方法
- [x] 8.6 声明 `DoFire()` 抽象方法

## 9. BowFireAbility 弓发射能力

- [x] 9.1 创建 `BowFireAbility.cs`
- [x] 9.2 继承 `WeaponFireAbility`
- [x] 9.3 实现 `CanFire()` 检查瞄准状态
- [x] 9.4 实现 `DoFire()` 发射逻辑
- [x] 9.5 实现连射模式（调用 ProjectileFactory.Spawn 多次）
- [x] 9.6 实现散射模式（计算角度偏移）

## 10. 能力绑定

- [x] 10.1 在 `EquipmentFactory` 中为 `BowEquipment` 添加 `BowFireAbility` 到 Core Abilities
- [x] 10.2 在 `ProjectileFactory.Spawn()` 中为 Projectile 自动挂载 Core Abilities（移动、伤害、生命周期）
- [x] 10.3 验证发射流程正确工作

## 11. 测试与验证

- [x] 11.1 创建测试场景验证箭矢发射（Arrow.prefab + ProjectileTest.cs）
- [x] 11.2 测试直线飞行（arrow_basic 配置，TrackingMode = None）
- [x] 11.3 测试追踪飞行（arrow_tracking 配置，TrackingMode = Target）
- [x] 11.4 测试穿透机制（PiercingCount 字段）
- [x] 11.5 测试散射模式（ShootType = 1，ArrowCount > 1）
- [x] 11.6 测试连射模式（ShootType = 0，ArrowCount > 1）

## 12. 文档与清理

- [x] 12.1 更新 `战斗系统设计.md` 文档
- [x] 12.2 清理临时测试代码
