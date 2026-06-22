# 对话系统打字机效果 — 实施报告 (2026-06-22)

## 概述

为对话系统新增逐字显示（打字机）效果，使用 DOTween 驱动 UI Toolkit Label 的文本逐字渲染。支持点击跳过动画，符合 RPG 对话交互惯例。

## 新增文件

| 文件 | 说明 |
|------|------|
| `Assets/Scripts/DialogueSystem/View/TypewriterEffect.cs` | 打字机核心组件 |

## 修改文件

| 文件 | 变更 |
|------|------|
| `Assets/Scripts/DialogueSystem/View/DialogueUIController.cs` | 集成 TypewriterEffect，修改文本设置、点击交互、继续指示器逻辑 |
| `Assets/Scripts/DialogueSystem/Presenter/DialoguePresenter.cs` | 移除 `ShowContinueIndicator` 调用（改由 View 根据打字机完成状态自行管理） |

## 架构决策

### 打字机放在 View 层
打字机是纯视觉展示效果，属于 View 职责。Presenter 不感知动画进度，仅像以前一样调用 `view.SetDialogueText(text)`。View 内部启动打字机动画，完成后通过内部回调决定是否显示继续指示器。

### DOTween 方案：`DOTween.To()` 单 Tween
- 用 `DOTween.To(float, float, duration)` 驱动一个 0 → 可见字符总数的插值
- 每帧 `OnUpdate` 中将当前进度映射为字符串截取并设置 `Label.text`
- 比循环 `DOVirtual.DelayedCall` 性能更好，`Skip()` 只需一次 `Kill()`
- `SetUpdate(true)` 使用 unscaled time，与 `FrameFreezeController` 惯例一致

### 富文本标签感知
预计算 `_visibleCharMap[]`：遍历原字符串，跳过 `<tag>` 内字符，建立「第 N 个可见字符 → 原字符串索引」映射表。当前 Yarn 台词无富文本，但预建映射表防止未来问题。

### 防重入保护
- `_lastSetText` 缓存文本：Yarn 的 `RunLine → RunOptions` 流程会触发两次 `OnYarnNodeReady`（同文本 + 新增选项），第二次调用 `SetDialogueText` 时检测到同文本且打字机播放中则跳过。
- `ShowContinueIndicator(true)` 在打字机播放期间被静默忽略。
- `ShowOptions(true)` 自动隐藏继续指示器。

## 交互流程

```
打字机播放中点击文本 → Skip() → 全文显示 + 继续指示器出现
全文显示后点击文本 → OnDialogueTextClicked → Presenter.ContinueDialogue()
```

## 配置

- `_charsPerSecond`（`[SerializeField]`）：默认 30 字符/秒，可在 Inspector 调整

## 已知限制

- 不支持逐字音效回调（可后续扩展 `onCharRevealed` 事件）
- 不支持按住加速显示（可后续扩展）
