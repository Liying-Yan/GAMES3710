# 收集-使用系统使用手册

本文档面向关卡设计人员，介绍如何在场景中配置和使用收集-使用系统。

---

## 目录

1. [前置准备](#前置准备)
2. [钥匙-门系统](#钥匙-门系统)
3. [Sanity-药片系统](#sanity-药片系统)
4. [后处理效果](#后处理效果)
5. [集成测试清单](#集成测试清单)
6. [迁移指南（旧系统 → 新系统）](#迁移指南旧系统--新系统)

---

## 前置准备

### 必需的场景组件

在开始配置关卡之前，确保场景中存在以下物体：

| 物体 | 组件 | 说明 |
|------|------|------|
| PlayerCapsule | `PlayerInventory` | 玩家道具管理 |
| PlayerCapsule | `SanityManager` | San值管理（关卡1、2需要） |
| PromptUI | `PromptUI` | 临时提示UI（底部） |
| InteractionPromptUI | `InteractionPromptUI` | 持续提示UI（交互提示） |
| SanityUI | `SanityUI` | San条和药片数量显示 |
| SanityPostProcess | `FullscreenEffect` + `SanityPostProcess` | 低San噪点效果 |

### UI层级结构参考

```
Canvas
├── PromptText          → 拖入 PromptUI.Prompt Text
├── InteractionText     → 拖入 InteractionPromptUI.Prompt Text
├── SanitySlider        → 拖入 SanityUI.Sanity Slider
├── PillCountText       → 拖入 SanityUI.Pill Count Text
└── ReadingUI           → ReadingUI component (全屏阅读面板，运行时自动创建)
```

---

## 钥匙-门系统

### 创建可拾取钥匙

1. 创建物体（Cube/自定义模型）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **ItemPickup** 组件
4. 配置参数：

| 参数 | 说明 |
|------|------|
| Display Name | 拾取时显示的名称（如 "Basement Key"） |
| Consumable | 是否一次性消耗（勾选=用后从背包移除，取消=可重复用于多扇门） |

**添加新钥匙**：直接在场景中创建新物体并挂载 `ItemPickup` 即可，无需修改任何枚举或脚本。每把钥匙都是独立的场景物体引用。

### 创建门/机关

1. 创建物体（门模型/机关模型）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **LockedDoor** 组件
4. 配置参数：

| 参数 | 说明 |
|------|------|
| Required Keys | 所需钥匙列表，从场景中拖入 `ItemPickup` 物体引用。留空表示不需要钥匙 |
| Required Mechanisms | 前置依赖的机关列表（保留不变） |
| Animation Type | 开门动画类型：None / SwingDoor / AnimatorTrigger |
| Locked Prompt | 缺钥匙时显示的提示文字（默认 "Requires a key"） |
| Is Open | 当前状态（运行时会自动更新） |

### 门动画配置

#### None 模式（默认）

门物体直接隐藏（`SetActive(false)`），兼容旧行为。

#### SwingDoor 模式

门绕Y轴旋转打开。需要特定的物体层级结构：

```
DoorRoot (挂载 LockedDoor + Collider)
├── DoorFrame (门框模型，不动)
└── Door (门板模型，会旋转)
```

额外参数：

| 参数 | 说明 |
|------|------|
| Door Transform | 拖入需要旋转的 `Door` 子物体 |
| Swing Angle | 旋转角度，默认 -90（负值=逆时针，正值=顺时针） |
| Swing Duration | 旋转时长（秒），默认 1 |

#### AnimatorTrigger 模式

通过 Animator 触发开门动画。

| 参数 | 说明 |
|------|------|
| Mechanism Animator | 拖入 Animator 组件引用 |
| Animator Trigger Name | Trigger 参数名，默认 "Activate" |

### 设置机关依赖

**场景示例**：玩家必须先开启电闸，才能操作水泵

1. 创建电闸物体，添加 `LockedDoor`，Required Keys 留空
2. 创建水泵物体，添加 `LockedDoor`
3. 将电闸物体拖入水泵的 **Required Mechanisms** 数组

**运行时行为**：
- 电闸未开时，交互水泵显示 "Requires another mechanism first"
- 电闸开启后，水泵可正常交互

---

## Sanity-药片系统

### SanityManager 参数配置

在 PlayerCapsule 的 `SanityManager` 组件中配置：

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Max Sanity | San值上限 | 100 |
| Current Sanity | 初始San值 | 100 |
| Decay Rate | 每秒衰减量 | 1-3 |
| Pill Restore Amount | 每颗药片恢复量 | 20-30 |
| Low Sanity Threshold | 触发后处理的阈值 | 30 |

### 创建可拾取药片

1. 创建物体（Capsule/自定义模型）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **PillPickup** 组件
4. 配置参数：

| 参数 | 说明 |
|------|------|
| Display Name | 拾取时显示的名称（如 "Pill"） |

### 玩家操作

| 按键 | 功能 |
|------|------|
| E | 拾取药片 |
| Q | 服用药片（恢复San值） |

---

## 后处理效果

### SanityPostProcess 参数配置

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Noise Effect | 引用 FullscreenEffect 组件 | 拖入同物体的组件 |
| Noise Material | 引用 NoiseEffectMaterial | 拖入材质 |
| Min Noise Intensity | 刚进入低San时的噪点强度 | 0 |
| Max Noise Intensity | San归零时的噪点强度 | 0.3-0.5 |

### NoiseEffectMaterial 参数

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Noise Intensity | 噪点强度（由脚本控制） | - |
| Noise Frame Rate | 噪点更新帧率 | 30 |

### FullscreenEffect 参数

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Material | NoiseEffectMaterial | 拖入材质 |
| Injection Point | 注入时机 | AfterRenderingPostProcessing |
| Input Requirements | 输入需求 | Color |

---

## 关卡适用性

| 系统 | 关卡1 | 关卡2 | 关卡3 |
|------|:-----:|:-----:|:-----:|
| 钥匙-门 | ✓ | ✓ | ✗ |
| Sanity-药片 | ✓ | ✓ | ✗ |
| 后处理噪点 | ✓ | ✓ | ✗ |

**关卡3**：禁用 SanityManager、SanityUI、SanityPostProcess 组件。

---

## 集成测试清单

### 场景准备

1. 放置2-3个钥匙（不同的 `ItemPickup` 物体）
2. 放置对应的门，将钥匙引用拖入 `Required Keys`
3. 放置3-4个药片
4. 临时将 San Decay Rate 调高（如 10）加快测试

### 钥匙-门系统测试

- [ ] 靠近钥匙，显示 "Press E to pick up"
- [ ] 按E拾取，底部显示 "Obtained [名称]"，钥匙物体隐藏（不销毁）
- [ ] 靠近门，显示 "Press E to interact"
- [ ] 缺钥匙时按E，底部显示 lockedPrompt 文字
- [ ] 持有全部所需钥匙时按E，门打开
- [ ] 一次性钥匙（consumable=true）消耗后，需要该钥匙的其他门无法打开
- [ ] 非一次性钥匙（consumable=false）不被消耗，可重复用于多扇门

### 门动画测试

- [ ] None 模式：门物体隐藏
- [ ] SwingDoor 模式：Door 子物体平滑旋转到目标角度
- [ ] AnimatorTrigger 模式：Animator 被正确触发

### 机关依赖测试

- [ ] 创建门A（无需钥匙）和门B（依赖门A）
- [ ] 门A未开时，交互门B显示 "Requires another mechanism first"
- [ ] 门A开启后，门B可正常交互

### Sanity-药片系统测试

- [ ] 运行游戏，观察San条随时间下降
- [ ] 靠近药片按E拾取，药片数量+1
- [ ] 按Q服用，San值恢复，药片数量-1
- [ ] 无药片时按Q，无反应
- [ ] San高于阈值时，无噪点效果
- [ ] San低于阈值时，噪点效果出现
- [ ] San越低，噪点越强

### UI提示测试

- [ ] 持续提示（交互提示）在离开范围后消失
- [ ] 临时提示（拾取反馈）几秒后自动消失
- [ ] 两种提示不会互相覆盖

---

## 常见问题

### Q: 拾取物品没有反应
- 检查物体是否有 Collider 且勾选 Is Trigger
- 检查玩家是否有 "Player" Tag
- 检查场景中是否有 PlayerInventory 和 PromptUI 单例

### Q: 门无法打开
- 检查 Required Keys 中是否正确拖入了对应的 `ItemPickup` 物体引用
- 检查 Required Mechanisms 中的前置机关是否已开启
- 检查钥匙是否设为 consumable 且已被其他门消耗

### Q: 噪点效果不显示
- 检查 SanityPostProcess 引用是否完整
- 检查 FullscreenEffect 的 Input Requirements 是否为 Color
- 检查 San 值是否低于 Low Sanity Threshold

### Q: SwingDoor 旋转方向不对
- 调整 Swing Angle 的正负值（负值=逆时针，正值=顺时针）
- 确保 Door Transform 拖入的是门板子物体，而非门框或根物体

### Q: 持续提示不显示
- 检查场景中是否有 InteractionPromptUI 单例
- 检查 Prompt Text 引用是否正确

---

## 迁移指南（旧系统 → 新系统）

> 本节面向从旧版 `ItemType` 枚举系统迁移的项目。新项目可跳过。

### 核心变更

旧系统使用 `ItemType` 枚举实现一对一的钥匙-门绑定；新系统改为直接引用 `ItemPickup` 场景物体，支持一门多钥匙、钥匙复用和多种开门动画。

**`ItemType.cs` 已被删除**，所有基于枚举的道具类型匹配不再存在。

### 字段映射表

#### ItemPickup

| 旧字段 | 新字段 | 说明 |
|--------|--------|------|
| `itemType` (ItemType) | *(已移除)* | 不再需要枚举类型 |
| `displayName` (string) | `displayName` (string) | 保持不变 |
| *(无)* | `consumable` (bool) | 新增，控制钥匙是否一次性消耗，默认 true |

#### LockedDoor

| 旧字段 | 新字段 | 说明 |
|--------|--------|------|
| `requiredItem` (ItemType) | `requiredKeys` (ItemPickup[]) | 从枚举改为场景物体引用数组 |
| `requiredItemName` (string) | `lockedPrompt` (string) | 改为完整提示文字，默认 "Requires a key" |
| `consumeItem` (bool) | *(已移除)* | 消耗控制转移到钥匙自身的 `consumable` 字段 |
| `requiredMechanisms` | `requiredMechanisms` | 保持不变 |
| `isOpen` | `isOpen` | 保持不变 |
| *(无)* | `animationType` | 新增，开门动画类型 |
| *(无)* | `swingAngle`, `swingDuration`, `doorTransform` | 新增，SwingDoor 模式参数 |
| *(无)* | `mechanismAnimator`, `animatorTriggerName` | 新增，AnimatorTrigger 模式参数 |

#### PlayerInventory

| 旧接口 | 新接口 |
|--------|--------|
| `AddItem(ItemType)` | `AddKey(ItemPickup)` |
| `HasItem(ItemType)` | `HasKey(ItemPickup)` |
| `RemoveItem(ItemType)` | `RemoveKey(ItemPickup)` |
| `Clear()` | `Clear()` |

### 场景迁移步骤

#### 1. 迁移 ItemPickup 组件

对于场景中每个 `ItemPickup` 物体：
1. Inspector 中 `itemType` 字段会显示为 Missing，可忽略
2. 配置新的 `consumable` 字段（旧系统中 `consumeItem` 在门上配置，现在移到钥匙上）
3. `displayName` 无需改动

#### 2. 迁移 LockedDoor 组件

对于场景中每个 `LockedDoor` 物体：
1. 旧的 `requiredItem` 和 `requiredItemName` 字段会显示为 Missing，可忽略
2. 将对应的 `ItemPickup` 场景物体拖入 **Required Keys** 数组
3. 配置 `lockedPrompt` 提示文字
4. 如需开门动画，配置 `animationType` 及对应参数
5. `requiredMechanisms` 无需改动
