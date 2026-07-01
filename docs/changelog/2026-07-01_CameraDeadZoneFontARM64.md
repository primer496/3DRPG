# 移动端相机死区 & 中文显示 & ARM64 修复

> 2026-07-01 | 修改 2 个文件 + 新增 1 个字体资源

## 相机死区（核心修复）

### 问题演进
1. 初始方案：`EventSystem.current.IsPointerOverGameObject()` — 全屏 root 被识别为 UI，所有触摸被拦截
2. 手指追踪方案：`HashSet<int>` 追踪 UI 交互手指 — 全局布尔无法区分双手
3. **最终方案**：触摸相机与 Input System 完全分离

### 最终架构

```
鼠标 → OnLook(InputValue) → Input System "Look" 动作 (<Pointer>/delta, 仅 ;Keyboard&Mouse)
触摸 → Update() → EnhancedTouch.Touch.activeTouches → screenPosition.x > 0.45 * Screen.width
```

- `FinalRPG.inputactions`：Look 绑定的 `<Pointer>/delta` 从 `";Keyboard&Mouse;Touch"` 改为 `";Keyboard&Mouse"`
- `ThirdPersonCamera.Update()`：用 `UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches` 直读触摸，`screenPosition.x > Screen.width * 0.45f` 过滤右半屏
- `ThirdPersonCamera.OnLook()`：恢复为纯鼠标处理
- 左半屏 = 摇杆死区，任何触摸不转相机；右半屏 = 相机区

### 原因分析
`OnLook(InputValue)` 中 `<Pointer>/delta` 会合并所有触摸 delta 后一次性传入回调，无法在回调内区分左右手。必须在更底层（原生触摸 API）做过滤。

## 中文文本显示

- 字体 `NotoSansSC-Regular.ttf` 从 AI Graph 包复制到 `Assets/Resources/Fonts/`
- `MobileInputBridge.Awake()` 中用 `Resources.Load<Font>("Fonts/NotoSansSC-Regular")` 加载并赋给 `root.style.unityFont`
- USS `resource()` 方式在移动端不可靠

## ARM64 构建

- `ProjectSettings.asset`：`AndroidTargetArchitectures: 3` (ARMv7+ARM64)
- `ProjectSettings.asset`：`scriptingBackend: Android: 1` (IL2CPP)

## 修改文件

| 文件 | 改动 |
|---|---|
| `Assets/FinalRPG.inputactions` | Look 绑定移除 `;Touch` |
| `Assets/Scripts/Camera/ThirdPersonCamera.cs` | +EnhancedTouch 触摸相机，+touchSensitivity 字段，简化 OnLook |
| `Assets/Resources/Fonts/NotoSansSC-Regular.ttf` | 新增中文字体 |
| `Assets/Scripts/UI/MobileInputBridge.cs` | C# 加载中文字体 |
