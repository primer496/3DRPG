# FinalRPG

Unity 2022.3+ URP 制作的 3D RPG 游戏 Demo，从零搭建全链路闭环的 RPG 核心体验。

**76 C# 脚本 · 7 大系统闭环 · 自定义 HSM 框架 · Yarn Spinner 对话 · Cinemachine 摄像机**

---

## 项目概述

FinalRPG 是一个完整的 3D RPG 演示项目，不依赖任何第三方游戏框架，所有核心系统均为自主实现。项目以"从零理解每个环节"为出发点，在实践中探索了状态机设计、系统间解耦通信、数据持久化、动画适配、战斗反馈等多个领域的工程问题。

你可以在游戏中：
- 移动/跳跃/翻越/攀爬/战斗/闪避/受击硬直
- 与 NPC 对话接取任务（多条分支剧情，每个选择影响后续走向）
- 击杀怪物获得经验/金币/物品
- 打开背包查看道具，查看任务面板追踪进度
- 存档退出，再次进入时恢复完整游戏状态

---

## 核心架构

### 分层状态机 (HSM)

项目自研了一套树形分层状态机框架 `Unity-HSM`，管理角色的全部行为。

```
                PlayerRoot
                /        \
          Grounded      Airborne
          /  |  \        /  \
       Idle Move Stop  Fall Land
              / | \
        Climb Vault Combat/HitReaction/Dodge
```

**设计要点：**

- **状态树与 LCA 路径寻路**：状态按父子关系组织成树，切换时自动计算最近公共祖先（LCA），仅退出需要退出的状态、仅进入需要进入的状态，公共状态不动
- **TransitionSequencer 异步三阶段切换**：退出活动 → 变更状态树 → 进入活动，每阶段拆为可异步的步骤。主循环用**轮询**（每帧 `Update()` 检查 Task 是否完成）推进，**不在 `Update()` 里写 `await`**，保证主循环不被阻塞
- **Activity 挂载机制**：每个状态可挂多个 `IActivity`，进入/离开时异步执行 Activate/Deactivate。支持 `ParallelPhase` 并行和 `SequentialPhase` 串行两种模式
- **15+ 玩家状态全覆盖**：Idle / Move / Stop / Airborne / Landing / Climb / Vault / Combat / HitReaction / Dodge，每个状态独立封装过渡逻辑

### EventBus 解耦通信

全局事件总线负责所有系统间的通信。背包、任务、战斗、存档等子系统完全通过事件交互，**零直接引用**，可插拔替换。

```
NPCInteractable → EventBus(Communicate) → QuestManager.HandleGameActivity
EnemyHealth.Die() → EventBus(Kill) → QuestManager + YarnVariableStorage
WeaponDetector → EventBus(Collect) → QuestManager + InventoryModel
```

### MVVM / MVP 分层

- **Model**：纯数据层，依托 ScriptableObject 配置 + 纯 C# 运行时数据类
- **ViewModel / Presenter**：桥接层，监听 Model 变化并驱动 UI 刷新
- **View**：UI Toolkit（UXML + USS）构建的被动视图，不包含业务逻辑

对话系统采用 MVP（Passive View）模式，View 只暴露 `SetCharacterName()` / `ShowOptions()` 等接口，通过 `UnityEvent` 将点击透传给 Presenter，完美适配 Yarn Spinner 引擎的注入。

---

## 核心系统

### 玩家控制系统

- 基于 HSM 的完整角色状态管理
- 输入经相机空间转换，支持 8 方向移动
- CharacterController + Rigidbody 两套方案均经过完整调试（见 `feature/cc-slope-vault` 和 `feature/rigidbody-slope` 分支）
- 斜坡检测（SphereCast + 接地缓冲 + 坡面切线投影）消除坡道闪断与卡顿
- 急停动画过渡（指数衰减速度 + Root Transform 统一 + 脚步事件同步 + HSM 轮询同步）
- Cinemachine 第三人称跟随相机，带碰撞检测与 Soft Zone 缓冲

### 敌人 AI 系统

- EnemyBrain 决策树驱动行为切换
- 可配置能力集（移动/战斗/跳跃/越障），行为可插拔组合
- IntentProvider 接口抽象行为意图，支持多种 AI 策略
- 扇形范围攻击检测（Physics.OverlapSphere），配合 HashSet 去重防多次命中

### 背包系统

- `ItemData` ScriptableObject 定义道具元数据（图标、类型、堆叠上限等）
- `InventoryModel` 纯 C# 类（非 MonoBehaviour），无 Unity 耦合
- 事件驱动增删逻辑，ViewModel 监听变化驱动 UI Toolkit 动态刷新
- 支持从 `/GameConfigs/PackageModel/` 动态图标加载

### 任务系统

- `QuestData` ScriptableObject 配置多目标任务（击杀/收集/对话）
- `QuestInstance` 运行时追踪每个目标的进度
- EventBus 事件自动推进匹配的目标进度
- 支持有序/无序目标完成方式
- 完整的接取→进行中→完成→奖励发放链路

### 对话系统（集成 Yarn Spinner）

- **MVP 架构 + YarnDialogueAdapter 适配器模式**：适配器继承 `DialogueViewBase`，伪装成 Yarn 的 UI 视图，将 Yarn 台词转化为 MVP 架构能听懂的 `DialogueNode`
- **7 个 `.yarn` 剧情脚本**：Start → Quest_Accepted → Progress_1/2 → Phase2_Waiting → DailyDialogueEnd，用 Yarn 变量实现对话状态机
- **8+ 自定义 Yarn 命令**：`<<AcceptQuest>>`、`<<CompleteQuest>>`、`<<GivePlayerItem>>`、`<<AdvanceQuestObjective>>`、`<<AutoSave>>` 等
- **打字机效果**：DOTween 驱动的逐字显示，富文本标签感知，点击跳过，`SetUpdate(true)` 不受 timeScale 影响
- Presenter 对底层引擎是 Yarn 还是自定义解析器**完全无感知**

### 存档系统

- **全量持久化**：PlayerStats + Inventory + Quest + Yarn 对话变量
- **触发链完整闭环**：启动自动读档 → 剧情节点 `<<AutoSave>>` → 退出自动存档
- **Yarn 变量类型安全序列化**：预声明类型字典（bool vs float），避免 `TryGetValue<T>()` 的 `InvalidCastException`
- 已完成任务过滤，防止数据污染读档

### 战斗反馈系统

- **帧冻结 (Hit Stop)**：DOTween `DOTween.To().From().SetUpdate(true)` 驱动 `Time.timeScale` 瞬时缩放，OutQuad 缓出恢复。仅玩家→敌人触发，避免受击时卡输入
- **镜头晃动 (Camera Shake)**：Cinemachine Impulse Source + Listener，从命中点指向相机的定向晃动，玩家命中轻晃（0.2），受击重晃（0.5）
- **伤害跳字 (Damage Floating Text)**：独立 UI Document 叠加层，对象池复用 Label，RuntimePanelUtils 坐标转换，上飘淡出动画

### HUD 系统

- HP/EXP/金币/等级实时显示
- UI Toolkit ProgressBar + Label，事件驱动刷新
- 半透明暗色面板叠加

### 移动端适配

- **虚拟摇杆**：纯 UI Toolkit 自研 `UIToolkitJoystick`，Pointer 事件驱动，滑块线性映射走/跑速度
- **攻击/闪避按钮**：右下角 UI 按钮，PointerDown 事件写入 `PlayerContext`
- **背包/任务入口**：右上角按钮，Click 事件直发 EventBus
- **对话入口**：靠近 NPC 显示名称按钮，EventBus 与 `NPCInteractable` 解耦
- **PC 回落**：`MobileInputBridge` 实现 `IIntentProvider`，包装 `PlayerInputProvider` 保证键鼠完整可用
- 见 [手游适配变更记录](docs/changelog/2026-06-29_MobileAdaptation.md)

---

## 关键技术难点与解决方案

### 1. 动画急停与混合适配

**问题**：走动/跑动突然松开方向键时，角色回 Idle 出现脚步抖动、根位置偏移。

**解决**：
- **Root Transform 统一基准**：Stop/Idle/Locomotion 动画片段统一使用"原始 (Original)"而非"质心 (Center of Mass)"作为根位置基准，消除混合时根位移
- **代码与 Animator 分工**：进入急停由代码 `CrossFade`，离开急停由 Animator Transition，HSM 轮询 `GetCurrentAnimatorStateInfo` 同步
- **Speed 指数衰减**：避免 SmoothDamp 极小时间常数突变
- **Foot Plant 脚步事件**：动画事件驱动 `StopFoot` 参数，确保急停姿势与当前步态一致

见 [急停与动画过渡经验总结](docs/practices/急停与动画过渡经验总结.md)

### 2. 斜坡检测与移动

**问题**：上坡卡住/抖动、斜坡边缘误判空中频繁触发 Landing、平移逐渐离开坡面。

**解决（Rigidbody 方案）**：
- 接地检测升级为 **SphereCast + CheckSphere fallback + 0.08s 缓冲** 三合一
- 地面运动改为 `velocity` 水平驱动，不做全量覆盖（保留 `v.y` 自然下落）
- 移动方向做 `Vector3.ProjectOnPlane(dir, groundNormal)` 坡面切线投影
- snapDown 仅在探测 miss 时触发，不在正常斜坡持续下压
- 起跳后短暂离地锁窗口防止起跳帧被重判接地

见 [斜坡检测与移动实践总结](docs/practices/斜坡检测与移动实践总结.md)

### 3. Yarn 变量类型安全持久化

**问题**：读档后 `InvalidCastException: Variable $QuestCompleted exists, but is the wrong type (expected System.Single, got System.Boolean)`

**根因**：Yarn 的 `InMemoryVariableStorage.TryGetValue<T>()` 类型不匹配时直接抛异常而非返回 false，无法实现 `if-try-bool-else-float` 的 fallback 模式。

**解决**：显式声明 `Dictionary<string, bool>` 类型映射表，每个变量标记为 bool 或 float，保存/恢复时直接用正确类型读写，彻底消除类型异常。

见 [任务/存档/Yarn变量三项连锁修复](docs/changelog/2026-06-12_QuestSaveYarnFixes.md)

### 4. 场景序列化引用丢失

**问题**：`DialogueRunner.dialogueViews` 数组在场景重组织后变为空引用，对话 UI 完全不显示——但 `IsDialogueRunning == true` 且 `LinesAvailable == true`，故障极具迷惑性。

**解决**：Yarn Spinner 的引用链（dialogueViews / lineProvider / variableStorage）是场景序列化的薄弱环节。建立按链路逐层日志排查的方法论，`IsDialogueRunning` 为真不等于台词已分发到 View。

见 [DialogueRunner.dialogueViews 丢失](docs/changelog/2026-06-12_DialogueRunnerViewsLost.md)

### 5. 存档系统的"最后一公里"

**问题**：序列化/反序列化机制完整实现，但无人调用——没有触发 Save/Load 的时机。

**解决**：在四个关键时机注入触发点：启动 `Start()` 读档 → Yarn `<<AutoSave>>` 命令 → 退出 `OnApplicationQuit()` 存档 → 读档后刷新任务 UI。`RuntimeInitializeOnLoadMethod` 只解决存在性，不能替代"何时执行"的架构决策。

见 [存档系统闭环](docs/changelog/2026-06-12_SaveSystemTriggerChain.md)

### 6. Cinemachine 与 UI Toolkit 技术踩坑

- **CinemachineImpulseListener 必须放在 VCam 上**而非 Main Camera（CinemachineBrain 不继承 CinemachineVirtualCameraBase）
- **UI Toolkit 坐标转换**：`Camera.WorldToScreenPoint` 不兼容 PanelSettings 参考分辨率，需用 `RuntimePanelUtils.CameraTransformWorldToPanel`
- **UXML 编码**：XML 注释不得包含非 ASCII 字符，Unity 解析报 `Invalid character in the given encoding`，需 UTF-8 without BOM

---

## 工程实践

- **全项目 ScriptableObject 驱动**：角色属性、道具、任务、能力配置均可在 Inspector 直接调参，运行时 `Resources.Load` 热加载
- **AGENTS.md 项目规范**：架构约定、命名空间、禁止事项、已知坑位知识库，支持 AI 协作开发的上下文注入
- **Python 自动化工具**：Excel 配置生成、角色 SO 批量创建、道具图标批处理、文生图资产生成
- **Git 分支策略**：`feature/rigidbody-slope`（Rigidbody 方案）与 `feature/cc-slope-vault`（CharacterController + 攀爬翻越）双分支保留，便于方案对比与增量修复

---

## 技术栈

| 类别 | 技术 |
|---|---|
| 引擎 | Unity 2022.3+ |
| 渲染管线 | URP |
| 状态机 | 自研 HSM (Hierarchical State Machine) |
| 对话引擎 | Yarn Spinner 2.x |
| 动画插值 | DOTween (Pro) |
| 摄像机 | Cinemachine |
| UI | UI Toolkit (UXML + USS) |
| 输入 | Unity Input System |
| 架构模式 | MVVM / MVP + EventBus |
| 配置数据 | ScriptableObject |
| 语言 | C# + Yarn (.yarn) |

---

## 项目结构

```
FinalRPG/
├── Assets/
│   ├── Scripts/
│   │   ├── Unity-HSM/              # 状态机框架 + 玩家/敌人行为
│   │   │   ├── Core/               # StateMachine, State, TransitionSequencer
│   │   │   ├── States/             # 15+ 玩家/敌人状态类
│   │   │   ├── Activities/         # 可挂载的异步活动
│   │   │   ├── AttackDetector/     # 武器/敌人攻击检测
│   │   │   └── Sequences/          # ParallelPhase, SequentialPhase
│   │   ├── DialogueSystem/         # 对话系统 (MVP + YarnAdapter)
│   │   │   ├── Model/              # DialogueModel, YarnDialogueAdapter
│   │   │   ├── Presenter/          # DialoguePresenter
│   │   │   └── View/               # DialogueUIController, TypewriterEffect
│   │   ├── QuestSystem/            # 任务系统
│   │   │   ├── ViewModel/          # QuestViewModel, QuestBindableData
│   │   │   ├── View/               # QuestUIController
│   │   │   └── Interaction/        # NPCInteractable
│   │   ├── InventorySystem/        # 背包系统 (MVVM)
│   │   ├── SaveSystem/             # 存档系统
│   │   ├── Player/                 # 玩家属性 + HUD
│   │   ├── Camera/                 # 第三人称相机 + 震屏
│   │   ├── Combat/                 # 帧冻结控制器
│   │   └── UI/                     # 移动端输入桥接 + 伤害跳字
│   ├── UIToolKit/                  # UI Toolkit 资源 (UXML/USS)
│   │   └── Mobile/                 # 移动端屏幕控件
│   ├── Resources/GameConfigs/      # 游戏配置 (角色/道具/任务/对话)
│   ├── Scenes/                     # 场景文件
│   └── Shader/                     # URP Shader
├── docs/                           # 技术文档
│   ├── architecture/               # 架构说明
│   ├── practices/                  # 实践经验
│   ├── changelog/                  # 变更记录
│   └── archive/                    # 归档
├── Packages/                       # Unity 包管理
└── ProjectSettings/                # Unity 项目设置
```

---

## 快速开始

1. 用 Unity Hub 打开项目文件夹，使用 `ProjectSettings/ProjectVersion.txt` 中指定的 Unity 版本
2. 打开 `Assets/Scenes/SampleScene.unity`
3. 运行游戏：A/D 移动，空格跳跃，E 与 NPC 对话，鼠标左键攻击

---

## 文档索引

| 文档 | 说明 |
|---|---|
| [项目代码结构说明](docs/architecture/项目代码结构说明.md) | HSM 框架与主循环的完整解析 |
| [对话任务系统说明](docs/architecture/Dialogue_Quest_System_Explanation.md) | 对话与任务全链路数据流 |
| [急停与动画过渡](docs/practices/急停与动画过渡经验总结.md) | 动画混合问题排查与修复经验 |
| [斜坡检测与移动](docs/practices/斜坡检测与移动实践总结.md) | Rigidbody 驱动下的斜坡稳定性方案 |
| [属性系统与背包闭环](docs/changelog/RPG属性系统与背包任务闭环_实施报告.md) | 打通战斗→奖励→背包→任务的完整链路 |
| [存档系统闭环](docs/changelog/2026-06-12_SaveSystemTriggerChain.md) | 存档触发时机设计与 Yarn 变量持久化 |
| [战斗反馈系统](docs/changelog/2026-06-22_HitFeedback.md) | 帧冻结 + 镜头晃动实现打击感 |
| [伤害跳字系统](docs/changelog/2026-06-21_DamageFloatingText.md) | UI Toolkit 对象池跳字方案 |
| [打字机效果](docs/changelog/2026-06-22_TypewriterEffect.md) | DOTween 驱动的逐字显示 |

---

## 分支说明

- `main`：主开发分支，当前使用 CharacterController + HSM 全状态
- `feature/rigidbody-slope`：Rigidbody 控制器方案，斜坡处理完善，但翻越未实现
- `feature/cc-slope-vault`：CharacterController 方案，支持斜坡与翻越

两个特性分支保留用于方案对比和增量修复。
