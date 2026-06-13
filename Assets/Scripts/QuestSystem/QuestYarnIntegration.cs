using UnityEngine;
using Yarn.Unity;
using TaskManager;

namespace QuestSystem
{
    /// <summary>
    /// 负责将 Yarn 对话中的自定义指令(Commands)绑定到 C# 逻辑
    /// </summary>
    public class QuestYarnIntegration : MonoBehaviour
    {
        private DialogueRunner dialogueRunner;

        private void Awake()
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                Debug.LogWarning("[QuestYarnIntegration] 场景中未找到 DialogueRunner 组件，请挂载！");
                return;
            }

            // 注册 Yarn 命令与对应的 C# 方法
            dialogueRunner.AddCommandHandler<string>("AcceptQuest", AcceptQuest);
            dialogueRunner.AddCommandHandler<string>("CompleteQuest", CompleteQuest);
            dialogueRunner.AddCommandHandler<string, int>("GivePlayerItem", GivePlayerItem);
            dialogueRunner.AddCommandHandler<string>("TriggerCommunicateEvent", TriggerCommunicateEvent);
            dialogueRunner.AddCommandHandler<string, int>("SetReputation", SetReputation);

            // 存档触发（纯自动存档模式，剧情节点插入 <<AutoSave>> 即可）
            dialogueRunner.AddCommandHandler("AutoSave", () => {
                EventBus.Instance.Raise("AutoSave");
            });

            // 直接推进任务目标（对话中提交线索时使用，不走 Collect 匹配）
            dialogueRunner.AddCommandHandler<string>("AdvanceQuestObjective", AdvanceQuestObjective);

            // 为了能够手动注射变量用于快速测试
            dialogueRunner.AddCommandHandler("Test_FastForwardVars", Test_FastForwardVars);
        }

        // =======================
        // 快速测试代码：强制推进全局变量
        // =======================
        private void Test_FastForwardVars()
        {
            var storage = FindFirstObjectByType<InMemoryVariableStorage>();
            if (storage == null) return;

            // 检查当前进度，每次运行这段命令时自动将进度推进一格
            storage.TryGetValue("$InvestigationProgress", out float currentProgress);

            if (currentProgress == 0)
            {
                storage.SetValue("$InvestigationProgress", 1f);
                Debug.Log("[Test] 自动发起了推进 -> InvestigationProgress 设为 1");
                GivePlayerItem("Clue_Footprint", 1); // 顺便给测试用的包里虚空塞个道具
            }
            else if (currentProgress == 1)
            {
                storage.SetValue("$InvestigationProgress", 2f);
                Debug.Log("[Test] 自动发起了推进 -> InvestigationProgress 设为 2");
                GivePlayerItem("Clue_Herb", 1);
            }
        }

        private void AcceptQuest(string questId)
        {
            QuestData questData = LoadQuestData(questId);
            if (questData != null)
            {
                QuestManager.Instance.AcceptQuest(questData);
            }
            else
            {
                Debug.LogError($"[Yarn] 接取任务失败，找不到 QuestData: {questId}");
            }
        }

        private void CompleteQuest(string questId)
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.CompleteQuest(questId);
        }

        private void SetReputation(string factionId, int amount)
        {
            // TODO: 接入声望系统后在此扩展；目前仅打日志，不阻塞对话流程
            Debug.Log($"[QuestYarnIntegration] SetReputation: {factionId} +{amount}");
        }

        private void GivePlayerItem(string itemId, int amount)
        {
            // 特殊处理：Gold 走货币管道而非物品
            if (string.Equals(itemId, "Gold", System.StringComparison.OrdinalIgnoreCase))
            {
                EventBus.Instance.RaiseGoldReward(amount);
                Debug.Log($"[QuestYarnIntegration] 发放金币: {amount}");
                return;
            }

            // 普通物品走 ItemReward 管道（InventoryViewModel 监听加包）
            // 同时广播 Collect 活动，供 QuestManager 追踪任务目标进度
            EventBus.Instance.RaiseItemReward(itemId, amount);
            EventBus.Instance.Raise(TargetType.Collect, itemId, amount);
            Debug.Log($"[QuestYarnIntegration] 发放物品: {itemId} x{amount}");
        }

        /// <summary>
        /// Yarn 命令 &lt;&lt;AdvanceQuestObjective questId&gt;&gt; —
        /// 直接推进指定任务的当前目标（标记为完成，移至下一个目标），同时通知 UI 刷新。
        /// 不走 Collect 匹配校验，用于对话中玩家提交线索时直接更新任务面板。
        /// </summary>
        private void AdvanceQuestObjective(string questId)
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.AdvanceQuestObjective(questId);
        }

        public void TriggerCommunicateEvent(string targetId)
        {
            EventBus.Instance.Raise(TargetType.Communicate, targetId, 1);
        }

        private QuestData LoadQuestData(string questId)
        {
            // 运行时加载：QuestData 资产必须放在 Resources/QuestData/ 文件夹下，文件名与 questId 一致
            QuestData data = Resources.Load<QuestData>($"GameConfigs/Quest/{questId}");
            if (data == null)
                Debug.LogError($"[QuestYarnIntegration] 找不到 QuestData：Resources/GameConfigs/Quest/{questId}，请确认资产已放入该目录且文件名与 questId 匹配。");
            return data;
        }

        /// <summary>
        /// 确保场景中存在一个已注册 Yarn 命令的 QuestYarnIntegration 实例。
        /// 若不存在，则自动挂载到 DialogueRunner 所在 GameObject，或创建独立节点。
        /// </summary>
        public static void EnsureRegistered()
        {
            if (FindFirstObjectByType<QuestYarnIntegration>() != null)
                return;

            var dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            if (dialogueRunner != null)
            {
                dialogueRunner.gameObject.AddComponent<QuestYarnIntegration>();
                Debug.Log("[QuestYarnIntegration] 自动注册到 DialogueRunner。");
            }
            else
            {
                var go = new GameObject("[QuestYarnIntegration]");
                go.AddComponent<QuestYarnIntegration>();
                Debug.Log("[QuestYarnIntegration] 未找到 DialogueRunner，已创建独立节点。");
            }
        }
    }
}