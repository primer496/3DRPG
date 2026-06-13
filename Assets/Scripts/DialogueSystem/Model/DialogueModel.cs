using System;
using UnityEngine;
using DialogueSystem.Adapter;
//此时Model层的DialogueModel就像是一个中介者，负责监听YarnDialogueAdapter抛出的事件（如台词准备好、对话结束等），并将这些事件转发给Presenter层。同时它也提供接口供Presenter调用，以控制对话流程（如选择选项、继续对话等）。通过这种方式，Model层解耦了Yarn和Presenter，使得它们之间的交互更加清晰和灵活。
namespace DialogueSystem.Model
{
    public class DialogueModel
    {
        public Action<DialogueNode> OnNodeChanged;
        public Action OnDialogueEnded;

        private YarnDialogueAdapter yarnAdapter;

        public DialogueModel(YarnDialogueAdapter adapter)
        {
            yarnAdapter = adapter;
            if (yarnAdapter != null)
            {
                yarnAdapter.OnYarnNodeReady += HandleYarnNodeReady;
                yarnAdapter.OnYarnDialogueEnded += HandleYarnDialogueEnded;
            }
        }
        // 这里的析构函数是为了确保当 DialogueModel 被销毁时，能够正确地取消订阅 YarnDialogueAdapter 的事件，避免潜在的内存泄漏或异常。
        ~DialogueModel()
        {
            if (yarnAdapter != null)
            {
                yarnAdapter.OnYarnNodeReady -= HandleYarnNodeReady;
                yarnAdapter.OnYarnDialogueEnded -= HandleYarnDialogueEnded;
            }
        }

        private void HandleYarnNodeReady(DialogueNode node)
        {
            OnNodeChanged?.Invoke(node);
        }

        private void HandleYarnDialogueEnded()
        {
            OnDialogueEnded?.Invoke();
        }

        public void SelectOption(int optionIndex)
        {
            if (yarnAdapter != null)
            {
                yarnAdapter.SelectOption(optionIndex);
            }
        }

        public void ContinueDialogue()
        {
            if (yarnAdapter != null)
            {
                yarnAdapter.UserRequestedViewAdvancement();
            }
        }
    }
}
