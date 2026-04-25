## 1. 时序抽象与基类

- [x] 1.1 在 Marble Ability 目录下新增 `TimedMarbleAbility` 基类，封装时序策略持有、注入、重置与 `IAbilityUpdate` 推进逻辑
- [x] 1.2 定义最小可用的 `IAbilityTiming` 接口，包含状态查询、更新时间、重置与触发入口
- [x] 1.3 为时序策略补充至少一种固定时间实现，并明确手动触发与自动循环的接入方式

## 2. 与现有执行模型对接

- [x] 2.1 确保 `TimedMarbleAbility` 派生类可同时参与 `Update` 与 `FixedUpdate`，无需修改 `ASC` 分发机制
- [x] 2.2 在定时移动类 Ability 的实现约束中落实“仅激活期持续向 Manager 提交值，结束后停止提交”的规则
- [x] 2.3 校验与 `MarbleMovementAbility`、`PriorityValueManager`、现有优先级/合成逻辑的兼容性

## 3. 冲刺能力落地示例

- [x] 3.1 新增一个基于 `TimedMarbleAbility` 的冲刺 Ability，激活期间持续提高 `TargetVelocity` 与 `Acceleration`
- [x] 3.2 决定并实现冲刺方向策略（如激活时锁定方向或实时追踪目标），避免与时序策略职责混淆
- [x] 3.3 为冲刺 Ability 接入固定冷却或自动循环示例，作为后续随机时序扩展的参考实现

## 4. 验证

- [x] 4.1 为时序状态流转补充可验证用例，覆盖激活、持续结束、冷却完成与再次触发
- [x] 4.2 为冲刺 Ability 补充行为验证，确认仅在激活期内持续提交移动意图值
- [x] 4.3 运行与本次改动相关的编译/测试验证，确认新增抽象未破坏现有 Combat 执行链


