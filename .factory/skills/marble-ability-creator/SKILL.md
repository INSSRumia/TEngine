---
name: marble-ability-creator
description: 在本项目中新增或修改 Marble 扩展 Ability 时必须使用的技能。适用于用户提到 marble ability、MarbleAbility、lst_ability、marble_ability.xml、Luban 配置、Combat Marble 行为扩展、接近/远离/冲刺/朝向/移动策略等场景。该技能负责把“配置定义 → Luban 生成 → 运行时 Ability 实现 → MarbleFactory 接入 → 数据表补齐 → 构建验证”串成固定流程，避免只改一半导致配置或运行时代码失配。
---

# Marble Ability 创建技能

用于在 `Gameplay.Combat.Marble` 下新增或调整配置驱动的扩展 Ability。

## 适用范围

当任务符合以下任一条件时，优先使用本技能：
- 新增一个 `MarbleXxxAbility`
- 修改 `close_to_target`、`dash`、`face_target_direction`、`keep_away_from_target` 这类 `lst_ability` 扩展能力
- 需要改 `Configs/GameConfig/Defines/marble_ability.xml`
- 需要执行 Luban 生成并更新 `GameProto`
- 需要把新配置接入 `MarbleFactory.CreateAbilityFromConfig`

如果改动是 Marble 固定骨架能力，而不是 `lst_ability` 扩展能力，先确认是否应进入 `AttachDefaultAbilities`，不要误用本技能。

## 先建立心智模型

开始前先确认这几个事实：
- `Configs/GameConfig/Defines/marble_ability.xml` 定义 Marble 扩展 Ability 的 schema
- `Configs/GameConfig/Datas/marble.xlsx` 里 `lst_ability` 是实际数据入口
- `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/` 是 Luban 生成代码，禁止手写业务逻辑
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/Ability/` 放运行时扩展 Ability
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/MarbleFactory.cs` 负责把配置映射到 Ability 实例

## 标准流程

### 1. 先判断属于哪类能力

先回答：
- 这是 `lst_ability` 扩展能力吗？
- 还是所有 Marble 必备的固定骨架能力？
- 是否需要 `Timing`？
- 写入的是 `State`、`Config` 还是 `Frame`？

扩展行为通常遵循现有模式：
- Ability 自己只产出“行为意图”
- 通过 `RuntimeData.Frame.*Manager` 写入方向、速度、角速度、加速度
- 由 `MarbleMovementAbility` / `MarbleRotationAbility` 等核心能力统一消费

不要在扩展 Ability 里直接复制底层移动或结算骨架。

### 2. 先对照现有实现

优先阅读相近能力，至少查看：
- `MarbleCloseToTargetAbility.cs`
- `MarbleKeepAwayFromTargetAbility.cs`
- `MarbleDashAbility.cs`
- `Core/TimedMarbleAbility.cs`
- `Timing/AbilityTimingFactory.cs`
- `MarbleFactory.cs`

如果用户描述的是“接近目标的反向逻辑”，优先复用现有能力结构而不是从零发明新模式。

### 3. 修改 XML schema

在 `Configs/GameConfig/Defines/marble_ability.xml` 中：
- 新增 `MarbleXxxAbilityConfig`
- 使用清晰的 `alias`
- 字段顺序尽量贴近同类能力
- 如果能力需要定时激活/冷却，加入 `timing : AbilityTimingConfig`

命名建议：
- C# 类：`MarbleXxxAbility`
- 配置类：`MarbleXxxAbilityConfig`
- alias：简短、稳定、偏行为语义，例如 `keep_away_from_target`

### 4. 检查并补齐数据表

这是高频漏项，必须主动检查。

修改 schema 后，立即检查：
- `Configs/GameConfig/Datas/marble.xlsx` 中所有受影响的 `lst_ability`
- 旧数据是否缺字段
- 新增字段是否需要给已有行补默认值

尤其是给现有 bean 增加新字段时，Luban 往往会因为旧数据格式不匹配而失败。

如果 Excel 正被占用，优先停止继续编码，先解决数据表锁定或让用户关闭文件。

### 5. 执行 Luban 生成

按项目固定方式执行：

```bat
Configs\GameConfig\gen_code_json_to_project_lazyload.bat
```

如果批处理不便直接观察输出，可改用等价 `dotnet Luban.dll ...` 命令执行，但要保持参数一致。

生成后检查：
- 目标配置类是否已生成
- `MarbleAbilityConfig.DeserializeMarbleAbilityConfig` 是否包含新类型分支
- 若新增了字段，生成类里是否已出现对应字段与 `ResolveRef`

### 6. 实现运行时 Ability

在 `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/Ability/` 下新增或修改类：
- 普通持续行为：`MarbleAbility + IAbilityFixedUpdate`
- 带定时激活/冷却：优先继承 `TimedMarbleAbility`

实现时遵循这些规则：
- 构造函数中读取 config 并设置 `Priority`、`CombineType`
- 需要 Timing 时调用 `InitializeTiming(AbilityTimingFactory.CreateTiming(config.Timing))`
- `OnAbilityFixedUpdate` 或 `OnAbilityUpdate` 开头先做 Owner/RuntimeData/Alive 判空
- 定时能力要检查 `IsActive`
- 行为意图写入 `RuntimeData.Frame.*Manager`

常见写法示例：

```csharp
Priority = config.Priority;
CombineType = (EnumCombineType)config.CombineType;
InitializeTiming(AbilityTimingFactory.CreateTiming(config.Timing));
```

### 7. 接入 MarbleFactory

更新 `DefaultMarbleAbilityCreatorForConfig`：
- 增加 `MarbleXxxAbilityConfig => new MarbleXxxAbility(...)`

如果忘了这一步，配置会加载成功但运行时无法创建 Ability。

### 8. 检查是否影响示例数据

如果任务要求能立即在现有 marble 上生效，确认：
- `marble.xlsx` 已给对应 Marble 配上新 ability
- `lst_ability` 的 lite 格式与 schema 完全匹配

### 9. 做最小可用验证

文件改动后至少执行：

```bash
dotnet build "D:\UnityProject\Marbles Legion\UnityProject\GameLogic.csproj" -nologo
```

如果本次改动涉及 Luban，必须先确保 Luban 生成成功，再做 `dotnet build`。

汇报时区分：
- 新错误
- 既有 warning
- 与本次改动无关的问题

## 处理 Timing 的专项规则

当用户说“给某个 ability 加 Timing”时，默认按以下顺序处理：
1. 在 `marble_ability.xml` 的对应 config bean 中增加 `timing`
2. 检查 `marble.xlsx` 旧数据并补齐 timing 参数
3. 重新跑 Luban
4. 让能力继承 `TimedMarbleAbility`
5. 构造函数中初始化 timing
6. 执行逻辑前判断 `IsActive`

不要只改运行时代码而忘记 schema 和数据表。

## 输出要求

完成后用简明中文汇报：
- 改了哪些文件
- 新能力或新配置做了什么
- 是否执行了 Luban
- 是否执行了构建验证
- 是否存在与本次改动无关的遗留 warning

## 失败排查清单

如果 Luban 失败，优先排查：
- `marble.xlsx` 的 `lst_ability` 旧数据缺字段
- Excel 文件被占用，存在 `~$marble.xlsx`
- alias 与 bean 名不匹配
- `timing` 嵌套格式不合法

如果运行时不生效，优先排查：
- `MarbleFactory` 是否接入 creator
- ability 是否真正挂进了 `lst_ability`
- `Priority` / `CombineType` 是否正确
- 是否因为 `IsActive == false` 导致逻辑未执行
