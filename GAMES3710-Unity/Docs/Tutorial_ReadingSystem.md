# Reading System Tutorial

本文档面向关卡设计人员，介绍如何在场景中配置和使用阅读交互系统。

---

## Overview

玩家靠近可阅读物体后显示 "Press E to read" 提示，按 **E** 打开全屏阅读界面，按 **ESC** 退出。阅读期间游戏暂停、玩家输入禁用、光标解锁。

---

## Setup

### Prerequisites

确保场景中已有以下物体（收集-使用系统的标准配置）：

| Object | Component | Description |
|--------|-----------|-------------|
| Canvas | `InteractionPromptUI` | "Press E to read" interaction prompt |

### Add ReadingUI to Scene

1. 在 Canvas 下创建空 GameObject，命名为 `ReadingUI`
2. 添加 `ReadingUI` 组件

UI 面板在运行时程序化创建，无需手动搭建。

```
Canvas
├── ...（existing UI elements）
└── ReadingUI          → ReadingUI component (auto-creates fullscreen panel)
```

### Create a Readable Object

1. 创建 3D 物体（Cube / Plane / custom model）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **Readable** 组件
4. 在 Inspector 中填写内容：

| Parameter | Description | Default |
|-----------|-------------|---------|
| Content | 阅读界面显示的文字内容（支持多行） | *(empty)* |
| Prompt Message | 靠近时的交互提示文字 | "Press E to read" |

---

## Runtime Behavior

| State | Behavior |
|-------|----------|
| 靠近物体 | 显示 Prompt Message 交互提示 |
| 按 E | 隐藏交互提示，打开全屏阅读面板，暂停游戏，解锁光标，禁用视角操作 |
| 按 ESC | 关闭阅读面板，恢复游戏，锁定光标，恢复视角操作 |
| 离开触发区域 | 交互提示消失 |

---

## Level Design Notes

- 物体的 Collider 大小决定了交互触发范围，可适当放大
- Content 字段支持多行文本，Inspector 中可直接换行编辑
- 同一场景可放置多个 Readable 物体，各自独立
- 阅读期间 `Time.timeScale = 0`，所有基于 deltaTime 的逻辑暂停

---

## Testing Checklist

- [ ] 靠近物体时显示 "Press E to read" 提示
- [ ] 离开范围后提示消失
- [ ] 按 E 后交互提示消失，全屏阅读面板显示
- [ ] 阅读面板正确显示 Content 中填写的文字
- [ ] 底部显示 "Press ESC to exit" 灰色提示
- [ ] 阅读期间玩家无法移动和转动视角
- [ ] 阅读期间光标可见且可自由移动
- [ ] 按 ESC 关闭面板，游戏恢复正常
- [ ] 关闭后光标重新锁定，视角操作恢复

---

## Troubleshooting

### Q: 按 E 没有反应
- 检查物体是否有 Collider 且勾选 Is Trigger
- 检查玩家是否有 "Player" Tag
- 检查场景 Canvas 下是否有 `ReadingUI` 组件

### Q: 阅读面板没有铺满全屏
- 确认 `ReadingUI` 挂载在 Canvas 的子物体上（而非 Canvas 外的物体）

### Q: 关闭阅读后视角仍然无法操作
- 检查场景中玩家是否有 `StarterAssetsInputs` 组件
