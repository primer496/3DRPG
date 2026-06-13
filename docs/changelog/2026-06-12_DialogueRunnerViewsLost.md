# DialogueRunner.dialogueViews 丢失导致对话 UI 不显示

**日期**: 2026-06-12  
**类型**: Bug 修复  
**影响系统**: 对话系统  
**严重程度**: 高（对话 UI 完全不显示）

---

## 症状

- 按 E 键走入 NPC 交互范围后：
  - ✅ `dialogueRunner.StartDialogue("Start")` 被调用
  - ✅ `dialogueRunner.IsDialogueRunning == true`
  - ✅ `dialogueRunner.lineProvider?.LinesAvailable == true`
  - ✅ Unity Console 输出 `Running node Start`
  - ❌ `[YarnDialogueAdapter] RunLine 收到台词: ...` 日志**未出现**
  - ❌ `[DialoguePresenter] 显示对话: ...` 日志**未出现**
  - ❌ 对话 UI 不显示

## 根因

场景文件 `SampleScene.unity` 中 `DialogueRunner` 组件的 `dialogueViews` 数组为**空**：

```yaml
# 场景中 DialogueRunner 的序列化数据 (修复前)
dialogueViews:
- {fileID: 0}           # ← 空引用，没有指向任何 DialogueViewBase
```

`DialogueRunner` 启动节点后，因为 `dialogueViews` 为空，**没有任何 `DialogueViewBase` 接收台词**。`RunLine`/`DialogueStarted` 等回调永远不被调用，导致整个对话链路在 Yarn 运行时内部静默断开。

**这不是代码逻辑错误，是场景序列化引用丢失。**

## 修复

将 `dialogueViews` 的第 0 项从 `{fileID: 0}` 恢复为场景中 `YarnDialogueAdapter` 组件的 fileID：

```yaml
# 修复后
dialogueViews:
- {fileID: 1607684144}   # ← 指向 YarnDialogueAdapter 组件
```

## 经验教训

1. **`IsDialogueRunning == true` 不等于对话正常工作** — 它只说明 `DialogueRunner` 进入了运行态，不代表台词已经分发到 View。
2. **排查对话 UI 不显示时，按链路逐层加日志**：
   - `NPCInteractable.Update` 确认 E 键触发
   - `DialogueRunner.StartDialogue` 确认启动
   - `lineProvider.LinesAvailable` 排除协程等待
   - `YarnDialogueAdapter.RunLine` 确认台词到达 Adapter
   - `DialoguePresenter.HandleNodeChanged` 确认 Presenter 收到数据
   - `DialogueUIController.ShowDialogue` 确认 UI 层被调用
3. **场景序列化引用（`dialogueViews`、`lineProvider`、`variableStorage`）是 Yarn Spinner 工作链的薄弱环节** — 任何场景重组织、脚本 GUID 变动、prefab 重导入都可能导致引用断裂。
4. **在场景变动后，优先检查 `DialogueRunner` 的 `dialogueViews` 是否仍指向有效组件**。

## 相关文件

- `Assets/Scenes/SampleScene.unity` — 场景序列化引用
- `Assets/Scripts/DialogueSystem/Model/YarnDialogueAdapter.cs` — 适配器
- `Assets/Scripts/DialogueSystem/Presenter/DialoguePresenter.cs` — 表现层
- `Assets/Scripts/DialogueSystem/View/DialogueUIController.cs` — UI 层
- `Assets/Scripts/QuestSystem/Interaction/NPCInteractable.cs` — E 键交互入口
