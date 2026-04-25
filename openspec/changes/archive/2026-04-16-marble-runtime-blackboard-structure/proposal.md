## Why

当前 `MarbleRuntimeData` 同时混合了运行时状态、配置投影数据和帧级临时控制器对象，虽然读写方便，但已经开始让语义边界变得模糊，不利于后续网络同步、AI 理解和能力扩展。现在需要在不破坏“Runtime 作为黑板”的前提下，把 `MarbleRuntimeData` 内部结构化为更清晰的三层区域。

## What Changes

- 将 `MarbleRuntimeData` 重构为黑板式聚合根，内部明确拆分为运行时状态数据、配置数据和帧临时数据三部分。
- 保持 Ability 仍然通过统一的 `RuntimeData` 入口读写数据，不把配置数据和帧临时数据完全移出 Runtime 根对象。
- 将现有控制器类（例如各类 `PriorityValueManager`）归入帧临时数据区，而不是继续与可同步状态字段混放。
- 重新梳理 Marble 现有字段，把真正的状态、配置投影和临时控制数据分别归位。
- 为未来网络同步建立清晰边界：优先同步运行时状态，按需重建配置和帧临时层。

## Capabilities

### New Capabilities
- `marble-runtime-blackboard-model`: 定义 Marble Runtime 黑板模型，明确 `State / Config / Frame` 三层分区及其职责。
- `marble-runtime-sync-boundary`: 定义 Marble Runtime 中哪些数据属于同步态，哪些属于可重建的本地态。

### Modified Capabilities
- `marble-ability-execution-model`: 调整 Marble ability 对 RuntimeData 的访问模型，使能力通过结构化黑板访问状态、配置与帧临时数据。

## Impact

- 主要影响代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/MarbleRuntimeData.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/`
  - 依赖 `MarbleRuntimeData` 读写字段的 Marble abilities 与相关结算逻辑
- 暂不涉及：
  - `EquipmentRuntimeData`
  - `ProjectileRuntimeData`
- 影响未来方向：
  - Marble 网络同步设计
  - AI 辅助修改 RuntimeData 和 Ability 时的字段定位稳定性
