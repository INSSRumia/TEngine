## 1. 配置与生成准备

- [x] 1.1 在 `Configs/GameConfig/Defines/projectile_ability.xml` 中新增 `ProjectileKnockbackConfig`，仅包含 `force` 字段并声明稳定 alias
- [x] 1.2 检查 `Configs/GameConfig/Datas/projectile.xlsx` 中计划使用击退能力的 `lst_ability` 数据格式，确认是否只需补数据区而无需调整表头
- [x] 1.3 执行 Luban 生成并确认新的 `ProjectileKnockbackConfig` 与反序列化分支已正确生成

## 2. Projectile 命中扩展链路

- [x] 2.1 新增统一的 Projectile 命中上下文类型，封装有效命中后需要复用的目标与刚体信息
- [x] 2.2 新增 Projectile 命中处理接口，使可选扩展 Ability 可以消费统一命中事件
- [x] 2.3 调整 `ProjectileDamageAbility` 的命中处理流程，在复用现有有效命中校验的前提下分发命中上下文

## 3. 击退能力接入

- [x] 3.1 新增 `ProjectileKnockbackAbility`，按 `force` 配置对有效命中目标施加一次 `ForceMode2D.Impulse`
- [x] 3.2 在击退能力中实现“优先使用发射物当前速度方向、速度不足时回退到命中位置方向”的方向解析
- [x] 3.3 更新 `ProjectileFactory` 的配置 creator 映射，使 `ProjectileKnockbackConfig` 能被实例化并按优先级挂载

## 4. 数据补齐与验证

- [x] 4.1 在得到用户同意后，为目标发射物的 `projectile.xlsx` 数据区补入 `knockback` ability 配置
- [x] 4.2 执行 `dotnet build "D:\\UnityProject\\Marbles Legion\\UnityProject\\GameLogic.csproj" -nologo` 验证 HotFix 战斗代码编译通过
- [x] 4.3 记录 Luban 生成结果、构建结果以及与本次变更无关的残留问题，作为实施收尾说明
