# 任务/存档/Yarn 变量 — 三项连锁修复

**日期**: 2026-06-12  
**类型**: Bug 修复  
**影响系统**: 任务系统、存档系统、对话系统（Yarn 变量持久化）

---

## 问题 1：已完成任务累积导致任务面板卡死

### 症状
- `activeQuests.Count` 逐次启动递增（3→5→7…）
- 任务面板永远显示第一个未完成任务的旧状态
- 全任务完成后重开面板仍显示旧数据

### 根因
`SaveSystem.CollectSaveData` 把**已完成任务也存入了 autosave.json**。下次启动 → `LoadAutoSave` 恢复所有已完成任务 → Yarn `<<AcceptQuest>>` 发现该 ID 的未完成任务不存在（旧的已完成）→ 再次创建新实例。多次循环后 `activeQuests` 不断膨胀。

### 修复
`CollectSaveData` 增加过滤：`if (quest.isCompleted) continue;`

---

## 问题 2：线索提交不更新任务面板进度

### 症状
对话中选「提交线索」后，任务面板进度条/目标状态不变。

### 根因
Yarn `<<GivePlayerItem QUEST_001 1>>` → `EventBus.Raise(Collect, "QUEST_001")` → `QuestManager.HandleGameActivity` 匹配失败。任务的 objective targetId 与 Yarn 给的 itemId 不一致，Collect 事件无法推进任务目标。

### 修复（两步）
1. **QuestManager 新增 `AdvanceQuestObjective(questId)`** — 直接将当前目标标记完成、推进 currentActiveIndex、触发 `OnQuestUpdated` 通知 UI 刷新。不依赖 Collect 匹配。
2. **QuestYarnIntegration 注册 `<<AdvanceQuestObjective questId>>` Yarn 命令**。
3. **Start.yarn 两处线索提交后各加 `<<AdvanceQuestObjective ClearBlackForest_Phase1_Investigate>>`**。

---

## 问题 3：Yarn 对话变量未持久化

### 症状
读档后对话从最初状态（接任务前）开始，而非上次退出时的进度。任务数据正确但对话分支错误。

### 根因
存档系统只保存了任务数据（`activeQuests`），未保存 Yarn 的 `InMemoryVariableStorage` 变量（`$QuestCompleted`、`$QuestAccepted`、`$InvestigationProgress` 等）。对话引擎根据这些变量决定从哪个分支开始。

### 修复
- `SaveData` 新增 `YarnVariableEntry`（name + value + wasBool 类型标记）
- `SaveSystem.CollectSaveData` 从 `InMemoryVariableStorage` 读取已知变量
- `SaveSystem.RestoreFromSaveData` 回写到 `InMemoryVariableStorage`
- 变量类型显式声明（见下文）

---

## 问题 4：Yarn 变量类型推断异常

### 症状
```
InvalidCastException: Variable $QuestCompleted exists, but is the wrong type
(expected System.Single, got System.Boolean)
InvalidCastException: Variable $InvestigationProgress exists, but is the wrong type
(expected System.Boolean, got System.Single)
```

### 根因
Yarn 的 `InMemoryVariableStorage.TryGetValue<T>()` **类型不匹配时直接抛 `InvalidCastException`**，不返回 false。无法用「先试 bool 再试 float」的 fallback 模式。同时 Yarn 内部 bool 和 float 变量以不同 C# 类型存储。

### 修复
改用 `Dictionary<string, bool>` 显式声明每个变量的类型（true=bool, false=float），保存/恢复时直接用正确类型读写，彻底消除类型异常：

| 变量 | 类型 |
|---|---|
| `$QuestCompleted` | bool |
| `$QuestAccepted` | bool |
| `$QuestRejected` | bool |
| `$Phase2Accepted` | bool |
| `$InvestigationProgress` | float |

---

## 经验教训

1. **Yarn 变量是存档盲区** — 任务数据恢复 ≠ 对话状态恢复。两者必须一起持久化，否则对话引擎无法定位正确分支。
2. **`InMemoryVariableStorage.TryGetValue<T>()` 不是安全的类型探测** — 类型不匹配时抛异常。必须预先知道变量类型，不能靠运行时 fallback。
3. **已完成的存档数据会污染读档** — 只保存活跃状态（未完成任务），已完成/已失效的数据会导致重复累积。
4. **Collect 事件匹配依赖 Quest SO 配置** — SO 中 objective.targetId 必须和 Yarn 中 `<<GivePlayerItem>>` 的 itemId 一致才能自动推进。不一致时用 `<<AdvanceQuestObjective>>` 直接推进。

## 涉及文件
- `Assets/Scripts/SaveSystem/SaveData.cs`
- `Assets/Scripts/SaveSystem/SaveSystem.cs`
- `Assets/Scripts/QuestSystem/QuestManager.cs`
- `Assets/Scripts/QuestSystem/QuestYarnIntegration.cs`
- `Assets/Resources/GameConfigs/Dialogue/Start.yarn`
