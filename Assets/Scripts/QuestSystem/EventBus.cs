using System;
using System.Collections.Generic;


namespace TaskManager
{
    public class EventBus
    {
        // C# 简洁单例写法，自带线程安全且省去了 null 判断的代码
        public static EventBus Instance { get; } = new EventBus();

        // 隐藏构造函数，防止外部 new EventBus()
        private EventBus() { } 

        private readonly Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

        // 名称加上 On 前缀，表明这是一个事件广播，并加上 event 关键字保护它（防止外部乱改或清空）
        public event Action<TargetType, string, int> OnGameActivityTriggered;

        // 全局输入锁定事件（如：在对话时锁定玩家操作）
        public event Action<bool> OnInputLockStateChanged;

        // ========== 奖励 & 伤害管道（不与任务追踪混合） ==========
        public event Action<string, int> OnItemRewarded;   // itemId, amount → InventorySystem 加物品
        public event Action<int> OnGoldRewarded;             // amount → PlayerStats 加金币
        public event Action<int> OnExpRewarded;              // amount → PlayerStats 加经验
        public event Action<int> OnPlayerDamaged;            // amount → PlayerStats 扣 HP

        // amount 给个默认值 1，大多数情况（杀一只怪、对话一次）就不需要每次都传数字了
        public void Raise(TargetType targetType, string targetId, int amount = 1)
        {
            OnGameActivityTriggered?.Invoke(targetType, targetId, amount);
        }

        public void RaiseInputLock(bool isLocked)
        {
            OnInputLockStateChanged?.Invoke(isLocked);
        }

        public void RaiseItemReward(string itemId, int amount)
        {
            OnItemRewarded?.Invoke(itemId, amount);
        }

        public void RaiseGoldReward(int amount)
        {
            OnGoldRewarded?.Invoke(amount);
        }

        public void RaiseExpReward(int amount)
        {
            OnExpRewarded?.Invoke(amount);
        }

        public void RaiseDamage(int amount)
        {
            OnPlayerDamaged?.Invoke(amount);
        }

        public void Raise(string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent?.Invoke();
            }
        }

        public void Subscribe(string eventName, Action listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = listener;
            }
            else
            {
                eventDictionary[eventName] += listener;
            }
        }

        public void Unsubscribe(string eventName, Action listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= listener;
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }
    }
}
