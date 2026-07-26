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
- **依赖方向**：`View → ViewModel → Model`；反向只靠事件/绑定通知。View **禁止**直接订阅 Model 事件（如 `viewModel.inventoryModel.OnXxx`），应订阅 ViewModel 对外暴露的事件（如 `OnDisplayChanged`）

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
- ❌ 直接调用 `Debug.Log` / `Debug.LogWarning` / `Debug.LogError` — 必须用 `RPGLog.Debug/Warning/Error("Channel", msg)`
- ❌ 在 `RPGLog.Debug` 消息里手写 `[Tag]` 前缀 — RPGLog 自动按频道追加 `[Channel]`
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

### 日志系统
1. **所有调试输出必须通过 RPGLog** — `RPGLog.Debug("Channel", msg)`，禁止裸调 `Debug.Log`。参见 `Assets/Scripts/Utils/RPGLog.cs`。
2. **频道命名与系统一一对应** — `Combat` / `Save` / `Quest` / `Dialogue` / `HSM` / `Player` / `Inventory` / `UI`。勿自创频道。
3. **发布版自动剥离 Debug 级日志** — 用 `[Conditional("UNITY_EDITOR")]` / `[Conditional("DEVELOPMENT_BUILD")]` 双特性，编译器连调用点和字符串插值一起剥离，零 GC 零开销。Warning 和 Error 始终保留。
4. **频道开关通过 RPGLogSettings SO 配置** — 运行时由 SaveSystem.Start() 自动加载 `Resources/GameConfigs/RPGLogSettings.asset`。

### 系统级
1. **任务完成状态存储不一致** — Yarn 里的 quest 完成/接受状态必须与 QuestViewModel 同步。修复方式：在 Yarn 结束节点同时设置 `QuestCompleted`/`QuestAccepted` 并调用 `CompleteQuest`。
2. **Yarn 里引用的 Item ID 必须和 `GameConfigs/PackageModel/` 下的 SO 文件名一致**。
3. **EventBus 订阅必须在 `OnDisable` 里取消**，否则会产生悬挂引用。
4. **UI Toolkit 的元素查询失败不报错**，`_root.Q<T>("name")` 返回 null 时静默失败，务必判空。
5. **攻击检测中遍历 Collider 必须按角色去重** — `Physics.OverlapSphere` / `SphereCastAll` 返回的是所有碰撞体，一个角色身上可能有多个 Collider（CharacterController + 子碰撞体）。必须用 `HashSet<PlayerStateDriver>` 对已命中的 Driver 去重，否则一次攻击触发多次伤害/跳字。
6. **DOTween 操作 `Time.timeScale` 必须用 `SetUpdate(true)`** — 帧冻结（Hit Stop）场景中 timeScale 被压低，如果 DOTween 自身也用 scaled time，tween 会被冻结无法完成恢复。`SetUpdate(true)` 强制使用 unscaled time。连段攻击时记得先 `_freezeTween?.Kill()` 再创建新 tween，避免多个 tween 竞争 timeScale。
7. **外部创建 .cs 文件后必须在 Unity 中 Reimport** — 通过 VS Code 或脚本创建的 .cs 文件，Unity 不会自动识别。手动编写 .meta 文件的 GUID 格式与 Unity 不兼容，会导致 `CS0246: 类型名未找到`。正确做法：创建 .cs 后删除手写 .meta，在 Unity Project 窗口中右键该文件 → Reimport，让 Unity 自动生成 GUID 和更新 Assembly-CSharp.csproj。
8. **MVVM 中 View 不得直接订阅 Model** — `InventoryUIController` 曾写 `viewModel.inventoryModel.OnInventoryChanged += RefreshUI`，导致 View 感知 Model。正确做法：ViewModel 暴露 `OnDisplayChanged`，在刷新显示缓存后广播；View 只订阅 ViewModel。详见 `docs/changelog/2026-07-26_InventoryMVVMDisplayChanged.md`。

### UI Toolkit
5. **`.Q<>()` 查不到 UXML 根元素自身** — `.Q()` 只搜索子元素。要获取 UXML 的根 `VisualElement`，直接用 `_root = _uiDoc.rootVisualElement`，不要用 `.Q<VisualElement>("root-name")`。
6. **世界坐标→面板坐标必须用原生 API** — `Camera.WorldToScreenPoint` 返回屏幕像素坐标，但 UI Toolkit 的 PanelSettings 有独立的参考分辨率和缩放模式，两者坐标系不一致。正确做法：`RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, worldPos, cam)`。
7. **跳字/头顶 UI 需要 Y 轴偏移** — 传入的世界坐标通常是 `transform.position`（脚底），需加 `_headHeight` 偏移到头顶。
8. **透视相机下不要对世界坐标做 Z 轴随机散布** — Z 轴偏移会显著改变屏幕投影位置，导致 UI 偏离目标。只保留 X 轴微小散布即可。
9. **USS 样式可能不加载，UI 元素需内联兜底** — `AddToClassList("damage-text")` 后如果 USS 路径不对或未加载，Label 可能完全不可见。在代码中设置 `style.color`、`style.fontSize` 等内联样式作为保底。

### 动画 & 移动
5. **HSM 不要直接驱动 Animator 状态切换** — 让 Animator 自己管理 Transition（Exit Time / Speed 条件），HSM 用 `GetCurrentAnimatorStateInfo(0).IsName()` 轮询同步。
6. **FBX 动画导入时检查 Root Transform 设置** — Position(Y/XZ) Based Upon 建议用 Original，否则不同 pose 的质心位置不一致会导致 Idle/Stop 混搭时根节点跳动。详见 `docs/practices/急停与动画过渡经验总结.md`。
7. **急停时不要在代码里直接设 velocity=0** — 会导致脚部 IK 跳变，应通过 Animator Transition + deceleration 曲线平滑过渡。
8. **接地检测不能只靠单帧射线** — 坡道边缘容易闪断，需要多帧平滑或 buffer。详见 `docs/practices/斜坡检测与移动实践总结.md`。
9. **斜坡移动方向必须沿坡面切线** — 不能直接用世界 XZ 平面投影，否则上坡会卡顿/抖动。
10. **不要双重注入 Y 轴位移** — 同时在代码和物理里修改 Y 会导致角色逐渐离开坡面。

### 移动端适配
1. **`IIntentProvider` 替换必须注意 Awake/Start 顺序** — `PlayerStateDriver.Awake()` 中初始化 intentProvider 时读取 `intentProviderOverride`。若 Bridge 在 `Start()` 才设置 override，为时已晚，HSM 会一直使用默认 `PlayerInputProvider`。修复：Bridge 在 `Awake()` 设置 override，`InitializeIntentProvider()` 移至 `Start()`。
2. **UI Toolkit Y 轴与游戏坐标系相反** — UI Toolkit 原点在左上角（Y↓），游戏 Y+ 为前方。摇杆计算游戏方向时必须翻转 Y 分量，视觉偏移保持原始坐标系。
3. **摇杆线性映射走/跑速度** — 设置 `ctx.runHeld = true` 使基准速度为 `runReal`，由摇杆位移量 `inputMagnitude` 线性缩放。轻推=慢走，推满=全速跑，Animator Speed 从 0 平滑到 1.0。
4. **移动端对话用 EventBus 解耦** — NPCInteractable 触发 `OnNPCInteractAvailable/Unavailable` → MobileInputBridge 显示/隐藏对话按钮 → 点击回传 `TriggerNPCInteract`。两端互不引用。
5. **移动端相机死区用半屏方案，不要用手指追踪** — `OnLook(InputValue)` 中 `<Pointer>/delta` 会合并所有触摸 delta，无法区分左右手。正确做法：触摸相机从 Input System 剥离，在 `Update()` 用 `EnhancedTouch` API 直读 `Touch.activeTouches`，按 `screenPosition.x > Screen.width * 0.45f` 过滤右半屏。鼠标走 `OnLook`，触摸走 `Update`，两条独立路径永不冲突。
6. **UI Toolkit `picking-mode="Ignore"` 会阻断子元素事件** — 根元素设 `Ignore` 会导致所有按钮/摇杆无法响应。用 USS `pointer-events: none`（仅阻止根自身）+ 子元素 `pointer-events: auto` 实现"空白区穿透、控件可交互"。需配合场景中的 `EventSystem` + `InputSystemUIInputModule` 组件。
7. **新输入系统下 UI Toolkit 必须配 EventSystem** — "仅新输入系统"模式下 UI Toolkit 需要场景中有 `EventSystem` + `InputSystemUIInputModule`，否则所有按钮点击静默失效。
8. **移动端中文字体需用 `Window→Text→FontAssetCreator` 创建 SDF 字体（非 TMP）** — `Window→TextMeshPro→FontAssetCreator` 生成的 TMP_FontAsset UI Toolkit 不支持。正确做法：`Window→Text→FontAssetCreator`，Character Set 选 `Custom Characters`，贴入精确字集，Atlas 512×512 Padding 5。通过 `PanelSettings→Text Settings→DefaultFontAsset` 配置，不要用 `Resources.Load<FontAsset>` 或 USS `resource()`。

### 摄像机
1. **`CinemachineImpulseListener` 必须放在 VCam 上，不能放在 Main Camera** — Cinemachine 2.x 的 `CinemachineImpulseListener` 继承自 `CinemachineExtension`，后者要求同 GameObject 存在 `CinemachineVirtualCameraBase`。`CinemachineBrain`（挂在 Main Camera 上）不继承该类，放在 Main Camera 上会报 `CinemachineExtension requires a Cinemachine Virtual Camera component`。正确做法：将 Listener 放在 `CinemachineVirtualCamera` 所在 GameObject。
2. **不要用 DOTween `DOShakePosition()` 配合 Cinemachine** — DOTween 直接写 Camera transform，与 `CinemachineBrain` 的 `LateUpdate` 输出冲突。震屏应使用 `CinemachineImpulseSource` + `CinemachineImpulseListener`。

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

### IL2CPP & 热更新
21. **AI Graph 包的 `System.Windows.Forms.dll` 和 `Ookii.Dialogs.dll` 必须排除 Standalone 平台** — 这两个 DLL 在 `Packages/cn.tuanjie.ai.graph/Runtime/EditorUtilities/Plugins/` 下，.meta 中默认 Standalone Win/Win64 设为 enabled。HybridCLR 的 `StripAOTDll`（内部触发 IL2CPP BuildPlayer）会因为 UnityLinker 无法解析 `Mono.Posix` 而失败。修复：将 .meta 中 `Standalone: Win` 和 `Standalone: Win64` 的 `enabled` 改为 `0`；同时 `StandaloneFileBrowserWindows.cs` 和 `StandaloneFileBrowser.cs` 需包裹 `#if UNITY_EDITOR`。
22. **HybridCLR Settings 中 `hotUpdateAssemblyDefinitions` 和 `hotUpdateAssemblies` 不能同时设置** — 用 asmdef 引用就用前者，用字符串名就用后者。两者都设会导致 `BuildFailedException: hot update assembly:HotUpdate is duplicated`。
23. **热更 DLL 中修改代码默认值不会覆盖 prefab 已序列化的值** — `public string Message = "新值"` 改了之后重编 DLL，运行时 Prefab 上序列化的旧值仍然生效。纯代码热更验证应修改非序列化逻辑（颜色、字号等）。
24. **Addressables 的 Address Key 必须与 `LoadAssetAsync(key)` 完全一致** — 大小写、拼写都要匹配，否则 `InvalidKeyException: No Location found for Key=xxx`。Group 中的 Address 列就是运行时加载用的 key。
25. **Addressables 句柄和 Instantiate 的对象必须在 OnDestroy 释放** — `LoadAssetAsync` 返回的 handle 要用 `Addressables.Release(handle)` 释放；`Instantiate` 出的 GameObject 要用 `Destroy()` 清理。否则场景关闭时报警告。

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
| 更新了 AGENTS.md / docs 下的 .md 文件 | commit + push 到远程仓库 | `git add -A && git commit && git push` |

**记录规范：**
- changelog 命名格式：`YYYY-MM-DD_<简短英文或中文描述>.md`
- AGENTS.md 第八节经验教训按子类别（系统级 / 动画&移动 / 对话系统 / …）分组追加
- 每次记录完成后，在对话末尾简要告知用户「已自动记录到 xxx.md」
- **不要为了记录而记录** — 只有真正值得留存的知识才记录。简单拼写修复、格式化、一句注释等不需要记录
