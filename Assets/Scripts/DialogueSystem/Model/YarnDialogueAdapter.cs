using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using DialogueSystem.Model;

namespace DialogueSystem.Adapter
{
    /// <summary>
    /// 这就是 MVP 和 Yarn 之间的"适配器/桥梁"。
    /// 它对 Yarn 宣称自己是一个合法的 View，但实际上它并不绘制任何UI图像。
    /// 它只负责截获 Yarn 抛出的台词和选项数据，组装成 MVP 需要的 DialogueNode 并向上抛出事件。
    /// </summary>
    public class YarnDialogueAdapter : DialogueViewBase
    {
        // 供 MVP (Presenter) 监听的事件
        public Action<DialogueNode> OnYarnNodeReady;
        public Action OnYarnDialogueEnded;

        // 缓存用于向 Yarn 汇报进度的委托
        private Action advanceHandler;
        private Action<int> optionHandler;

        // 缓存当前正在播放的数据节点
        private DialogueNode currentNode;

        /// <summary>
        /// 当 Yarn 读取到一句普通台词时调用
        /// </summary>
        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // [Debug] Debug.Log($"[YarnDialogueAdapter] RunLine 收到台词: {dialogueLine.TextWithoutCharacterName.Text}");
            currentNode = new DialogueNode
            {
                Id = dialogueLine.TextID.GetHashCode(), // Fake ID 模拟原始结构
                SpeakerName = dialogueLine.CharacterName,
                Text = dialogueLine.TextWithoutCharacterName.Text,
                Options = new List<DialogueSystem.Model.DialogueOption>()
            };

            // 缓存通知 Yarn "这句放完了" 的方法
            advanceHandler = onDialogueLineFinished;

            // 组装成 MVP 能听懂的 Node 后抛出给 Presenter
            if (OnYarnNodeReady != null)
            {
                OnYarnNodeReady.Invoke(currentNode);
            }
            else
            {
                // 为了防止卡死，强行继续
                onDialogueLineFinished?.Invoke();
            }
        }

        /// <summary>
        /// 当 Yarn 读取到需要玩家选择的分支选项时调用
        /// </summary>
        public override void RunOptions(Yarn.Unity.DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            if (currentNode == null) currentNode = new DialogueNode();

            currentNode.Options.Clear();

            // 将 Yarn 格式的选项转为我们的 DialogueOption 格式
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                var opt = dialogueOptions[i];
                currentNode.Options.Add(new DialogueSystem.Model.DialogueOption
                {
                    Text = opt.Line.Text.Text,
                    NextNodeId = opt.DialogueOptionID // 使用 Yarn 的内部选项 ID
                });
            }

            // 缓存通知 Yarn "玩家选了第几个选项" 的方法
            optionHandler = onOptionSelected;

            // 选项准备好后，带着前面的台词再次抛给 Presenter 刷新UI
            if (OnYarnNodeReady != null)
            {
                OnYarnNodeReady.Invoke(currentNode);
            }
        }

        /// <summary>
        /// 当整个 Yarn 对话树执行到结束时调用
        /// </summary>
        public override void DialogueComplete()
        {
            currentNode = null;
            advanceHandler = null;
            optionHandler = null;
            OnYarnDialogueEnded?.Invoke();
        }

        /// <summary>
        /// 暴露给 MVP (Presenter)：当玩家在UI上点击“继续”时调用
        /// </summary>
        public override void UserRequestedViewAdvancement()
        {
            if (advanceHandler != null)
            {
                // 取出并立刻置空，防止 Yarn 内部逻辑错误导致的二次触发
                var handler = advanceHandler;
                advanceHandler = null;
                handler.Invoke(); // 告诉 Yarn 继续往下读
            }
        }

        /// <summary>
        /// 暴露给 MVP (Presenter)：当玩家在UI上点击某个分支按钮时调用
        /// </summary>
        public void SelectOption(int index)
        {
            if (optionHandler != null)
            {
                var handler = optionHandler;
                optionHandler = null;
                handler.Invoke(index); // 告诉 Yarn 玩家选了哪个
            }
        }
    }
}
