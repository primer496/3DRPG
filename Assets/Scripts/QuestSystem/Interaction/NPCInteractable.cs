using UnityEngine;
using Yarn.Unity;
using TaskManager;

namespace QuestSystem.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class NPCInteractable : MonoBehaviour
    {
        [Header("NPC 信息")]
        public string npcId = "VillageChief";
        [Tooltip("移动端靠近时显示的 NPC 名称")]
        public string npcDisplayName = "村长";

        [Header("Yarn 对话设置")]
        public string startNodeName = "Start";

        private DialogueRunner dialogueRunner;
        private bool isPlayerInRange = false;

        private void Start()
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            EventBus.Instance.Subscribe("TriggerNPCInteract", OnMobileInteract);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe("TriggerNPCInteract", OnMobileInteract);
        }

        private void Update()
        {
            // 当玩家在范围内，按下 E 键，且当前没有正在播放的对话时触发
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
                {
                    // [Debug] Debug.Log($"[NPCInteractable] LinesAvailable={dialogueRunner.lineProvider?.LinesAvailable}, YarnProject={dialogueRunner.yarnProject?.name}");
                    dialogueRunner.StartDialogue(startNodeName);

                    // 抛出沟通事件完成目标
                    EventBus.Instance.Raise(TargetType.Communicate, npcId, 1);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                EventBus.Instance.RaiseNPCInteractAvailable(npcDisplayName);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                EventBus.Instance.RaiseNPCInteractUnavailable();

                // 如果玩家被外力击退或某种原因脱离了触发器范围，强制中断对话以防Bug
                if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
                {
                    dialogueRunner.Stop();
                }
            }
        }
        /// <summary>移动端对话按钮点击时，由 EventBus "TriggerNPCInteract" 触发。</summary>
        private void OnMobileInteract()
        {
            if (!isPlayerInRange) return;
            if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.StartDialogue(startNodeName);
                EventBus.Instance.Raise(TargetType.Communicate, npcId, 1);
            }
        }
    }
}