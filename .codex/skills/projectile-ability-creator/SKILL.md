---
name: projectile-ability-creator
description: 在本项目中新增或修改 Projectile 配置驱动 Ability 时必须使用的技能。适用于用户提到 projectile ability、ProjectileAbility、projectile_ability.xml、projectile.xlsx、ProjectileFactory、发射物扩展能力、命中特效、击退、跟踪外的玩法扩展、IProjectileHitHandler、ProjectileHitContext、Luban 配置联动等场景。该技能负责把“固定骨架识别 → schema 修改 → Luban 生成 → 运行时 Ability 实现 → ProjectileFactory 接入 → 数据表补齐 → 构建验证”串成固定流程，避免只改配置或只改运行时代码。
---

# Projectile Ability 创建技能

用于在 `Gameplay.Combat.Projectile` 下新增或调整配置驱动的扩展 Ability。

## 适用范围

当任务符合以下任一条件时，优先使用本技能：
- 新增一个 `ProjectileXxxAbility`
- 修改 `lst_ability` 中的发射物扩展能力，例如击退、命中特效、持续附加效果
- 需要改 `Configs/GameConfig/Defines/projectile_ability.xml`
- 需要执行 Luban 生成并更新 `GameProto`
- 需要把新配置接入 `ProjectileFactory.CreateAbilityFromConfig`
- 需要调整 `IProjectileHitHandler` / `ProjectileHitContext` / `Projectile.OnTriggerEnter2D` 这类命中扩展链路

如果改动目标是 Projectile 固定骨架能力，而不是 `lst_ability` 扩展能力，先确认是否应该进入 `move_ability`、`damage_ability`、`lifetime`、`tracking` 这几个固定入口，不要误用本技能。

## 先建立心智模型

开始前先确认这几个事实：
- `Configs/GameConfig/Defines/projectile_ability.xml` 定义 Projectile 扩展 Ability 的 schema
- `Configs/GameConfig/Defines/projectile.xml` 定义 Projectile 固定骨架入口：`move_ability`、`damage_ability`、`lifetime`、`tracking`、`lst_ability`
- `Configs/GameConfig/Datas/projectile.xlsx` 中 `lst_ability#format=lite` 是实际数据入口
- `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/` 是 Luban 生成代码，禁止手写业务逻辑
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/` 放 Projectile 运行时扩展 Ability
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/ProjectileFactory.cs` 负责把配置映射到 Ability 实例
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Projectile.cs` 负责碰撞入口和命中事件分发
- 命中特效类扩展现在应优先实现 `IProjectileHitHandler`，并消费池化的 `ProjectileHitContext`

## 标准流程

### 0. 配置表修改权限规则

在碰到任何配置表相关操作前，先执行下面两条规则：
- 如果涉及修改配置表内容，例如 `projectile.xlsx` 的数据行、`lst_ability` 内容、旧配置补字段、默认值回填，必须先向用户确认，得到明确同意后才能动表
- 如果涉及修改配置表表头，例如新增列、改列名、调整 `##var / ##type / lite format` 头部结构，一律不允许代理直接操作，只能由用户自己修改

也就是说：
- `Defines/*.xml` 的 schema 修改可以由代理完成
- `Datas/*.xlsx` 的数据区修改需要先确认
- `Datas/*.xlsx` 的表头区修改禁止代理代做

### 1. 先判断属于哪类能力

先回答：
- 这是 `lst_ability` 扩展能力吗？
- 还是 Projectile 固定骨架能力？
- 它是“逐帧行为”还是“命中后效果”？
- 是否需要读 `RuntimeData.TargetDirection`、`Rigidbody.velocity`、`ProjectileHitContext` 或目标刚体？

判断规则：
- 改 `move_ability`、`damage_ability`、`lifetime`、`tracking`，属于固定骨架
- 改 `lst_ability`，属于扩展能力
- 命中后生效的扩展能力，优先走 `IProjectileHitHandler`
- 不要在每个命中特效 Ability 里重复解析 `Collider2D`、敌我阵营和重复命中保护，这些应优先复用 `Projectile` 的命中流水线

### 2. 先对照现有实现

优先阅读相近实现，至少查看：
- `ProjectileFactory.cs`
- `Projectile.cs`
- `Ability/Core/ProjectileDamageAbility.cs`
- `Ability/ProjectileKnockbackAbility.cs`
- `Ability/IProjectileHitHandler.cs`
- `Ability/ProjectileHitContext.cs`

如果用户描述的是“命中后做某件事”，优先复用 `IProjectileHitHandler + ProjectileHitContext` 模式，而不是从零发明新的碰撞入口。

### 3. 修改 XML schema

在 `Configs/GameConfig/Defines/projectile_ability.xml` 中：
- 新增 `ProjectileXxxConfig`
- 使用清晰稳定的 `alias`
- 字段顺序尽量贴近同类能力
- 首版能少字段就少字段，避免给旧数据制造兼容负担

命名建议：
- C# 类：`ProjectileXxxAbility`
- 配置类：`ProjectileXxxConfig`
- alias：简短、稳定、偏行为语义，例如 `knockback`

### 4. 检查并补齐数据表

修改 schema 后，立即检查：
- `Configs/GameConfig/Datas/projectile.xlsx` 中所有受影响的 `lst_ability`
- 旧数据是否缺字段
- 新增字段是否需要给已有行补默认值
- `lst_ability#format=lite` 对应的拼装列是否仍能正确生成最终配置串

但在真正修改 `projectile.xlsx` 之前，必须先再次确认用户是否允许修改数据区。
如果发现需要改的是表头而不是数据内容，立即停止并明确告知用户“这一步只能由用户自行操作”。

尤其是给现有 bean 增加新字段时，Luban 很容易因为旧 `lst_ability` 数据格式不匹配而失败。

### 5. 执行 Luban 生成

按项目固定方式执行：

```bat
Configs\GameConfig\gen_code_json_to_project_lazyload.bat
```

如果批处理不便直接观察输出，可改用等价 `dotnet Luban.dll ...` 命令执行，但要保持参数一致。

生成后检查：
- 目标配置类是否已生成
- `ProjectileAbilityConfig.DeserializeProjectileAbilityConfig` 是否包含新类型分支
- 若新增了字段，生成类里是否已出现对应字段

### 6. 实现运行时 Ability

在 `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/` 下新增或修改类：
- 逐帧行为：`ProjectileAbility + IAbilityUpdate / IAbilityFixedUpdate`
- 命中后效果：优先实现 `IProjectileHitHandler`

实现时遵循这些规则：
- 构造函数中读取 config 并设置 `Priority`
- 需要命中事件时，只消费 `ProjectileHitContext`，不要在 Ability 内部重新做碰撞解析
- 需要读目标刚体时，优先使用 `context.TargetRigidbody`
- `ProjectileHitContext` 是池化对象，只在当前命中处理链内使用，不要跨帧缓存
- 如果需要额外扩展命中上下文，优先沿用 `MemoryObject + MemoryPool` 模式

常见写法示例：

```csharp
public class ProjectileXxxAbility : ProjectileAbility, IProjectileHitHandler
{
    public ProjectileXxxAbility(ProjectileXxxConfig config)
    {
        Priority = config?.Priority ?? 0;
    }

    public void HandleHit(ProjectileHitContext context)
    {
        if (Owner == null || context == null)
            return;
    }
}
```

### 7. 接入 ProjectileFactory

更新 `DefaultProjectileAbilityCreatorForConfig`：
- 增加 `ProjectileXxxConfig => new ProjectileXxxAbility(...)`

如果忘了这一步，配置会加载成功但运行时无法创建 Ability。

### 8. 检查是否影响示例数据

如果任务要求能立即在现有 projectile 上生效，确认：
- `projectile.xlsx` 已给对应 Projectile 配上新 ability
- `lst_ability` 的 lite 格式与 schema 完全匹配

### 9. 做最小可用验证

文件改动后至少执行：

```bash
dotnet build "D:\UnityProject\Marbles Legion\UnityProject\GameLogic.csproj" -nologo
```

如果本次改动涉及 Luban，必须先确保 Luban 生成成功，再做 `dotnet build`。

如果 build 因 `.csproj` 没刷新而漏掉新文件，优先让 Unity 重新生成工程文件，再重跑构建验证。

汇报时区分：
- 新错误
- 既有 warning
- 与本次改动无关的问题

## 输出要求

完成后用简明中文汇报：
- 改了哪些文件
- 新能力或新配置做了什么
- 是否执行了 Luban
- 是否执行了构建验证
- 是否存在与本次改动无关的遗留 warning

## 失败排查清单

如果 Luban 失败，优先排查：
- `projectile.xlsx` 的 `lst_ability` 旧数据缺字段
- Excel 文件被占用，存在 `~$projectile.xlsx`
- alias 与 bean 名不匹配
- lite 格式拼装后的字符串不合法

如果运行时不生效，优先排查：
- `ProjectileFactory` 是否接入 creator
- ability 是否真正挂进了 `lst_ability`
- 命中效果类是否实现了 `IProjectileHitHandler`
- `Projectile.cs` 的命中分发链是否经过该 Ability
- `Priority` 是否正确
