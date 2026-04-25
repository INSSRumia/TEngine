## 1. Ability 执行模型改造

- [x] 1.1 为 `Ability<T>` 增加执行模式声明，支持逻辑帧、物理帧与仅显式触发三类能力
- [x] 1.2 调整 `ASC<T>` 的能力注册与移除流程，维护全量列表、`Update` 列表与 `FixedUpdate` 列表
- [x] 1.3 保持 Ability 优先级排序规则在分流后仍然稳定生效

## 2. Marble 结算链路迁移

- [x] 2.1 将 `MarbleHandleDamageAbility` 改为通过显式结算入口触发，而不是逐帧轮询
- [x] 2.2 将 `MarbleDeadAbility` 改为在生命值结算后触发死亡判定
- [x] 2.3 将 `MarbleLevelUpAbility` 改为在经验变更后触发升级检测
- [x] 2.4 保留 `MarbleMoveAbility` 在 `FixedUpdate` 中的持续驱动，并确认其执行声明正确

## 3. 宿主接口与初始化收敛

- [x] 3.1 在 Marble 宿主侧提供少量统一的显式结算入口，避免外部直接调用单个 Ability
- [x] 3.2 更新 Marble 初始化流程，确保默认挂载的能力按新的执行模型注册
- [x] 3.3 检查 `MarbleFactory` 与相关调用方，确保结算入口和能力装配时序一致

## 4. 验证与回归

- [x] 4.1 验证伤害、治疗、死亡与升级在新触发模型下的结果与现状一致
- [x] 4.2 验证事件型 Ability 不再进入常规 `Update` / `FixedUpdate` 轮询
- [x] 4.3 运行项目现有的相关验证命令，确认本次重构未引入编译或运行时错误
