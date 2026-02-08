# 收集-使用系统设计文档

## 概述

本文档记录 Sanity-药片系统 和 钥匙-门系统 的设计决策与技术选型。

---

## 系统概览

### Sanity-药片系统

| 功能 | 状态 | 说明 |
|------|:----:|------|
| San条UI显示 | 实现 | 屏幕显示San值进度条 |
| 时间自然下降 | 实现 | San值随时间衰减 |
| 按E拾取药片 | 实现 | 触发器范围内按E拾取 |
| 药片数量UI | 实现 | 显示持有药片数量 |
| 按Q服用恢复 | 实现 | 消耗药片恢复San值 |
| 低San噪点效果 | 实现 | 后处理噪点滤镜 |
| San归零处理 | 预留 | 游戏结束/重试画面（暂不实现） |
| 看怪加速下降 | 预留 | 接口预留，暂不实现 |
| 频繁服药副作用 | 预留 | 视觉扭曲（暂不实现） |
| 服药生成怪物 | 预留 | 暂不实现 |

### 钥匙-门系统

| 功能 | 说明 |
|------|------|
| 拾取方式 | 触发器范围内按E拾取 |
| 拾取提示 | 屏幕底部显示"获得XX" |
| 钥匙使用 | 一把钥匙对应一扇门，消耗使用 |
| 无钥匙交互 | 显示"需要XX" |
| 机关类型 | 门/水泵/电闸等统一抽象 |
| 机关依赖 | 某些门依赖其他机关先被激活 |
| 持有物UI | 不显示，仅内部状态 |

---

## 技术选型

| 方面 | 选择 | 说明 |
|------|------|------|
| 交互检测 | 触发器范围 | 范围内按E交互，场景保证不重叠 |
| 道具ID | 枚举 | `ItemType` 枚举，类型安全 |
| 机关依赖 | 直接引用 | Inspector拖拽，无跨场景需求 |
| 提示UI | 单例管理器 | 统一底部位置显示 |
| 后处理 | FullscreenEffect | 复制自 post-processing-shader 项目 |

---

## 脚本设计

### 文件结构

```
Assets/Scripts/
├── Core/
│   └── ItemType.cs              # 道具枚举
├── Player/
│   ├── PlayerInventory.cs       # 道具持有管理（单例）
│   └── SanityManager.cs         # San值管理（单例）
├── Interaction/
│   ├── ItemPickup.cs            # 可拾取物品
│   ├── PillPickup.cs            # 药片拾取
│   ├── Interactable.cs          # 可交互物基类
│   └── LockedDoor.cs            # 门/机关
├── UI/
│   ├── PromptUI.cs              # 底部提示（单例）
│   └── SanityUI.cs              # San条+药片数量
└── PostProcess/
    ├── FullscreenEffect.cs      # 后处理框架（复制）
    └── Shaders/
        └── NoiseEffect.shader   # 噪点Shader
```

### 脚本职责

| 脚本 | 职责 |
|------|------|
| `ItemType` | 定义所有道具类型枚举 |
| `PlayerInventory` | 单例，记录持有道具集合，提供 Add/Has/Remove 接口 |
| `SanityManager` | 单例，管理San值、衰减速率、恢复、药片数量、服用冷却 |
| `ItemPickup` | 触发器检测 + 按E拾取，配置道具类型，调用 PromptUI 显示提示 |
| `PillPickup` | 药片拾取，增加 SanityManager 药片计数 |
| `Interactable` | 基类，定义交互接口 |
| `LockedDoor` | 配置所需道具、前置机关依赖，交互时检查条件 |
| `PromptUI` | 单例，Show(string) 显示临时提示，底部位置 |
| `SanityUI` | 绑定 SanityManager，更新San条和药片数量显示 |
| `FullscreenEffect` | 后处理框架，挂载GameObject控制开关 |
| `NoiseEffect.shader` | 噪点效果Shader |

---

## 交互流程

### 拾取物品
```
玩家进入触发器 → 按E → ItemPickup.Interact()
  → PlayerInventory.AddItem(type)
  → PromptUI.Show("获得" + displayName)
  → Destroy(gameObject)
```

### 拾取药片
```
玩家进入触发器 → 按E → PillPickup.Interact()
  → SanityManager.AddPill()
  → PromptUI.Show("获得药片")
  → Destroy(gameObject)
```

### 服用药片
```
按Q → SanityManager.UsePill()
  → 检查药片数量 > 0
  → San值恢复
  → 药片数量 - 1
```

### 开门/机关
```
玩家进入触发器 → 按E → LockedDoor.Interact()
  → 检查前置机关依赖
  → 检查所需道具
  → 成功: 开门/激活机关, PlayerInventory.RemoveItem()
  → 失败: PromptUI.Show("需要XX" / "需要先激活XX")
```

---

## 预留接口

### SanityManager 扩展接口

```csharp
// 加速衰减（看到怪物时调用）
public void SetDecayMultiplier(float multiplier);

// 服药副作用检查（频繁服药）
public event Action OnOverdose;

// 归零回调（游戏结束）
public event Action OnSanityDepleted;
```

---

## 关卡适用

| 系统 | 关卡1 | 关卡2 | 关卡3 |
|------|:-----:|:-----:|:-----:|
| Sanity-药片 | ✓ | ✓ | ✗ |
| 钥匙-门 | ✓ | ✓ | ✗ |

---

## 输入绑定

| 按键 | Action | 用途 |
|------|--------|------|
| E | Interact | 拾取/交互 |
| Q | (新增) | 服用药片 |

需在 `InputSystem_Actions.inputactions` 中添加 Q 键绑定。

---

## 实现计划

### 阶段1：基础框架

**代码实现：**
1. `ItemType.cs` - 道具枚举
2. `PromptUI.cs` - 提示UI单例
3. `PlayerInventory.cs` - 道具管理单例

**Unity设置：**
1. 创建 Canvas（如已有可复用）
2. 创建 Text (TMP)，放置于底部偏上位置
3. 创建空物体 `PromptUI`，挂载 `PromptUI.cs`，拖入Text引用
4. 在玩家物体上挂载 `PlayerInventory.cs`

**测试验证：**
- 运行游戏，在代码中调用 `PromptUI.Instance.Show("测试提示")`
- 确认底部显示提示文字，几秒后消失

---

### 阶段2：钥匙-门系统

**代码实现：**
4. `ItemPickup.cs` - 物品拾取
5. `Interactable.cs` - 交互基类
6. `LockedDoor.cs` - 门/机关

**Unity设置：**
1. 创建测试钥匙：Cube + Collider (Is Trigger) + `ItemPickup`，设置 ItemType
2. 创建测试门：Cube + Collider (Is Trigger) + `LockedDoor`，设置所需 ItemType
3. 创建第二扇门，设置前置依赖为第一扇门

**测试验证：**
- [ ] 靠近钥匙，按E拾取，显示"获得XX"
- [ ] 无钥匙时靠近门按E，显示"需要XX"
- [ ] 有钥匙时靠近门按E，门打开（可用SetActive模拟）
- [ ] 第二扇门在第一扇门未开时显示"需要先激活XX"
- [ ] 第一扇门打开后，第二扇门可正常交互

---

### 阶段3：Sanity系统

**代码实现：**
7. `SanityManager.cs` - San值管理单例
8. `PillPickup.cs` - 药片拾取
9. `SanityUI.cs` - San条+药片数量UI
10. 添加Q键绑定到 InputSystem_Actions.inputactions

**Unity设置：**
1. 在玩家物体上挂载 `SanityManager.cs`
2. 创建 UI：
   - Slider 作为 San条
   - Text (TMP) 显示药片数量
3. 创建空物体挂载 `SanityUI.cs`，拖入引用
4. 创建测试药片：Capsule + Collider (Is Trigger) + `PillPickup`

**测试验证：**
- [ ] 运行游戏，观察San条随时间下降
- [ ] 靠近药片按E拾取，药片数量+1，显示提示
- [ ] 按Q服用，San值恢复，药片数量-1
- [ ] 无药片时按Q，无反应

---

### 阶段4：后处理效果

**代码实现：**
11. 复制 `FullscreenEffect.cs` 到项目
12. 创建 `NoiseEffect.shader`
13. 在 `SanityManager` 中控制后处理

**Unity设置：**
1. 创建 NoiseEffect Material，使用 NoiseEffect Shader
2. 创建空物体 `SanityPostProcess`，挂载 `FullscreenEffect`
3. 设置 Material 引用
4. 在 `SanityManager` 中引用此物体，根据San值控制启用/参数

**测试验证：**
- [ ] 手动启用 FullscreenEffect 组件，确认噪点效果显示
- [ ] San值高时无噪点
- [ ] San值降到阈值以下，噪点效果出现
- [ ] San值越低，噪点越强

---

### 阶段5：集成测试

**场景设置：**
1. 放置多个钥匙、门、药片
2. 设置门之间的依赖关系
3. 调整San衰减速率和药片恢复量

**完整流程测试：**
- [ ] 从头开始游玩，拾取钥匙开门
- [ ] San值下降，拾取并服用药片恢复
- [ ] 低San时噪点效果正常
- [ ] 机关依赖逻辑正确
