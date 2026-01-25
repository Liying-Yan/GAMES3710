# 敌人AI设置指南

## 前置条件
- Unity 6.3 + AI Navigation 包

## 设置步骤

### 1. 烘焙 NavMesh
1. 选择可行走的地面物体
2. 添加组件 > **NavMesh Surface**
3. 点击 **Bake**

### 2. 创建巡逻路径
1. 创建空物体，添加 **PatrolPath** 组件
2. 添加子物体作为路径点（位置有效，名称无所谓）
3. 可选：在 `Next Paths` 列表中指定其他 PatrolPath，形成随机路线网络

### 3. 创建敌人
1. 创建 Capsule（+ 小球放在本地坐标 (0, 0, 0.5) 标识正面）
2. 添加 **NavMeshAgent** 组件
3. 添加 **EnemyAI** 组件
4. 将 PatrolPath 指定给 `Initial Path`
5. 保存为 Prefab

## Inspector 参数说明

### PatrolPath
| 参数 | 说明 |
|------|------|
| Next Paths | 到达终点后可选的下一个路径片段 |

### EnemyAI
| 参数 | 说明 |
|------|------|
| Initial Path | 初始巡逻路径 |
| Waypoint Reach Threshold | 判定到达路径点的距离阈值 |
| Patrol Speed | 巡逻状态移动速度 |
| Chase Speed | 追逐状态移动速度 |
| Search Speed | 搜索状态移动速度 |
