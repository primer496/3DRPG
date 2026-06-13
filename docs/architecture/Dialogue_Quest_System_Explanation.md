# 对话与任务系统全流程说明

## 整体架构

```
玩家输入 (E键)
    │
    ▼
NPCInteractable          ← 触发入口
    │ StartDialogue()
    ▼
DialogueRunner (Yarn Spinner)  ← 对话引擎
    │ 解析 .yarn 脚本
    ▼
Yarn 脚本节点             ← 内容 + 流程控制
    │ <<Commands>>
    ▼
QuestYarnIntegration      ← Yarn 命令 → C# 桥接
    │ AcceptQuest / CompleteQuest
    ▼
QuestManager              ← 任务数据模型 (M)
    │ OnQuestUpdated 事件
    ▼
QuestViewModel            ← 数据绑定层 (VM)
    │ bindableData.SetActiveQuest → OnQuestChanged 事件
    ▼
QuestUIController         ← UI 渲染层 (V)
    │ RefreshUI()
    ▼
UI Toolkit (UXML/USS)     ← 任务日志面板
```

---

## 各文件职责

### 触发层

**`NPCInteractable.cs`**  
挂在村长 NPC 上。玩家进入 Collider 范围后按 E，调用 `dialogueRunner.StartDialogue("Start")`，同时向 EventBus 抛出 `Communicate/VillageChief` 事件（用于推进任务中"与村长对话"类型的目标）。

---

### 对话层（.yarn 脚本）

所有脚本放在 `Assets/Resources/GameConfigs/Dialogue/`，由 Yarn Spinner 的 `Project.yarnproject` 统一注册。

| 文件 | 节点 | 作用 |
|---|---|---|
| `Start.yarn` | `Start` | 主入口，所有对话分支的总调度中心 |
| `Quest_Accepted.yarn` | `Quest_Accepted` | 首次接任务后的回应对话 |
| `Progress_1/2.yarn` | `Progress_1/2` | 汇报线索时的剧情反应对话 |
| `Phase2_Waiting.yarn` | `Phase2_Waiting` | 选择完成方式后、未完成前的等待回环；击杀后再对话时在此完成任务 |
| `Ending_Fight/Peace.yarn` | `Ending_Fight/Peace` | 最终结局剧情（当前版本未使用，保留给扩展） |
| `DailyDialogueEnd.yarn` | `DailyDialogueEnd` | 任务完成后的日常回环终止节点 |
| `DialogueEnd.yarn` | `DialogueEnd` | 通用对话结束节点 |

**`Start.yarn` 内部的状态机逻辑**（用 Yarn 变量实现）：

```
$QuestCompleted == true  →  跳 DailyDialogueEnd（永久日常回环）
  ↑
$QuestAccepted == true
  ├─ $InvestigationProgress == 0  →  等待提交第一条线索
  ├─ $InvestigationProgress == 1  →  等待提交第二条线索
  └─ $InvestigationProgress == 2
        ├─ $Phase2Accepted == true  →  跳 Phase2_Waiting（等待完成）
        └─ 否则  →  选择结局分支（击杀 / 和平）
  ↑
$QuestRejected == false  →  首次对话，发出委托
$QuestRejected == true   →  二次邀请
```

---

### Yarn 命令桥接层

**`QuestYarnIntegration.cs`**  
Yarn 脚本里写的 `<<AcceptQuest>>` 等都是自定义命令，必须在 C# 里注册才能执行。这个脚本的 `Awake()` 向 `DialogueRunner` 注册所有命令。`QuestManager.Start()` 调用 `EnsureRegistered()`，确保懒单例创建后命令也被注册。

| Yarn 命令 | C# 方法 | 作用 |
|---|---|---|
| `<<AcceptQuest id>>` | `QuestManager.AcceptQuest(data)` | 接取任务 |
| `<<CompleteQuest id>>` | `QuestManager.CompleteQuest(id)` | 强制完成任务（对话完成后） |
| `<<GivePlayerItem id n>>` | `EventBus.Raise(Collect, id, n)` | 给玩家物品（目前触发收集事件） |
| `<<TriggerCommunicateEvent id>>` | `EventBus.Raise(Communicate, id, 1)` | 触发对话类任务目标 |
| `<<SetReputation faction n>>` | 日志占位 | 声望系统预留 |

---

### 任务模型层

**`QuestData.cs`** (ScriptableObject)  
静态数据配置，放在 `Assets/Resources/GameConfigs/Quest/`。文件名即任务 ID。包含：
- `id`、`title`、`description`
- `isOrdered`：目标是否按顺序完成
- `objectives[]`：每个目标有 `targetType (Kill/Collect/Communicate)`、`targetId`、`requiredAmount`
- `rewards[]`

**`QuestInstance`** (运行时类，嵌套在 QuestManager.cs)  
运行时状态，包含 `progressList[]`（每个目标当前进度）、`currentActiveIndex`、`isCompleted`。

**`QuestManager.cs`**  
单例，管理所有运行中的 `QuestInstance`。核心逻辑：
- `AcceptQuest(data)` → 创建 QuestInstance，加入 `activeQuests`，触发 `OnQuestUpdated`
- `HandleGameActivity(targetType, targetId, amount)` → 收到 EventBus 事件后遍历 activeQuests，匹配目标并累加进度，完成目标则 `currentActiveIndex++`，全部完成则 `MarkQuestCompleted()`，每次变更触发 `OnQuestUpdated`
- 关键：**`targetId` 必须和 EventBus 上报的字符串完全一致**（本次修复的核心问题）

**`EventBus.cs`**  
非 MonoBehaviour 单例，解耦各系统间的通信。核心事件 `OnGameActivityTriggered(TargetType, string, int)` 被：
- `NPCInteractable`（触发 Communicate 事件）
- `EnemyHealth`（触发 Kill 事件）
- `QuestYarnIntegration`（触发 Collect 事件）

三方上报，由 QuestManager 统一消费。

---

### 战斗层（本次新增）

**`EnemyHealth.cs`**  
挂在敌人根对象上。职责：
- `TakeDamage(1)` 被 `WeaponDetector` 在命中时调用
- 血量归零 → `EventBus.Raise(Kill, "ForestMonster", 1)` → QuestManager 处理
- 同时 `InMemoryVariableStorage.SetValue("$MonsterKilled", true)` → Yarn 脚本感知到击杀状态

**`WeaponDetector.cs`**  
玩家武器骨骼上用 SphereCast 做命中检测。命中 Enemy Layer 的 Collider 后：
1. 触发 `PlayerStateDriver.ctx.isHit = true`（敌人硬直状态机）
2. 调用 `EnemyHealth.TakeDamage(1)`

---

### MVVM UI 层

**`QuestViewModel.cs`**  
挂在 QuestManager 同一个 GameObject 上（DontDestroyOnLoad）。订阅 `QuestManager.OnQuestUpdated`，收到后调用 `bindableData.SetActiveQuest()`，将 QuestInstance 转换为 UI 可消费的纯数据。

**`QuestBindableData.cs`**  
纯 C# 类（非 MonoBehaviour），持有当前展示任务的数据快照。每次 `SetActiveQuest()` 调用都触发 `OnQuestChanged` 事件。

**`QuestUIController.cs`**  
挂在场景里 Quest UI GameObject 上（带 UIDocument 组件）。订阅 `bindableData.OnQuestChanged`，调用 `RefreshUI()` 用 UI Toolkit API 更新 UXML 里的 Label、ProgressBar、目标列表。`ToggleQuestLog()` 由 EventBus 字符串事件 `"ToggleQuestLog"` 触发，打开面板时调用 `viewModel.RefreshToLatestQuest()` 主动刷一次。

---

## 完整流程时序

```
【第一阶段：接取调查任务】
玩家 E → NPCInteractable → DialogueRunner("Start")
  → Yarn: $QuestAccepted=false, $QuestRejected=false → 首次对话
  → 玩家选"包在我身上" → <<AcceptQuest ClearBlackForest_Phase1_Investigate>>
  → QuestYarnIntegration.AcceptQuest() → QuestManager.AcceptQuest()
  → activeQuests.Add(Phase1实例) → OnQuestUpdated → QuestViewModel → bindableData → UI刷新
  → <<TriggerCommunicateEvent VillageChief>> → EventBus(Communicate, VillageChief)
  → QuestManager.HandleGameActivity → Phase1目标0完成 → currentActiveIndex=1 → UI再刷新

【第二阶段：调查线索（重复对话汇报）】
  → <<GivePlayerItem Clue_Footprint 1>> → EventBus(Collect) → Phase1目标1进度+1
  → $InvestigationProgress=1 → 再次对话汇报第二条线索同理

【第三阶段：选择结局分支】
  → $InvestigationProgress==2 → <<TriggerCommunicateEvent VillageChief>> → Phase1目标3完成
  → 玩家选"击杀" → $Phase2Accepted=true
  → <<AcceptQuest ClearBlackForest_Phase2_Kill>>
  → QuestManager接取Phase2实例 → OnQuestUpdated → UI切换到Phase2任务

【第四阶段：击杀敌人】
  玩家攻击 → WeaponDetector.ProcessHit() → EnemyHealth.TakeDamage(1) × 3
  → EnemyHealth.Die() → EventBus(Kill, "ForestMonster")
  → QuestManager.HandleGameActivity → Phase2目标0(ForestMonster 0→1) → currentActiveIndex=1
  → OnQuestUpdated → QuestViewModel → bindableData → UI立即刷新（目标勾选）
  → storage.SetValue("$MonsterKilled", true)
  → Destroy(敌人GO)

【第五阶段：回报完成】
  玩家 E → NPCInteractable → DialogueRunner("Start")
  → $QuestAccepted=true, $InvestigationProgress==2, $Phase2Accepted=true → jump Phase2_Waiting
  → $MonsterKilled==true → 村长庆祝台词
  → <<CompleteQuest ClearBlackForest_Phase2_Kill>>
  → QuestManager.MarkQuestCompleted() → isCompleted=true → OnQuestUpdated
  → <<set $QuestCompleted = true>> → jump DailyDialogueEnd
  → 此后每次对话直接进日常回环
```

---

## 本次对话中遇到的问题及解决办法

### 问题 1: `<<AcceptQuest>>` 命令未执行
**原因**: Yarn 脚本中任务 ID 与 ScriptableObject 文件名不一致。
**解决**: 修正 Yarn 文件中的任务 ID，确保与 SO 文件名完全匹配。

### 问题 2: Phase2 对话重复显示选择分支
**原因**: 缺少状态变量记录玩家是否已选择分支。
**解决**: 添加 `$Phase2Accepted` 变量，并在 Yarn 中跳转到 `Phase2_Waiting` 节点。

### 问题 3: 击杀任务 UI 不更新
**原因**: SO 中 `targetId` 为 `Monster_BlackForestBoss`，而敌人 `EnemyHealth.enemyId` 为 `ForestMonster`。
**解决**: 修正 SO 的 `targetId` 为 `ForestMonster`。

### 问题 4: QuestManager 在游戏开始时被销毁
**原因**: `QuestManager.Instance` 在 `OnEnable` 中直接访问，触发懒单例创建，导致场景中的真实对象被销毁。
**解决**: 恢复原逻辑，`OnEnable` 仅重新订阅 `bindableData`，避免直接访问 `QuestManager.Instance`。

### 问题 5: QuestUIController 订阅丢失
**原因**: `OnDisable` 中取消订阅，`OnEnable` 未重新订阅。
**解决**: 在 `OnEnable` 中添加重新订阅逻辑。