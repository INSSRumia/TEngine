## ADDED Requirements

### Requirement: The project SHALL provide an AI-first reading guide
项目 MUST 提供一个面向 AI 和开发者的阅读入口文档，明确说明项目结构、核心模块、推荐阅读顺序和关键约定，避免后续接手者只能从源码碎片中自行猜测系统结构。

#### Scenario: AI starts with a reading guide
- **WHEN** 一个新的 AI agent 首次进入项目
- **THEN** 它必须能够先读取到项目级阅读入口文档
- **AND** 文档必须告诉它先看哪些目录、哪些模块和哪些关键概念

#### Scenario: Reading guide identifies core subsystems
- **WHEN** AI 或开发者阅读该入口文档
- **THEN** 文档必须明确标出 Combat、RuntimeData、Factory、Luban 配置等核心子系统
- **AND** 说明它们各自的职责定位
