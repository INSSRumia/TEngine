## 1. 配置接入准备

- [x] 1.1 确认并消费用户已生成的 `CampConfig`、`InitialConfig`、`MarbleSpawnConfig` 与更新后的远征遭遇生成代码，不修改任何 xlsx 文件
- [x] 1.2 为运行时补齐 `InitialConfig -> CampConfig` 的解析入口与基础校验日志
- [x] 1.3 清理远征侧对 `ExpeditionEnemyMarbleConfig` 的残留依赖，统一切换到 `MarbleSpawnConfig`

## 2. 局外持久化初始化

- [x] 2.1 为 `MarblePersistentData` 增加 `camp_config_id` 对应字段，并提供从 `MarbleSpawnConfig` 构建默认持久化数据的入口
- [x] 2.2 重构 `ExpeditionPersistentDataStore.EnsureInitialized()`，在持久化数据为空时按 `InitialConfig -> CampConfig` 初始化资金与 Marble 列表
- [x] 2.3 保持“只初始化一次”的语义，确保已有局外持久化数据不会被开局配置重复覆盖

## 3. 远征入口与 Combat 桥接

- [x] 3.1 将远征入口和默认启动逻辑改为从当前 `CampConfig.LstExpedition` 选择可用远征，移除对写死远征常量的硬依赖
- [x] 3.2 更新 `CombatSessionRequest`、`ExpeditionFlowController` 与 `ExpeditionCombatSessionController`，统一使用 `MarbleSpawnConfig` 作为敌方编队输入
- [x] 3.3 在战斗生成链路中同时保留 `camp_config_id` 配置语义和战斗内 `CombatSide` 赋值语义

## 4. CombatSide 命名迁移

- [x] 4.1 将 `Gameplay.Combat` 域内用于敌我判定的 `Camp` 命名统一迁移为 `CombatSide`，覆盖 `MarbleRuntimeData`、`MarbleFactory` 和核心常量
- [x] 4.2 更新 `CombatManager`、Projectile、Equipment 命中判定链路以及 Combat 测试代码中的相关命名和比较逻辑
- [x] 4.3 更新远征与 Combat 的桥接层常量和调用点，确保不再将战斗敌我归属称为 `Camp`

## 5. 验证与收口

- [x] 5.1 完成全局检索，确认运行时代码不再残留对旧敌方配置结构和旧战斗 `Camp` 命名的关键引用
- [x] 5.2 运行 `dotnet build D:\\UnityProject\\Marbles Legion\\UnityProject\\GameLogic.csproj` 并处理本次变更引入的编译错误
- [x] 5.3 回归最小流程，验证“开局配置初始化 -> 进入远征 -> Combat -> 结算返回入口”在新配置链路下可正常跑通
