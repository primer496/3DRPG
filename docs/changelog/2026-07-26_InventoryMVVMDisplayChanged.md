# 背包 View 经 ViewModel 中转刷新格子

**日期**: 2026-07-26  
**类型**: 架构修正  
**影响系统**: 背包系统（InventorySystem）  
**严重程度**: 低（行为不变，分层更干净）

---

## 问题

`InventoryUIController` 在 `OnEnable` 里直接订阅了：

```csharp
viewModel.inventoryModel.OnInventoryChanged += RefreshUI;
```

这导致 **View 感知 Model**（通过 `viewModel.inventoryModel`），违反 MVVM 单向依赖：`View → ViewModel → Model`。

功能上能工作（格子仍会刷新），但：
- View 与 Model 耦合，换 UI 实现时仍要知道 Model 事件名
- ViewModel 本应是唯一对 View 暴露的门面，却被绕过

---

## 修复

1. **`InventoryViewModel`** 新增 `public event Action OnDisplayChanged;`
2. 在 `RefreshDisplaySlots()` 末尾调用 `OnDisplayChanged?.Invoke()`（分类切换、排序、Model 变更等最终都会走到此方法）
3. **`InventoryUIController`** 改为订阅 `viewModel.OnDisplayChanged`，不再访问 `inventoryModel`

数据流：

```
Model.OnInventoryChanged
  → ViewModel.HandleModelChanged
  → RefreshDisplaySlots()
  → OnDisplayChanged
  → View.RefreshUI
```

---

## 分层约定（摘要）

| 层 | 知道谁 | 不知道谁 |
|---|---|---|
| View | ViewModel | Model |
| ViewModel | Model | View |
| Model | 仅自身数据 | ViewModel / View |

项目内背包仍用手写 C# 事件做单向通知，不是 WPF/UI Toolkit 声明式双向绑定引擎；但「谁知道谁」的方向已对齐纯正 MVVM。
