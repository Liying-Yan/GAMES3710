# Hiding Spots Tutorial

本文档面向关卡设计人员，介绍如何配置和使用两种躲藏点：**柜子（Cabinet）** 和 **桌子（Desk）**。

---

## Overview

躲藏点允许玩家按 **E** 进入隐藏状态，对敌人完全不可见。两种躲藏点有不同的进入/退出动画：

| 类型 | 进入动画 | 退出动画 |
|------|----------|----------|
| Cabinet | 开门 → 移入 → 关门 | 开门 → 移出 → 关门 |
| Desk | 下蹲 → 滑入 → 转身 | 转身 → 滑出 → 站起 |

两种躲藏点都支持：
- 躲藏中有限视角（可配置范围）
- 自动调整相机 Near Clip Plane（防止近距离面片裁剪）
- 可选的敌人检查机制（敌人在视线中看到玩家躲入时可前来抓人）

---

## Cabinet Hiding Spot

### Hierarchy Structure

```
Cabinet (Parent)                ← CabinetHidingSpot + BoxCollider (Is Trigger ✓)
├── Body                        ← 柜体（玩家躲藏位置参考）
├── Door                        ← 柜门（pivot 必须在铰链位置）
└── ExitPoint (Empty GameObject) ← 退出后玩家出现的位置
```

### Setup Steps

1. **准备模型**：确保柜门的 pivot 在铰链处（门轴旋转点），柜门和柜体分别是独立的子物体

2. **挂载脚本**：在 Cabinet 父物体上添加 `CabinetHidingSpot` 组件

3. **添加 Trigger**：在 Cabinet 父物体上添加 `BoxCollider`，勾选 **Is Trigger**，调整大小覆盖柜子前方玩家可交互的区域

4. **创建 ExitPoint**：在 Cabinet 下创建一个空子物体 `ExitPoint`，放置在柜子正前方地面位置，**旋转朝向**设为玩家退出后应面对的方向

5. **拖入引用**：
   - `Cabinet Body` → Body 子物体
   - `Cabinet Door` → Door 子物体
   - `Exit Point` → ExitPoint 子物体

### Inspector Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| **Door Settings** | | |
| Door Open Angle | 门打开角度，正值 = 顺时针 | 90 |
| Door Duration | 开/关门动画时长（秒） | 0.5 |
| **Camera Transition** | | |
| Camera Duration | 玩家移入/移出动画时长（秒） | 0.5 |
| Hide Position Offset | 相对柜体 pivot 的偏移（本地空间），用于微调躲藏位置。XZ 控制水平偏移，Y 控制高度 | (0, 0, 0) |
| **Camera** | | |
| Hiding Near Clip | 躲藏时相机的 Near Clip Plane（越小越不容易裁剪近处面片） | 0.01 |
| **Hiding Look Clamp** | | |
| Max Yaw | 躲藏中左右视角范围（度） | 45 |
| Max Pitch | 躲藏中上下视角范围（度） | 30 |
| **Prompt Settings** | | |
| Enter Prompt | 进入提示文本 | "Press E to hide" |
| Exit Prompt | 退出提示文本 | "Press E to exit" |
| **Breath Audio** | | |
| Breath Audio | 躲藏时播放的呼吸声 AudioSource | None |
| **Audio Mixer** | | |
| Mixer | 用于静音环境道具声音的 AudioMixer | None |
| **SFX** | | |
| Door Audio Source | 门音效的 AudioSource | None |
| Door Open Clip | 开门音效 | None |
| Door Close Clip | 关门音效 | None |
| **Enemy Check** | | |
| Can Be Checked | 是否允许敌人打开此柜子抓人（需配合敌人端开关） | false |

### Notes

- `Hide Position Offset` 可以在 Play Mode 中**实时调整**，方便微调位置
- ExitPoint 的 Y 值不重要，系统会自动使用玩家进入前的地面高度
- Door 的旋转轴是本地 Y 轴，确保模型的 pivot 设置正确

---

## Desk Hiding Spot

### Hierarchy Structure

```
Desk (Parent)                    ← DeskHidingSpot + BoxCollider (Is Trigger ✓)
├── HidePoint (Empty GameObject)  ← 位置 = 相机目标位置，Z 朝向 = 视线方向
└── ExitPoint (Empty GameObject)  ← 可选，退出后玩家位置（不设则回到进入前位置）
```

### Setup Steps

1. **挂载脚本**：在 Desk 物体上添加 `DeskHidingSpot` 组件

2. **添加 Trigger**：在 Desk 上添加 `BoxCollider`，勾选 **Is Trigger**，调整大小覆盖桌子前方交互区域

3. **创建 HidePoint**：在 Desk 下创建空子物体 `HidePoint`
   - **位置**：放在桌子下方玩家**眼睛**应该在的位置（不是脚的位置，系统会自动补偿相机高度）
   - **旋转**：Z 轴（蓝色箭头）指向玩家躲藏后应该面对的方向

4. **创建 ExitPoint**（可选）：在 Desk 下创建空子物体 `ExitPoint`，放在桌子前方。如果不创建，玩家会回到进入前的位置

5. **拖入引用**：
   - `Hide Point` → HidePoint 子物体
   - `Exit Point` → ExitPoint 子物体（可选）

### Inspector Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| **Animation Durations** | | |
| Crouch Duration | 下蹲/站起动画时长（秒） | 0.4 |
| Move Duration | 水平滑入/滑出动画时长（秒） | 0.5 |
| Rotate Duration | 转身动画时长（秒） | 0.3 |
| **Hiding Position** | | |
| Hide Position Offset | 相对 HidePoint 的偏移（本地空间），运行时可实时调整 | (0, 0, 0) |
| **Camera** | | |
| Hiding Near Clip | 躲藏时相机 Near Clip Plane | 0.01 |
| **Hiding Look Clamp** | | |
| Max Yaw | 躲藏中左右视角范围（度） | 45 |
| Max Pitch | 躲藏中上下视角范围（度） | 30 |
| **Prompt Settings** | | |
| Enter Prompt | 进入提示文本 | "Press E to hide" |
| Exit Prompt | 退出提示文本 | "Press E to exit" |
| **Breath Audio / Audio Mixer** | 同 Cabinet | — |
| **Enemy Check** | | |
| Can Be Checked | 是否允许敌人来桌前抓人 | false |

### Notes

- HidePoint 的位置代表**相机位置**（眼睛高度），不是玩家脚底位置
- 如果不设置 ExitPoint，玩家会退回到进入前站立的位置
- 进入动画顺序是 下蹲 → 滑入 → 转身，所以 HidePoint 高度应低于玩家站立时的眼睛高度

---

## Enemy Check System

当敌人**正在追逐**玩家时看到玩家躲入躲藏点，如果两个开关都激活，敌人会走到躲藏点并触发 Game Over。

### Two Switches

| Switch | Location | Default | Description |
|--------|----------|---------|-------------|
| `Can Check Hiding Spots` | EnemyAI 组件 | false | 敌人端总开关 |
| `Can Be Checked` | 每个躲藏点组件 | false | 躲藏点端开关 |

**两个开关都为 true** 时才会触发检查行为，任一为 false 则和之前一样（敌人进入搜索模式）。

### Behavior

| 躲藏点类型 | 敌人行为 |
|------------|----------|
| Cabinet | 敌人走到 ExitPoint 位置 → 打开柜门 → Game Over |
| Desk | 敌人走到 ExitPoint（或桌子位置） → Game Over |

### Setup

1. 在 EnemyAI 组件中勾选 `Can Check Hiding Spots`
2. 在需要被检查的躲藏点上勾选 `Can Be Checked`
3. 确保躲藏点的 ExitPoint 或 Transform 位置在 NavMesh 可达范围内

> **Design Tip**: 不是所有躲藏点都需要开启检查。可以设计一些"安全"躲藏点（检查关闭）和一些"危险"躲藏点（检查开启），增加关卡策略性。

---

## Troubleshooting

### Q: 玩家按 E 没有反应

检查：
1. 柜子/桌子上是否有 `BoxCollider` 且勾选了 **Is Trigger**
2. 玩家是否有 `Player` Tag
3. Trigger 区域是否覆盖了玩家站立的位置

### Q: 玩家进入后位置不对

检查：
1. Cabinet: 调整 `Hide Position Offset`（Play Mode 中可实时调整）
2. Desk: 确认 HidePoint 放在**相机应在的位置**（眼睛高度，不是地面）
3. 确认子物体的 pivot / 旋转是否正确

### Q: 柜门开合方向不对

调整 `Door Open Angle` 的正负值：正值 = 顺时针，负值 = 逆时针。确保柜门的 pivot 在铰链位置。

### Q: 近处面片被裁掉 / 看到模型内部

减小 `Hiding Near Clip` 值（默认 0.01，可以尝试更小的值如 0.005）。

### Q: 躲藏后退出时穿过地面

ExitPoint 的 Y 高度不影响结果（系统使用进入前的地面高度）。如果仍有问题，检查进入前玩家脚下是否有地面。

### Q: 敌人看到我躲入但没有来检查

检查两个开关是否都已激活：
1. EnemyAI 上的 `Can Check Hiding Spots` = true
2. 对应躲藏点上的 `Can Be Checked` = true
3. 敌人必须在 **Chase 状态**时看到玩家躲入才会触发（Patrol/Search 状态不会）
