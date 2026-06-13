# FinalRPG — AI 协作规范 (AGENTS.md)

> 本文档为 AI 编程助手提供项目级上下文。每次对话自动加载。
> 当你在项目中添加新规则、修复 Bug 后学到教训，请更新此文件。

---

## 一、项目概览

Unity 2022.3+ 的 2.5D/3D RPG 示例项目，渲染管线为 **URP**。

### 核心系统
| 系统 | 路径 | 说明 |
|---|---|---|
| **玩家控制 & 状态机 (HSM)** | `Assets/Scripts/Unity-HSM/` | 分层状态机，每帧轮询，不用 await |
| **背包系统** | `Assets/Scripts/InventorySystem/` | Model-ViewModel-View，SO 存储道具数据 |
| **任务系统** | `Assets/Scripts/QuestSystem/` | SO 存储任务数据，Yarn Spinner 驱动对话 |
| **对话系统** | `Assets/Scripts/DialogueSystem/` | Presenter-Model-Adapter 架构，Yarn 集成 |
| **玩家属性** | `Assets/Scripts/Player/` | PlayerStats 纯数据 + Provider 桥接层 |
| **摄像机** | `Assets/Scripts/Camera/` | Cinemachine 第三人称 |

---

## 二、命名空间约定

| 命名空间 | 用途 |
|---|---|
| `HSM` | 玩家状态机相关（PlayerStateDriver, State, Activity 等） |
| `InventorySystem.Model` | 背包数据层（ItemData SO, InventoryModel） |
| `InventorySystem.ViewModel` | 背包 VM 层 |
| `InventorySystem.View` | 背包 UI 层 |
| `InventorySystem.Utils` | 背包工具类 |
| `QuestSystem` | 任务系统 |
| `QuestSystem.ViewModel` | 任务 VM 层 |
| `QuestSystem.View` | 任务 UI 层 |
| `QuestSystem.Interaction` | NPC 交互 |
| `TaskManager` | EventBus、QuestData SO、QuestManager（历史命名遗留，不要改名） |
| `DialogueSystem.Model` | 对话数据层 |
| `DialogueSystem.Presenter` | 对话表现层 |
| `DialogueSystem.Adapter` | Yarn 适配器 |
| `DialogueSystem.View` | 对话 UI 层 |

---

## 三、架构约定（强制）

### 3.1 通信方式：EventBus，禁止直接引用
```csharp
// ✅ 正确：通过 EventBus 解耦
EventBus.Instance.OnGoldRewarded += AddGold;
EventBus.Instance.RaiseInputLock(true);

// ❌ 错误：跨系统直接引用
InventorySystem.Instance.AddItem(...);   // 不存在这种写法
QuestManager.Instance.CompleteQuest(...); // 不要这样做
```
系统间通信一律走 `EventBus`（单例，位于 `TaskManager` 命名空间）。主要事件：
- `OnInputLockStateChanged(bool)` — 输入锁定
- `OnItemRewarded(string itemId, int amount)` — 物品奖励
- `OnGoldRewarded(int)` / `OnExpRewarded(int)` — 金币/经验
- `OnPlayerDamaged(int)` — 伤害

### 3.2 UI 框架：UI Toolkit (UIDocument)
- 所有 UI 用 `UIDocument` + `VisualElement`，**禁止使用 uGUI (Canvas/Image/Text)**
- UI 查询用 `_root.Q<ProgressBar>("hp-bar")` 模式
- 显示/隐藏用 `DisplayStyle.None` / `DisplayStyle.Flex`

### 3.3 配置数据：ScriptableObject
- 所有策划配置（道具、任务、怪物属性、角色参数）用 **ScriptableObject**
- 放在 `Assets/Resources/GameConfigs/` 下，运行时 `Resources.Load<T>(path)` 加载
- 不要用 JSON/XML/CSV 做运行时配置
- SO 菜单路径约定：`RPG/Inventory/...`、`Quest System/...`

### 3.4 异步模式：禁止 await，用轮询
- **主循环 (Update) 里严禁 `async/await`**
- 异步操作用 **Task + 每帧轮询 `IsCompleted`** 模式（见 `TransitionSequencer`）
- 协程也尽量避免，优先用 Task 轮询

### 3.5 MVC/MVP 分层
- **Model** — 纯 C# 数据类，不继承 MonoBehaviour
- **ViewModel/Presenter** — MonoBehaviour 桥接层，监听 Model 变化，驱动 View
- **View** — MonoBehaviour 挂 UI 上，只负责显示，不包含业务逻辑
- 例：`ItemData(SO/Model)` → `InventoryViewModel` → `InventoryUIController(View)`

---

## 四、命名与代码风格

### C# 命名
```csharp
// 私有字段：_camelCase
private PlayerStats _stats;
private ProgressBar _hpBar;
private bool _locked;

// 公共属性：PascalCase
public PlayerStats Stats { get; private set; }
public int CurrentHP { get; set; }

// 方法：PascalCase
private void CacheElements() { }
public void AddGold(int amount) { }

// 事件：On + 过去式
public event Action<int> OnGoldRewarded;
public event Action<bool> OnInputLockStateChanged;
```

### ScriptableObject 文件命名
- 文件名 = `[类型][名称]`，如 `ItemData_HealthPotion.asset`、`QuestData_Tutorial.asset`
- CreateAssetMenu 的 fileName 用描述性名称：`NewItemAsset`、`NewQuest`

### 序列化字段
- 用 `[field: SerializeField]` 自动属性
- `[Header("...")]` 分组
- `[Tooltip("...")]` 写清楚用途
- `[TextArea(3,5)]` 给长文本字段

---

## 五、关键路径速查

| 用途 | 路径 |
|---|---|
| 玩家预制体 | `Assets/Resources/Prefab/MainCharacter.prefab` |
| 道具 SO | `Assets/Resources/GameConfigs/PackageModel/` |
| 道具图标 | `Assets/Resources/PackageIcon/` |
| 角色配置 | `Assets/Resources/Character/` |
| 主场景 | `Assets/Scenes/SampleScene.unity` |

### 参考文档（按需手动引用）
| 文档 | 路径 |
|---|---|
| 项目代码结构说明 | `docs/architecture/项目代码结构说明.md` |
| 对话任务系统详解 | `docs/architecture/Dialogue_Quest_System_Explanation.md` |
| 对话 MVP 重构总结 | `docs/changelog/DialogueTransitionSummary.md` |
| 属性&背包闭环报告 | `docs/changelog/RPG属性系统与背包任务闭环_实施报告.md` |
| 斜坡检测实践 | `docs/practices/斜坡检测与移动实践总结.md` |
| 急停&动画过渡经验 | `docs/practices/急停与动画过渡经验总结.md` |

---

## 六、禁止事项

- ❌ 在 `Update()` / `LateUpdate()` / `FixedUpdate()` 里写 `await`
- ❌ 在 ScriptableObject 里引用场景中的 GameObject/Component
- ❌ 跨系统直接 `GetComponent<T>()` 或 `FindObjectOfType<T>()`（用 EventBus 代替）
- ❌ 用 uGUI (Canvas/Image/Text/Button) — 必须用 UI Toolkit
- ❌ 在 SO 的 `OnEnable()` / `OnValidate()` 里做运行时逻辑
- ❌ 用 `Resources.Load` 在 Update 里频繁调用（缓存结果）
- ❌ `Debug.Log` 在生产代码里乱打（用条件编译或封装日志系统）
- ❌ 修改 `TaskManager` 命名空间的名字（历史遗留，但涉及面太广）

---

## 七、MCPForUnity 工作流提示

当使用 AI + MCPForUnity 开发时：
1. **创建脚本后立即检查编译** — 用 `refresh_unity` + `read_console`
2. **批量操作用 `batch_execute`** — 减少往返，提高效率
3. **涉及场景操作前先读 `manage_scene get_hierarchy`** — 确认当前场景结构
4. **创建 SO 后要 `refresh_unity`** — 否则 CreateAssetMenu 可能不生效
5. **查找已有资源用 `manage_asset search`** — 避免重复创建

---

## 八、已知坑位 & 经验教训

### 系统级
1. **任务完成状态存储不一致** — Yarn 里的 quest 完成/接受状态必须与 QuestViewModel 同步。修复方式：在 Yarn 结束节点同时设置 `QuestCompleted`/`QuestAccepted` 并调用 `CompleteQuest`。
2. **Yarn 里引用的 Item ID 必须和 `GameConfigs/PackageModel/` 下的 SO 文件名一致**。
3. **EventBus 订阅必须在 `OnDisable` 里取消**，否则会产生悬挂引用。
4. **UI Toolkit 的元素查询失败不报错**，`_root.Q<T>("name")` 返回 null 时静默失败，务必判空。

### 动画 & 移动
5. **HSM 不要直接驱动 Animator 状态切换** — 让 Animator 自己管理 Transition（Exit Time / Speed 条件），HSM 用 `GetCurrentAnimatorStateInfo(0).IsName()` 轮询同步。
6. **FBX 动画导入时检查 Root Transform 设置** — Position(Y/XZ) Based Upon 建议用 Original，否则不同 pose 的质心位置不一致会导致 Idle/Stop 混搭时根节点跳动。详见 `docs/practices/急停与动画过渡经验总结.md`。
7. **急停时不要在代码里直接设 velocity=0** — 会导致脚部 IK 跳变，应通过 Animator Transition + deceleration 曲线平滑过渡。
8. **接地检测不能只靠单帧射线** — 坡道边缘容易闪断，需要多帧平滑或 buffer。详见 `docs/practices/斜坡检测与移动实践总结.md`。
9. **斜坡移动方向必须沿坡面切线** — 不能直接用世界 XZ 平面投影，否则上坡会卡顿/抖动。
10. **不要双重注入 Y 轴位移** — 同时在代码和物理里修改 Y 会导致角色逐渐离开坡面。

### 对话系统
11. **MVP 架构中 View 是被动的** — View 只暴露 SetText/ShowOptions 等接口，不包含"下一步该干什么"的逻辑。View 通过 Event/回调把玩家操作透传给 Presenter。
12. **Presenter 与 View 的绑定用 Awake + GetComponent** — 不要用 SerializeField 拖拽，容易漏拖/拖错。
13. **`DialogueRunner.dialogueViews` 引用可能在场景变动中丢失** — 症状：`IsDialogueRunning=true`、`LinesAvailable=True`、`Running node Start`，但没有 `RunLine` 日志，UI 不显示。根因：场景序列化中 `dialogueViews` 变成了 `{fileID: 0}`，Yarn 运行时没有视图接收台词。修复：在场景中把该引用重新绑回 `YarnDialogueAdapter` 组件。排查链路：E 键 → StartDialogue → LinesAvailable → RunLine → HandleNodeChanged → ShowDialogue，逐层加日志定位断点。详见 `docs/changelog/2026-06-12_DialogueRunnerViewsLost.md`。

### 存档系统
14. **存档机制 ≠ 存档闭环** — 序列化/反序列化方法写好后，必须明确指定 Save 和 Load 的触发时机。缺失触发点（启动读档、退出存档、剧情节点存档、读档后 UI 刷新），存档形同虚设。
15. **自动读档放在 `Start()` 而非 `Awake()`** — `Start()` 在所有场景对象 `Awake()` 后执行，此时 `FindObjectOfType` 可找到 PlayerStatsProvider、InventoryViewModel 等依赖。
16. **`RuntimeInitializeOnLoadMethod` 只解决单例创建** — 不能替代「何时读档」的设计。懒加载 + 自动初始化负责 GameObject 存在性，Start() 负责数据恢复时序。
17. **已完成任务不应存入存档** — 只保存 `!isCompleted` 的任务。已完成任务存入会导致下次读档后 Yarn `<<AcceptQuest>>` 重新创建实例，`activeQuests` 逐次膨胀，任务面板永远显示旧状态。修复：`CollectSaveData` 加 `if (quest.isCompleted) continue;`。
18. **Yarn 变量必须与任务数据一起持久化** — 任务数据恢复 ≠ 对话状态恢复。Yarn 根据 `$QuestCompleted`、`$InvestigationProgress` 等变量决定对话分支，不恢复这些变量会导致对话退回到最初状态。修复：Save/Load 时从 `InMemoryVariableStorage` 读写 Yarn 变量。
19. **`InMemoryVariableStorage.TryGetValue<T>()` 类型不匹配时抛异常** — 不能用于运行时类型探测（先试 bool 再试 float 会炸）。必须预先知道每个变量的类型，用 `Dictionary<string, bool>` 显式声明，新增变量时追加定义即可。
20. **Collect 事件匹配失败时用 `AdvanceQuestObjective` 兜底** — 当 Quest SO 的 objective.targetId 与 Yarn `<<GivePlayerItem>>` 的 itemId 不一致时，Collect 事件无法推进任务目标。可在 QuestManager 中新增 `AdvanceQuestObjective(questId)` 直接标记当前目标完成并推进索引，通过 Yarn 命令 `<<AdvanceQuestObjective questId>>` 调用。详见 `docs/changelog/2026-06-12_QuestSaveYarnFixes.md`。

---

## 九、自动化记录规则（强制执行）

当 AI 完成以下任何操作时，**主动**更新对应文档，无需等待用户指令：

| 触发条件 | 自动操作 | 目标位置 |
|---|---|---|
| 修复了一个 Bug | 追加根因 + 修复方式 | `docs/changelog/YYYY-MM-DD_<简述>.md`，同时提炼到本文第八节 |
| 新增/修改了架构或设计模式 | 更新或新建说明文档 | `docs/architecture/` 对应文件 |
| 发现了新的禁止事项或常见踩坑 | 追加到本文第八节，按子类别分组 | `AGENTS.md` 第八节 |
| 完成了有意义的配置/数据变更 | 记录变更摘要 | `docs/changelog/YYYY-MM-DD_<简述>.md` |
| 遇到并解决了一个非显而易见的工程问题 | 追加到 `/memories/repo/` | `/memories/repo/<主题>.md` |

**记录规范：**
- changelog 命名格式：`YYYY-MM-DD_<简短英文或中文描述>.md`
- AGENTS.md 第八节经验教训按子类别（系统级 / 动画&移动 / 对话系统 / …）分组追加
- 每次记录完成后，在对话末尾简要告知用户「已自动记录到 xxx.md」
- **不要为了记录而记录** — 只有真正值得留存的知识才记录。简单拼写修复、格式化、一句注释等不需要记录
