## Why

当前项目中部分游戏对象初始化依赖 MonoBehaviour 默认生命周期顺序（Awake/Start/OnEnable），在复杂场景与跨对象依赖下容易出现时序不稳定、空引用和初始化竞态。需要引入统一调度机制，将实体业务生命周期从 Unity 默认顺序中解耦，提升可控性与可维护性。

## What Changes

- 新增 `EntityModule`，统一管理实体注册、反注册、排序与生命周期调度。
- 新增 `Entity` 基类（继承 `MonoBehaviour`），提供一套由 `EntityModule` 驱动的自定义生命周期函数。
- 为 `Entity` 增加 `Priority` 属性，支持按优先级确定生命周期执行顺序。
- 约束 `Entity` 的 Unity 生命周期职责：仅用于桥接注册/解绑，不承载跨实体业务初始化依赖。
- 增加生命周期调用阶段与安全规则（遍历期间增删、销毁对象跳过、统一清理）。

## Capabilities

### New Capabilities
- `entity-lifecycle-module`: 提供实体统一生命周期调度能力，包括注册管理、优先级排序、阶段化回调和安全清理。

### Modified Capabilities
- 无

## Impact

- 影响代码范围：`HotFix` 侧模块系统（新增 `EntityModule`）、基础实体脚本（新增 `Entity` 基类）、可能需要接入 `GameModule` 门面访问。
- 影响运行时行为：实体更新顺序由 `EntityModule` 明确控制，不再依赖 Unity 默认脚本执行顺序。
- 对外 API 影响：新增实体生命周期接口与优先级约定；旧逻辑若依赖 Awake/Start 顺序需逐步迁移。
