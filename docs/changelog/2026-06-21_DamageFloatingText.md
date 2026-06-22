# 伤害跳字系统 — 实施记录

> **日期**: 2026-06-21 | **状态**: ✅ 完成

## 概述

在 EventBus 架构上新增 `OnDamagePopup` 事件，由 `FloatingTextController`（独立 UIDocument 屏幕空间叠加层）订阅，在角色头顶生成红色上飘淡出的伤害数字。覆盖玩家受伤和敌人受伤双向。

## 新增文件

| 文件 | 说明 |
|---|---|
| `Assets/Scripts/UI/FloatingTextController.cs` | 核心控制器：EventBus 订阅、Label 对象池、Update 轮询动画、坐标转换 |
| `Assets/UIToolKit/FloatingText/FloatingText.uxml` | 根容器模板（`picking-mode="Ignore"`） |
| `Assets/UIToolKit/FloatingText/FloatingText.uss` | `.damage-text` 红色粗体 + 黑色描边，预留 `.damage-crit` / `.heal-text` |

## 修改文件

| 文件 | 变更 |
|---|---|
| `EventBus.cs` | 新增 `OnDamagePopup(int, Vector3)` 事件 + `RaiseDamagePopup()` + `using UnityEngine` |
| `EnemyAttackDetector.cs` | `ApplyHit()` 中追加 `RaiseDamagePopup`（玩家位置）；新增 `_hitDrivers` HashSet 去重 |
| `WeaponDetector.cs` | `ProcessHit()` 中追加 `RaiseDamagePopup`（敌人位置）+ `using TaskManager` |

## 场景变更

- 新建 `FloatingTextCanvas` GameObject，挂载 `UIDocument`（Source Asset=`FloatingText.uxml`）+ `FloatingTextController`
- PanelSettings Sort Order 设为最高值，确保跳字在所有 UI 之上

## 踩坑记录

1. **`_uiDoc.rootVisualElement.Q<>()` 查不到根元素** — `.Q()` 只搜索子元素，UXML 根元素直接就是 `rootVisualElement` 本身。正确写法：`_root = _uiDoc.rootVisualElement`。

2. **`Camera.WorldToScreenPoint` 与 UI Toolkit 坐标系不兼容** — PanelSettings 有独立的参考分辨率和缩放模式，屏幕像素坐标不等于面板坐标。正确做法：使用 UI Toolkit 原生 API `RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, worldPos, cam)`。

3. **跳字在脚底而非头顶** — 世界坐标传入的是 `transform.position`（脚底），需加 `_headHeight` Y 轴偏移到头顶。

4. **Z 轴随机散布导致透视偏移** — 在透视相机下，世界 Z 轴偏移会显著改变屏幕投影位置。只保留 X 轴微小散布即可。

5. **USS 样式不生效时 Label 不可见** — 在 `GetFromPool()` 中加内联样式兜底（`style.color`、`style.fontSize`、`style.unityFontStyleAndWeight`）。

6. **一次攻击触发多次跳字** — `EnemyAttackDetector.PerformSectorAttack` 遍历 `Physics.OverlapSphere` 所有碰撞体，玩家身上多个 Collider 各自命中同一 `PlayerStateDriver`，导致 `ApplyHit` 重复调用。修复：新增 `_hitDrivers` HashSet 去重（参照 `WeaponDetector` 已有模式）。
