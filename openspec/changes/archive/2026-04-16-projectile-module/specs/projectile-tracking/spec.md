# Projectile Tracking 发射物追踪行为

## Purpose

定义发射物的三种追踪模式及其行为规范。

## ADDED Requirements

### Requirement: 追踪模式枚举

系统 SHALL 提供 `EnumProjectileTrackingMode` 枚举，定义三种追踪模式。

#### Scenario: 枚举值定义
- **WHEN** 引用 EnumProjectileTrackingMode
- **THEN** 可用值包括：
  - None = 0: 直线飞行
  - Target = 1: 追踪目标 Marble
  - Point = 2: 追踪固定坐标点

### Requirement: 直线飞行模式

系统 SHALL 在追踪模式为 None 时，保持初始方向飞行。

#### Scenario: 直线飞行
- **WHEN** TrackingMode 为 None
- **THEN** 每帧保持初始方向
- **AND** 不进行任何方向调整

### Requirement: 追踪目标模式

系统 SHALL 在追踪模式为 Target 时，持续调整方向朝向目标 Marble。

#### Scenario: 追踪有效目标
- **WHEN** TrackingMode 为 Target
- **AND** 目标 Marble 存在且存活
- **THEN** 每帧计算指向目标的方向向量
- **AND** 根据 TrackingRate 限制转向速度
- **AND** 旋转发射物朝向

#### Scenario: 追踪目标丢失
- **WHEN** TrackingMode 为 Target
- **AND** 目标 Marble 不存在或已死亡
- **THEN** 保持当前方向继续飞行
- **AND** 不再追踪

### Requirement: 追踪点模式

系统 SHALL 在追踪模式为 Point 时，持续调整方向朝向固定坐标点。

#### Scenario: 追踪固定点
- **WHEN** TrackingMode 为 Point
- **THEN** 每帧计算指向 TargetPoint 的方向向量
- **AND** 根据 TrackingRate 限制转向速度
- **AND** 旋转发射物朝向

### Requirement: 转向速率限制

系统 SHALL 通过 TrackingRate 控制追踪转向速度。

#### Scenario: 限制转向角度
- **WHEN** 需要转向角度为 θ
- **AND** 最大允许转向为 TrackingRate * deltaTime
- **THEN** 实际转向角度为 clamp(θ, -maxTurn, maxTurn)
- **AND** 防止发射物急转
