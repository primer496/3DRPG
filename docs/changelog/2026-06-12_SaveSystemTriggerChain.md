# 存档系统闭环 — 从零触发到完整自动存档链路

**日期**: 2026-06-12  
**类型**: 功能实现  
**影响系统**: 存档系统、对话系统、任务系统  
**严重程度**: 高（存档完全无效）

---

## 背景

上一轮实现了 SaveSystem 的全部机制（序列化/反序列化、文件 I/O、PlayerStats/Inventory/Quest 数据收集与恢复），但遗漏了最关键的一环：**没有任何代码在运行时触发 Save 或 Load**。

结果：第二次进入游戏时，autosave.json 要么不存在，要么存在但从未被读取，所有数据仍是默认初始值。

## 根因分析

存档系统需要四个时刻的触发：

| 触发点 | 操作 | 缺失状态 |
|--------|------|----------|
| 启动时 | LoadAutoSave() | ❌ 无人调用 |
| 退出时 | AutoSave() | ❌ 无人调用 |
| 剧情节点 | Yarn `<<AutoSave>>` | ❌ 命令未注册 |
| 读档后 | QuestViewModel 刷新 UI | ❌ 未订阅 QuestsRestored |

`RuntimeInitializeOnLoadMethod` 只解决了 GameObject 创建问题，未解决「何时读档」问题。

## 修改内容

### 1. QuestYarnIntegration.cs — 注册 `<<AutoSave>>` Yarn 命令
```csharp
dialogueRunner.AddCommandHandler("AutoSave", () => {
    EventBus.Instance.Raise("AutoSave");
});
```
剧情作者在 Yarn 关键节点插入 `<<AutoSave>>` 即可触发自动存档。

### 2. SaveSystem.cs — 启动自动读档
`Start()` 末尾新增：
```csharp
if (HasAutoSave()) { LoadAutoSave(); }
```
选择 `Start()` 而非 `Awake()` 的原因：`Start()` 在所有场景对象的 `Awake()` 之后执行，此时 `PlayerStatsProvider`、`InventoryViewModel` 等已就绪。

### 3. SaveSystem.cs — 退出自动存档
新增 `OnApplicationQuit()` → `AutoSave()`。

### 4. QuestViewModel.cs — 读档后任务 UI 刷新
- `OnEnable()` 订阅 `EventBus("QuestsRestored")` → `HandleQuestsRestored()`
- `OnDisable()` 取消订阅
- `HandleQuestsRestored()` 调用 `RefreshToLatestQuest()` 驱动 UI 更新

## 完整生命周期

```
首次启动 → autosave.json 不存在 → 跳过，默认值开始
    ↓
Yarn <<AutoSave>> / Application.Quit → RawAutoSave() → autosave.json
    ↓
再次启动 → Start() → HasAutoSave()=true → LoadAutoSave()
    → PlayerStats.RestoreFromSave + UI 刷新
    → InventoryModel.LoadFromSave + UI 刷新
    → RestoreQuestData + EventBus("QuestsRestored") → QuestViewModel 刷新
```

## 经验教训

1. **存档系统的"最后一公里"是触发时机** — 序列化/反序列化机制只是基础设施，没有明确的 Save/Load 调用点，整个系统形同虚设。
2. **`Start()` 是安全的自动读档时机** — 此时所有场景对象的 `Awake()` 已完成，`FindObjectOfType` 能正常找到依赖。
3. **懒加载单例 + `RuntimeInitializeOnLoadMethod` 只解决"存在性"**，不能替代"何时执行"的设计决策。

## 相关文件
- `Assets/Scripts/SaveSystem/SaveSystem.cs`
- `Assets/Scripts/QuestSystem/QuestYarnIntegration.cs`
- `Assets/Scripts/QuestSystem/ViewModel/QuestViewModel.cs`
