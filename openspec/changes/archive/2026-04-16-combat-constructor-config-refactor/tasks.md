## 1. 配置契约梳理

- [x] 1.1 盘点 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 中所有通过多参数构造创建 `RuntimeData` 和 `Ability` 的位置
- [x] 1.2 标记哪些参数来自 Luban 配置，哪些参数属于运行时上下文，形成“config + 最小上下文”迁移边界
- [x] 1.3 为所有被装配 Ability（包括当前无初始化参数的固定骨架能力）补齐 XML 配置定义与生成类

## 2. RuntimeData 构造收敛

- [x] 2.1 重构 `MarbleRuntimeData` 等受影响运行态对象的构造函数，使其直接接收对应配置对象
- [x] 2.2 将 Factory 中对运行态字段的逐项赋值迁移到 `RuntimeData` 内部
- [x] 2.3 保留并明确非配置上下文字段的注入方式，避免重新引入离散 config 参数

## 3. Ability 构造收敛

- [x] 3.1 重构装备域中受影响能力的构造函数，使其通过配置对象读取字段
- [x] 3.2 重构发射物域中受影响能力的构造函数，使其通过配置对象读取字段
- [x] 3.3 重构 Marble 域中受影响能力与定时能力初始化路径，使其通过配置对象读取字段

## 4. Factory 与 Creator 迁移

- [x] 4.1 更新默认 Factory 主路径，移除成组的 `config.xxx` 字段展开构造
- [x] 4.2 更新各类 `CreatorForConfig` 接口与默认实现，使其统一传递配置对象
- [x] 4.3 清理迁移后失效的旧构造函数与中间映射样板代码

## 5. 生成与验证

- [x] 5.1 更新 Luban XML 定义与生成代码，确保所有被装配 Ability 的配置类都可被正确生成
- [x] 5.2 重新生成配置代码并检查 Factory / Ability / RuntimeData 编译通过
- [x] 5.3 运行受影响项目的构建验证，确认新的构造契约未破坏现有战斗装配流程
