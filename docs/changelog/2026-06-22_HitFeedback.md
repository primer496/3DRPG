# 攻击命中反馈系统实施报告

**日期**: 2026-06-22  
**类型**: 新增功能  

---

## 概述

为战斗系统增加打击感反馈：**帧冻结**（Hit Stop）和 **镜头晃动**（Camera Shake）。

## 技术方案

| 效果 | 实现 | 触发条件 |
|---|---|---|
| **帧冻结** | DOTween `DOTween.To().From().SetUpdate(true)` 驱动 `Time.timeScale` | 仅玩家→敌人 |
| **镜头晃动** | Cinemachine Impulse (`CinemachineImpulseSource` + `CinemachineImpulseListener`) | 双向，强度不同 |

## 新增文件

### `Assets/Scripts/Combat/FrameFreezeController.cs`

- MonoBehaviour，挂载于 `HitFeedbackManager` GameObject
- 订阅 `EventBus.OnAttackHit`，仅响应 `isPlayerAttack == true`
- 每次先 `_freezeTween?.Kill()` 再创建新 tween，防止连段竞争
- `SetUpdate(true)` 确保自身不受 timeScale 冻结影响
- 可调参数：`_freezeDuration`（默认 0.05s）、`_freezeTimeScale`（默认 0.1f）、`_freezeEase`（默认 `Ease.OutQuad`）

### `Assets/Scripts/Camera/CameraShakeController.cs`

- MonoBehaviour，挂载于 `HitFeedbackManager` GameObject
- 订阅 `EventBus.OnAttackHit`，根据 `isPlayerAttack` 选强度
- 用 `[RequireComponent(typeof(CinemachineImpulseSource))]` + `Awake` 中 `GetComponent` 自动获取
- 晃动方向：从命中点指向相机，`(camPos - worldPos).normalized * intensity`
- 可调参数：`_playerAttackShakeIntensity`（默认 0.2f）、`_enemyAttackShakeIntensity`（默认 0.5f）

## 修改文件

| 文件 | 改动 |
|---|---|
| `EventBus.cs` | + `Action<Vector3, bool> OnAttackHit` + `RaiseAttackHit()` |
| `WeaponDetector.cs` | `ApplyHit()` 末尾 `RaiseAttackHit(pos, isPlayerAttack: true)` |
| `EnemyAttackDetector.cs` | `ApplyHit()` 末尾 `RaiseAttackHit(pos, isPlayerAttack: false)` |

## 场景变更

| 对象 | 新增组件 |
|---|---|
| `ThirdPersonCamera` (CinemachineVirtualCamera) | `CinemachineImpulseListener` |
| `HitFeedbackManager` (新建空 GameObject) | `FrameFreezeController` + `CameraShakeController` + `CinemachineImpulseSource` |

## 踩坑记录

1. **`CinemachineImpulseListener` 必须放在 VCam 上**，不能放在 Main Camera（带 `CinemachineBrain`）上。Cinemachine 2.x 的 `CinemachineExtension` 要求同 GameObject 有 `CinemachineVirtualCameraBase`，而 `CinemachineBrain` 不继承该类。

2. **`CinemachineImpulseSource` 的 NoiseSettings 非必须** — 默认 `ImpulseDefinition` 使用 6D Shake + 默认 Amplitude/Frequency，开箱即用。

## 设计决策

- **帧冻结仅玩家→敌人**：玩家受击时已在 `HitReaction` 硬直中，加冻结会造成"卡输入"体感，不符合动作游戏业界惯例。
- **震屏双向不同强度**：玩家命中轻晃（正反馈 0.2），受击重晃（警示信号 0.5）。
- **DOTween 用于帧冻结**：比手动 Update 轮询更简洁，内置缓出曲线（OutQuad）让 timeScale 恢复更自然。
- **震屏用 Cinemachine Impulse**：DOTween 的 `DOShakePosition()` 直接写 Camera transform，与 CinemachineBrain 的 LateUpdate 输出冲突，无法共存。
