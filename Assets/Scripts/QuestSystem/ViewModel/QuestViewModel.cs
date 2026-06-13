using UnityEngine;
using TaskManager;

namespace QuestSystem.ViewModel
{
    /// <summary>
    /// ViewModel 层：桥接 QuestManager（Model）与 QuestUIController（View）。
    /// 职责：监听 QuestManager 的事件，将 QuestInstance 数据同步到 QuestBindableData，
    /// 由 QuestBindableData 的属性变更事件驱动 View 自动刷新。
    /// </summary>
    public class QuestViewModel : MonoBehaviour
    {
        /// <summary>可绑定数据对象，View 层订阅其事件以响应刷新</summary>
        public QuestBindableData bindableData { get; private set; }

        private bool _isSubscribed = false;

        private void Awake()
        {
            bindableData = new QuestBindableData();
        }

        private void OnEnable()
        {
            TrySubscribeQuestManager();
            EventBus.Instance.Subscribe("QuestsRestored", HandleQuestsRestored);
        }

        private void Start()
        {
            // OnEnable 时若 QuestManager.Instance 尚未就绪，Start 阶段再兜底订阅一次。
            // QuestManager 采用懒单例，此处访问会自动创建实例。
            if (!_isSubscribed)
                TrySubscribeQuestManager();
        }

        private void OnDisable()
        {
            if (_isSubscribed && QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestUpdated -= HandleQuestUpdated;
                _isSubscribed = false;
            }
            EventBus.Instance.Unsubscribe("QuestsRestored", HandleQuestsRestored);
        }

        private void TrySubscribeQuestManager()
        {
            if (_isSubscribed) return;
            // Instance 属性为懒单例，访问时若不存在会自动创建
            QuestManager.Instance.OnQuestUpdated += HandleQuestUpdated;
            _isSubscribed = true;
        }

        /// <summary>
        /// 接收 QuestManager 的任务更新事件，将最新状态写入 bindableData。
        /// bindableData 的 setter 会自动触发 OnQuestChanged 事件通知 View 刷新。
        /// </summary>
        private void HandleQuestUpdated(QuestInstance quest)
        {
            if (quest == null) return;
            bindableData.SetActiveQuest(
                quest.questData,
                quest.progressList,
                quest.currentActiveIndex
            );
        }

        // =======================
        // 供 View 层调用的指令接口（对齐 InventoryViewModel 模式）
        // =======================

        /// <summary>追踪指定任务：将其设为当前显示的任务</summary>
        public void TrackQuest(string questId)
        {
            var quest = QuestManager.Instance.activeQuests
                .Find(q => q.questData.id == questId && !q.isCompleted);
            if (quest != null)
                bindableData.SetActiveQuest(quest.questData, quest.progressList, quest.currentActiveIndex);
        }

        /// <summary>读档后任务恢复回调，刷新 UI 显示</summary>
        private void HandleQuestsRestored()
        {
            RefreshToLatestQuest();
        }

        /// <summary>刷新 bindableData 为当前第一个未完成任务（用于打开面板时默认显示）</summary>
        public void RefreshToLatestQuest()
        {
            var quest = QuestManager.Instance.activeQuests.Find(q => !q.isCompleted);
            if (quest != null)
                bindableData.SetActiveQuest(quest.questData, quest.progressList, quest.currentActiveIndex);
            else
                bindableData.SetActiveQuest(null, null, -1); // 无任务时清空
        }
    }
}
