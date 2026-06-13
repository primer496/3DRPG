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

        [Header("Yarn 对话设置")]
        public string startNodeName = "Start";

        private DialogueRunner dialogueRunner;
        private bool isPlayerInRange = false;

        private void Start()
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
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
                // TODO: 可以在这里抛出事件，通过UI显示“按 [E] 键交谈”的提示
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                
                // 如果玩家被外力击退或某种原因脱离了触发器范围，强制中断对话以防Bug
                if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
                {
                    dialogueRunner.Stop();
                }
            }
        }
    }
}