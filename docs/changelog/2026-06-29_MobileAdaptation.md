# 手游适配 — 移动端输入层

> 2026-06-29 | 新建 4 个文件，修改 2 个文件，零破坏性变更

## 背景

为 FinalRPG 添加移动端屏幕控件（虚拟摇杆、攻击/闪避/背包/任务/对话按钮），同时保留 PC 键盘鼠标完整可用。

## 实现方案

### 架构设计

```
摇杆 (UIToolkitJoystick) ──→ InputDirection ──┐
攻击按钮 (PointerDown) ──→ flag ──────────────┤
闪避按钮 (PointerDown) ──→ flag ──────────────┤  MobileInputBridge.WriteIntent(ctx)
背包按钮 (ClickEvent) ──→ EventBus ───────────┤  (实现 IIntentProvider)
任务按钮 (ClickEvent) ──→ EventBus ───────────┤
键盘/鼠标 ──→ PlayerInputProvider ────────────┘  (回落层)
```

- `MobileInputBridge` 实现 `IIntentProvider`，通过已有的 `PlayerStateDriver.intentProviderOverride` 机制注入
- 内部包装 `PlayerInputProvider` 作为键盘回落，移动端仅在检测到输入时覆盖对应字段
- 摇杆使用纯 UI Toolkit（自研 `UIToolkitJoystick`），不依赖 PinePie Canvas/预制件
- 对话入口通过 EventBus (`OnNPCInteractAvailable/Unavailable`) 与 `NPCInteractable` 解耦

### 新建文件

| 文件 | 说明 |
|---|---|
| `Assets/UIToolKit/Mobile/MobileHUD.uxml` | 6 个屏幕控件布局 |
| `Assets/UIToolKit/Mobile/MobileHUD.uss` | 控件样式（红攻击/蓝闪避/金菜单/摇杆精灵） |
| `Assets/Scripts/UI/MobileInputBridge.cs` | 核心桥接，IIntentProvider 实现 |
| `Assets/Scripts/UI/UIToolkitJoystick.cs` | 纯 UI Toolkit 摇杆，Pointer 事件驱动 |

### 修改文件

| 文件 | 改动 |
|---|---|
| `Assets/Scripts/QuestSystem/EventBus.cs` | 新增 `OnNPCInteractAvailable(string)` / `OnNPCInteractUnavailable` 事件 + Raise 方法 |
| `Assets/Scripts/QuestSystem/Interaction/NPCInteractable.cs` | 新增 `npcDisplayName` 字段，范围进入/离开时发 EventBus，订阅 `TriggerNPCInteract` |
| `Assets/Scripts/Unity-HSM/PlayerStateDriver.cs` | `InitializeIntentProvider()` 从 `Awake()` 移至 `Start()`（修复执行顺序） |

### 不改动

`PlayerInputProvider.cs`、`InventoryUIController.cs`、`QuestUIController.cs`、`FinalRPG.inputactions`、PinePie 代码 — 全部原封不动。

## 踩坑记录

### 1. Awake/Start 执行顺序导致 intentProvider 未替换

**症状**：摇杆/攻击/闪避不响应，键盘正常。

**根因**：`PlayerStateDriver.Awake()` 中 `InitializeIntentProvider()` 读取 `intentProviderOverride`，此时 `MobileInputBridge.Start()` 尚未执行（override 为 null），因此使用了默认 `PlayerInputProvider`。

**修复**：`MobileInputBridge` 在 `Awake()` 中设置 override 并创建键盘回落；`PlayerStateDriver` 将 `InitializeIntentProvider()` 从 `Awake()` 移到 `Start()`。

### 2. UI Toolkit Y 轴方向与游戏坐标系相反

**症状**：摇杆向前推角色向后走。

**根因**：UI Toolkit 原点在左上角（Y↓），游戏世界 Y+ 为前方。`UIToolkitJoystick.UpdateHandle()` 中 `offset = localPos - center` 产生的 Y 值与游戏预期相反。

**修复**：计算游戏方向时翻转 Y：`offset.y = -(localPos.y - center.y)`；手柄视觉仍用原始坐标系。

### 3. 摇杆推满 Speed 仅 0.5

**症状**：Animator Speed 参数最大只能到 0.5（走），无法达到 1.0（跑）。

**根因**：`GetTargetRealSpeed()` 在 `runHeld=false` 时基准速度为 `moveSpeed(6)`，而 Animator Speed 公式为 `velocity / runReal(12)`，上限 = 6/12 = 0.5。

**修复**：摇杆活跃时强制 `ctx.runHeld = true`，使基准速度为 `runReal(12)`，再由摇杆位移量线性缩放。轻推=慢走，推满=全速跑，Speed 从 0 平滑到 1.0。

## 场景配置步骤

1. 创建 `MobileHUD` GameObject，挂载 `UIDocument`（引用 `MobileHUD.uxml`）
2. 挂载 `MobileInputBridge` 组件
3. Inspector 中拖入：`_uiDoc`、`_playerStateDriver`（玩家身上）、`_joystickBaseSprite`（`joystick Circle`）、`_joystickHandleSprite`（`joystick Thumb`）
4. 无需额外 Canvas/PinePie JoystickController 组件
