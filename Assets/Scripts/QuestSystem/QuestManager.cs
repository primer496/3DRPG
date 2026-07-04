using System;
using System.Collections.Generic;
using QuestSystem;
using UnityEngine;
using FinalRPG.Utils;

namespace TaskManager
{
    // 用于在运行时记录一个任务具体的进展状态
    [System.Serializable]
    public class QuestInstance
    {
        public QuestData questData;
        
        // 记录每个目标的当前进度数值
        public List<int> progressList;
        
        // 如果是按顺序的任务，记录当前激活的目标索引
        public int currentActiveIndex;
        
        public bool isCompleted;

        public QuestInstance(QuestData data)
        {
            questData = data;
            progressList = new List<int>(new int[data.objectives.Count]); // 初始化为全 0
            currentActiveIndex = 0;
            isCompleted = false;
        }
    }

    public class QuestManager : MonoBehaviour
    {
        private static QuestManager _instance;

        /// <summary>
        /// 懒单例：若场景中不存在 QuestManager，首次访问时自动创建并 DontDestroyOnLoad。
        /// </summary>
        public static QuestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[QuestManager]");
                    DontDestroyOnLoad(go);  // 先移出场景再挂组件，避免关闭场景时残留警告
                    _instance = go.AddComponent<QuestManager>();
                }
                return _instance;
            }
        }

        public List<QuestInstance> activeQuests = new List<QuestInstance>();

        // 抛出事件给 UI：当任务接取、进度更新、完成时调用
        public event Action<QuestInstance> OnQuestUpdated;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // 确保 QuestYarnIntegration 已注册到场景中的 DialogueRunner
            QuestYarnIntegration.EnsureRegistered();
        }

        private void OnEnable()
        {
            // 订阅全局事件总线
            EventBus.Instance.OnGameActivityTriggered += HandleGameActivity;
        }

        private void OnDisable()
        {
            // 取消订阅，防内存泄漏
            EventBus.Instance.OnGameActivityTriggered -= HandleGameActivity;
        }

        /// <summary>
        /// 当游戏中发生击杀、收集、对话时，由 EventBus 触发此方法
        /// </summary>
        private void HandleGameActivity(TargetType targetType, string targetId, int amount)
        {
            bool hasUpdate = false;

            // 遍历所有进行中的任务
            foreach (var quest in activeQuests)
            {
                if (quest.isCompleted) continue; // 已完成跳过

                if (quest.questData.isOrdered)
                {
                    // 必须按顺序完成目标
                    if (quest.currentActiveIndex < quest.questData.objectives.Count)
                    {
                        var currentObj = quest.questData.objectives[quest.currentActiveIndex];
                        
                        // 匹配类型和目标 ID
                        if (currentObj.targetType == targetType && currentObj.targetId == targetId)
                        {
                            quest.progressList[quest.currentActiveIndex] += amount;
                            
                            // 检查当前目标是否达成设定数量
                            if (quest.progressList[quest.currentActiveIndex] >= currentObj.requiredAmount)
                            {
                                quest.progressList[quest.currentActiveIndex] = currentObj.requiredAmount;
                                quest.currentActiveIndex++; // 推进至下一个目标
                                CheckQuestCompletion(quest); // 检查整个任务是否完成
                            }
                            hasUpdate = true;
                            OnQuestUpdated?.Invoke(quest); // 进度刷新，通知UI
                        }
                    }
                }
                else
                {
                    // 无序任务：同时检查所有未完成的目标
                    for (int i = 0; i < quest.questData.objectives.Count; i++)
                    {
                        var obj = quest.questData.objectives[i];
                        if (quest.progressList[i] < obj.requiredAmount && 
                            obj.targetType == targetType && 
                            obj.targetId == targetId)
                        {
                            quest.progressList[i] += amount;
                            if (quest.progressList[i] >= obj.requiredAmount)
                            {
                                quest.progressList[i] = obj.requiredAmount;
                            }
                            hasUpdate = true;
                        }
                    }

                    if (hasUpdate)
                    {
                        CheckQuestCompletion(quest); // 检查整个任务是否完成
                        OnQuestUpdated?.Invoke(quest); // 进度刷新，通知UI
                    }
                }
            }
        }

        private void CheckQuestCompletion(QuestInstance quest)
        {
            bool allDone = true;
            for (int i = 0; i < quest.questData.objectives.Count; i++)
            {
                if (quest.progressList[i] < quest.questData.objectives[i].requiredAmount)
                {
                    allDone = false;
                    break;
                }
            }

            if (allDone)
            {
                MarkQuestCompleted(quest);
            }
        }

        private void MarkQuestCompleted(QuestInstance quest)
        {
            if (quest.isCompleted) return;
            quest.isCompleted = true;
            RPGLog.Debug("Quest", $"任务完成：{quest.questData.title} (id={quest.questData.id})");

            var rewards = quest.questData.rewards;
            if (rewards == null || rewards.Count == 0)
            {
                RPGLog.Warning("Quest", $"任务 {quest.questData.id} 没有配置奖励！");
                OnQuestUpdated?.Invoke(quest);
                return;
            }

            RPGLog.Debug("Quest", $"准备发放 {rewards.Count} 项奖励");
            foreach (var reward in rewards)
            {
                RPGLog.Debug("Quest", $"奖励: type={reward.rewardType} id={reward.rewardId} amount={reward.amount}");
                switch (reward.rewardType)
                {
                    case RewardType.Item:
                        EventBus.Instance.RaiseItemReward(reward.rewardId, reward.amount);
                        break;
                    case RewardType.Currency:
                        EventBus.Instance.RaiseGoldReward(reward.amount);
                        break;
                    case RewardType.Experience:
                        EventBus.Instance.RaiseExpReward(reward.amount);
                        break;
                }
            }
            RPGLog.Debug("Quest", "奖励广播完毕");

            OnQuestUpdated?.Invoke(quest);
        }

        /// <summary>
        /// 由 Yarn 命令 &lt;&lt;CompleteQuest questId&gt;&gt; 调用，强制完成指定任务
        /// </summary>
        public void CompleteQuest(string questId)
        {
            var quest = activeQuests.Find(q => q.questData.id == questId);
            if (quest != null)
            {
                MarkQuestCompleted(quest);
            }
            else
            {
                RPGLog.Warning("Quest", $"CompleteQuest: 找不到进行中的任务 {questId}");
            }
        }

        /// <summary>
        /// 由 Yarn 命令 &lt;&lt;AdvanceQuestObjective questId&gt;&gt; 调用，
        /// 直接将当前目标标记为完成并推进至下一个目标（不检验 Collect 匹配）。
        /// 用于对话中玩家提交线索时直接更新任务面板进度。
        /// </summary>
        public void AdvanceQuestObjective(string questId)
        {
            var quest = activeQuests.Find(q => q.questData.id == questId && !q.isCompleted);
            if (quest == null)
            {
                RPGLog.Warning("Quest", $"AdvanceQuestObjective: 找不到进行中的任务 {questId}");
                return;
            }

            if (quest.currentActiveIndex >= quest.questData.objectives.Count)
            {
                RPGLog.Warning("Quest", $"AdvanceQuestObjective: {questId} 所有目标已完成，无法继续推进");
                return;
            }

            var obj = quest.questData.objectives[quest.currentActiveIndex];
            quest.progressList[quest.currentActiveIndex] = obj.requiredAmount;
            quest.currentActiveIndex++;

            RPGLog.Debug("Quest", $"推进目标: {questId} objective[{quest.currentActiveIndex - 1}] 完成 → 当前 activeIndex={quest.currentActiveIndex}");

            OnQuestUpdated?.Invoke(quest);
        }

        // =======================
        // 供 Yarn 等外部系统调用的接口
        // =======================
        
        /// <summary>
        /// 接收新任务（例如在 Yarn 对话中被调用）
        /// </summary>
        public void AcceptQuest(QuestData data)
        {
            if (data == null) return;
            
            // 防止重复接取
            if (activeQuests.Exists(q => q.questData.id == data.id && !q.isCompleted))
            {
                return; 
            }

            var newQuest = new QuestInstance(data);
            activeQuests.Add(newQuest);
            
            RPGLog.Debug("Quest", $"接受了新任务：{data.title}");
            OnQuestUpdated?.Invoke(newQuest); // 通知 UI 添加新任务面板
        }
    }
}
