using UnityEngine;
using DialogueSystem.Model;
using DialogueSystem.Adapter;
using HSM;
using FinalRPG.Utils;

namespace DialogueSystem.Presenter
{
    // Presenter层，它不应该挂在UI(View)物体上，而是作为全局管理器DialogueManager的一部分。
    // 它在运行时主动寻找/接收绑定的UI层(View)，从而彻底断开与UI物体的生命周期挂钩。
    public class DialoguePresenter : MonoBehaviour
    {
        [Header("UI 引用 (若没拖拽，会尝试全局寻找)")]
        [SerializeField] private DialogueUIController view;
        
        [Header("逻辑层引用 (拖拽 DialogueManager上的Adapter)")]
        [SerializeField] private YarnDialogueAdapter adapter;
        
        private DialogueModel model;

        private void Awake()
        {
            // 作为常驻大脑，它自己就在 DialogueManager 上，所以获取同物体的 adapter
            if (adapter == null) adapter = GetComponent<YarnDialogueAdapter>();
            // 全局寻找 UI，即使 UI 由于某些原因暂时未激活也可以找到（FindObjectsInactive.Include）
            if (view == null) view = FindFirstObjectByType<DialogueUIController>(FindObjectsInactive.Include);

            if (adapter != null)
            {
                model = new DialogueModel(adapter);

                model.OnNodeChanged += HandleNodeChanged;
                model.OnDialogueEnded += HandleDialogueEnded;
            }
            else
            {
                RPGLog.Error("Dialogue", "找不到 YarnDialogueAdapter！");
            }
            
            // 将Presenter绑定到View上
            if (view != null)
            {
                view.BindPresenter(this);
            }
            else
            {
                RPGLog.Warning("Dialogue", "找不到 DialogueUIController！请确保场景中存在UI。");
            }
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                model.OnNodeChanged -= HandleNodeChanged;
                model.OnDialogueEnded -= HandleDialogueEnded;
            }
        }

        // 接收Model发来的数据变化事件并指示View进行显示
        private void HandleNodeChanged(DialogueNode node)
        {
            // 通过 EventBus 全局广播锁定输入，无需直接依赖 PlayerStateDriver
            TaskManager.EventBus.Instance.RaiseInputLock(true);

            // 如果UI在游戏过程中被销毁了或者没找到，直接返回
            if (view == null) { RPGLog.Error("Dialogue", "HandleNodeChanged: view 为 null！"); return; }

            // [Debug] Debug.Log($"[DialoguePresenter] 显示对话: {node.SpeakerName}: {node.Text}");
            view.ShowDialogue(true);
            view.SetCharacterName(node.SpeakerName);
            view.SetDialogueText(node.Text);

            bool hasOptions = node.Options != null && node.Options.Count > 0;
            view.ShowOptions(hasOptions);
            // 继续指示器由 View 根据打字机完成状态自行管理

            if (hasOptions)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < node.Options.Count)
                    {
                        view.SetOptionState(i, true, node.Options[i].Text);
                    }
                    else
                    {
                        view.SetOptionState(i, false, string.Empty);
                    }
                }
            }
        }

        private void HandleDialogueEnded()
        {
            // 通过 EventBus 全局广播解锁输入
            TaskManager.EventBus.Instance.RaiseInputLock(false);

            if (view != null)
            {
                view.ShowDialogue(false);
            }
        }

        public void SelectOption(int optionIndex)
        {
            model?.SelectOption(optionIndex);
        }

        public void ContinueDialogue()
        {
            model?.ContinueDialogue();
        }
    }
}
