## 1. Factory 装配骨架统一

- [x] 1.1 统一 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 的 creator 列表、注册入口和配置能力创建入口结构
- [x] 1.2 统一三个 Factory 中固定骨架能力与配置驱动能力的装配顺序和方法命名
- [x] 1.3 清理工厂层的重复挂载、返回值无效和作用域错误等结构性问题

## 2. Timing 配置工厂化

- [x] 2.1 新增独立的 `AbilityTimingFactory`，统一 `AbilityTimingConfig` 到 `IAbilityTiming` 的映射
- [x] 2.2 将 Marble 定时能力 creator 中分散的 timing 创建逻辑切换到共享 Timing Factory
- [x] 2.3 清理与 Timing 工厂化不一致的中间态接口和错误抽象

## 3. 默认能力与配置能力边界收敛

- [x] 3.1 调整 `MarbleFactory`，让配置能力通过统一的配置挂载入口完成装配
- [x] 3.2 调整 `ProjectileFactory`，让追踪类等配置能力通过统一入口挂载
- [x] 3.3 调整 `EquipmentFactory`，明确 creator 只负责默认骨架，配置扩展由工厂统一挂载

## 4. EquipmentFactory 深度整理

- [x] 4.1 拆分 `Armor`、`Bow`、`Sword` 的默认骨架挂载逻辑，降低 `DefaultEquipmentCreatorForConfig` 的体积
- [x] 4.2 收敛装备类型主能力与配置扩展能力之间的边界，避免默认逻辑继续吞噬配置职责
- [x] 4.3 校正装备相关配置到能力构造的映射错误，并为后续扩展预留稳定结构

## 5. 验证与回归

- [x] 5.1 在 Unity 中刷新资源并重新生成 `csproj`
- [x] 5.2 在 Unity 编译链中验证新增文件和重构后的 Combat Factory 均可通过编译
- [x] 5.3 回归检查 Marble、Equipment、Projectile 的核心战斗对象创建流程未被破坏
