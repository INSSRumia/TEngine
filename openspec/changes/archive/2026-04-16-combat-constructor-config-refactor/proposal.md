## Why

当前 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 在创建 `RuntimeData` 和 `Ability` 时普遍依赖长参数构造函数，导致大量配置字段细节泄漏到 Factory 层，持续放大工厂类的装配代码体积与修改面。现在需要把配置读取职责回收到 `RuntimeData` 和 `Ability` 自身，让 Factory 只负责选择配置、传递宿主上下文和组织装配流程。

## What Changes

- 将战斗域中通过多参数构造函数创建的 `RuntimeData` 和 `Ability` 改为优先接收对应 `config` 对象构造。
- 为所有可装配 Ability（包括当前无初始化参数的固定骨架能力）补齐 XML 配置定义，并统一由 Luban 生成配置类，使每个能力都有明确配置载体。
- 收敛 Factory 的字段读取逻辑，避免 Factory 继续直接展开 `config.xxx.yyy` 的细节并拼接大量构造参数。
- 统一 `RuntimeData` / `Ability` 的配置消费模式，让构造函数内部负责从配置中提取并保存运行所需字段。
- **BREAKING**：受影响的 `RuntimeData`、`Ability` 构造函数签名将发生调整，Factory 与 creator 实现需要同步迁移。

## Capabilities

### New Capabilities
- `combat-constructor-config-contract`: 定义战斗域 `RuntimeData` 与 `Ability` 通过配置对象构造的统一契约，以及所有可装配 Ability 的 Luban 配置类补齐规则

### Modified Capabilities
- `combat-factory-assembly-model`: 工厂装配要求从“展开字段并传多参数”调整为“选择配置对象并传入构造函数”，减少 Factory 对配置细节的直接依赖

## Impact

- 受影响代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Equipment/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/`
- 受影响配置定义：
  - `Configs/GameConfig/Defines/*.xml`
- 受影响生成物：
  - `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/`
- 受影响对象：
  - 各类 `RuntimeData`
  - 各类 `Ability`
  - `CreatorForConfig` / Factory 装配流程
