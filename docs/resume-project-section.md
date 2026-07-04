# FinalRPG — 简历项目经历

> 可根据篇幅选用「完整版」或「压缩版」

---

## 完整版（推荐 · 12 条）

**FinalRPG · 2.5D/3D RPG 游戏 Demo · Unity 2022.3+ · URP · 独立开发**

**规模**：70+ C# 脚本 · 7 大系统闭环 · 15+ Python 自动化工具

### 架构设计
- 实现非阻塞多帧状态过渡系统：Activity 承载跨帧操作（动画等待、延迟激活），TransitionSequencer 编排退出→切换→进入三阶段，Task 轮询替代协程避免 GC，支持串行/并行执行与随时取消，过渡零帧阻塞
- 搭建 **EventBus 解耦架构**，所有系统间通信走事件总线，消除跨系统直接引用，子系统完全可插拔
- 统一采用 **MVVM/MVP 分层**：Model 纯数据 (ScriptableObject) → ViewModel/Presenter 桥接 → View (UI Toolkit) 被动渲染，UI 迭代无需改逻辑
- 全项目配置数据 **ScriptableObject 驱动**，策划可在 Inspector 直接调参，运行时 `Resources.Load` 热加载

### 核心系统
- **玩家控制**：15+ 状态全覆盖 (Idle/Move/Stop/Airborne/Landing/Climb/Vault/Combat/HitReaction/Dodge)，搭配 Cinemachine 第三人称相机
- **敌人 AI**：EnemyBrain 决策树 + IntentProvider + 可配置能力集 (移动/战斗/跳跃/越障)，行为可插拔组合
- **背包系统**：ItemData SO 定义道具元数据，事件驱动增删，UI Toolkit 动态图标加载
- **任务系统**：QuestData SO 配置多目标 (击杀/收集/对话)，Yarn Spinner 驱动剧情推进，EventBus 事件自动推进进度
- **对话系统**：MVP 架构 + YarnDialogueAdapter 适配器，5+ 分支剧情脚本，8+ 自定义 Yarn 命令 (接任务/交任务/给物品/自动存档)
- **存档系统**：PlayerStats + Inventory + Quest + Yarn 变量全量持久化，自动存档/读档触发链完整闭环，类型安全序列化

### 技术难点
- 解决动画混合异构 Rig 适配：统一 Root Transform 设置 + 指数减速曲线 + Foot Plant 事件同步，消除急停/切换时脚步抖动与浮空
- 解决斜坡检测与移动：多帧接地缓冲 + 坡面切线方向移动计算，消除坡道边缘闪断与卡顿
- 解决 Yarn 变量类型安全持久化：`TryGetValue<T>()` 无优雅降级，实现显式类型字典声明的序列化方案，避免读档后对话状态回退

### 工程效率
- 编写 15+ Python 自动化脚本：Excel 配置生成、角色 SO 批量创建、道具图标批处理、文生图资产生成，配置效率提升约 80%
- 制定并维护 AGENTS.md 项目规范：包含架构约定、命名空间、禁止事项、已知坑位知识库，支持 AI 协作开发的上下文注入与自动化记录

---

## 精简版（4 条 · 推荐）

**3D RPG Demo · Unity 2022.3+ URP · 独立开发 · 70+ C# 脚本**

- 基于 HSM（树形组织、Activity 异步过渡）实现 12 种玩家状态与敌人行为树决策，通过意图抽象层统一玩家输入与 AI 控制，SO 能力配置集支持行为可插拔组合
- 搭建 EventBus 解耦架构，基于 MVVM/MVP 分层完成背包、任务、对话等 7 大系统闭环，全项目 ScriptableObject 驱动、UI Toolkit 构建界面
- 集成 Yarn Spinner 对话引擎（5+ 分支剧情、8+ 自定义命令），Yarn 变量与任务状态类型安全同步持久化
- 实现全量数据存档闭环（PlayerStats / Inventory / Quest / Yarn 变量），攻克动画混合适配与 CC 接地/墙体检测等关键技术难点

---

## 压缩版（篇幅极紧张时使用 · 3 条）

> **3D RPG Demo · Unity 2022.3+ URP · 独立开发**
> - 基于 HSM 实现 12 种玩家状态与敌人行为树决策，搭建 EventBus 解耦架构与 MVVM/MVP 分层体系
> - 完成 7 大核心系统闭环：玩家控制、敌人 AI、背包、任务、对话（Yarn Spinner）、存档、摄像机
> - 攻克动画混合适配、CC 接地/墙体检测、Yarn 变量持久化等关键技术难点，实现全链路存档闭环

---

## 使用建议

| 场景 | 版本 |
|---|---|
| 简历「项目经历」板块 | **精简版**（4 条，架构/系统/难点全覆盖，符合国内行业简历规范） |
| 简历篇幅充裕 | **完整版**（12 条分 4 组，面试可展开聊细节） |
| 简历只剩 2-3 行 | **压缩版**（3 条浓缩） |
| 面试自我介绍 | 从精简版中挑 3 条口述，重点讲 HSM 动画过渡机制和存档闭环 |
| HR 一句话 | 「用 Unity 从零搭建 70+ 脚本的 3D RPG Demo，实现 HSM 状态机 + Yarn Spinner 对话 + 全链路存档闭环，7 大系统完整可玩」 |
