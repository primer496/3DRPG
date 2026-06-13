using System.Collections.Generic;
using UnityEngine;

namespace TaskManager
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [Header("基础配置")]
        [Tooltip("任务的全局唯一标识，例如: Quest_001_Investigation")]
        public string id; 
        
        public string title;
        
        [TextArea(3, 5)]
        public string description;

        [Header("流程配置")]
        [Tooltip("是否要求玩家按顺序完成目标（如果是true，必须完成上一个目标才会激活下一个）")]
        public bool isOrdered = true;
        
        [Header("多阶段任务目标")]
        public List<QuestObjective> objectives = new List<QuestObjective>();

        [Header("任务奖励")]
        public List<QuestReward> rewards = new List<QuestReward>();
    }

    [System.Serializable]
    public class QuestObjective
    {
        [Tooltip("目标的类型：击杀、收集、对话等")]
        public TargetType targetType;
        
        [Tooltip("目标的具体ID，如: 怪物ID(Slime_01)、物品ID(IronOre)或NPC名字")]
        public string targetId;
        
        [Tooltip("完成该目标需要的数量")]
        public int requiredAmount = 1;

        [Tooltip("在UI(任务日志)右侧显示的具体文字提示。")]
        public string uiDescription;
    }

    [System.Serializable]
    public class QuestReward
    {
        [Tooltip("奖励的类型: 物品(放入背包)、虚拟货币、经验值")]
        public RewardType rewardType;
        
        [Tooltip("如果是Item类型，填写放入背包的Item ID; 如果是其他类型，可留空。")]
        public string rewardId;
        
        [Tooltip("发放的数量。比如 200 (金币)、500 (经验) 或 3 (物品个数)")]
        public int amount = 1;
    }

    public enum TargetType
    {
        Kill,
        Collect,
        Communicate
    }

    public enum RewardType
    {
        Item,
        Currency,
        Experience
    }
}
