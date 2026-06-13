using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using QuestSystem.ViewModel;
using TaskManager;

namespace QuestSystem.View
{
    public class QuestUIController : MonoBehaviour
    {
        [Header("ViewModel Reference")]
        public QuestViewModel viewModel;

        private QuestBindableData bindableData;

        private VisualElement root;
        private Label questTitleLabel;
        private Label questDescLabel;
        private VisualElement objectivesListContainer;
        private ProgressBar questProgressBar;
        private Button btnTrackQuest;
        private Button btnCloseQuest;
        private Label footerHintLabel;

        private void Awake()
        {
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogError("[QuestUIController] 未找到 UIDocument 组件，请确认挂载在正确的 GameObject 上。");
                return;
            }
            root = uiDoc.rootVisualElement;
            CacheElements();
            RegisterEvents();
        }

        private void Start()
        {
            // 初始隐藏：UI Toolkit 用 display:none 代替 SetActive(false)
            if (root != null)
                root.style.display = DisplayStyle.None;

            // 若 Inspector 未赋值 viewModel，先在场景中查找（可能在 QuestManager 上），再回退到本 GameObject
            if (viewModel == null)
            {
                viewModel = GetComponent<QuestViewModel>();
                if (viewModel == null)
                {
                    viewModel = FindFirstObjectByType<QuestViewModel>();
                    if (viewModel == null)
                    {
                        viewModel = gameObject.AddComponent<QuestViewModel>();
                        Debug.LogWarning("[QuestUIController] 未找到 QuestViewModel，已在本 GameObject 上创建。建议在 Inspector 中手动赋值。");
                    }
                }
            }

            // 绑定 bindableData（与 InventoryUIController 一致，在 Start 而非 OnEnable 中绑定）
            if (viewModel != null)
            {
                bindableData = viewModel.bindableData;
                if (bindableData != null)
                {
                    bindableData.OnQuestChanged += RefreshUI;
                    RefreshUI();
                }
            }
        }

        private void CacheElements()
        {
            questTitleLabel = root.Q<Label>("quest-title-label");
            questDescLabel = root.Q<Label>("quest-desc-label");
            objectivesListContainer = root.Q<VisualElement>("objectives-list-container");
            questProgressBar = root.Q<ProgressBar>("quest-progress-bar");
            btnTrackQuest = root.Q<Button>("btn-track-quest");
            btnCloseQuest = root.Q<Button>("btn-close-quest");
            footerHintLabel = root.Q<Label>("footer-hint-label");
        }

        private void RegisterEvents()
        {
            if (btnTrackQuest != null)
            {
                btnTrackQuest.clicked += OnTrackQuestClicked;
            }
            if (btnCloseQuest != null)
            {
                btnCloseQuest.clicked += OnCloseQuestClicked;
            }
        }

        private void UnsubscribeBindableChanges()
        {
            if (bindableData != null)
            {
                bindableData.OnQuestChanged -= RefreshUI;
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe("ToggleQuestLog", ToggleQuestLog);
            EventBus.Instance.Subscribe("CloseQuestLog", CloseQuestLog);
            // OnDisable 会取消 bindableData 订阅；OnEnable 时若 Start 已运行则重新订阅，防止订阅丢失。
            if (bindableData != null)
            {
                bindableData.OnQuestChanged -= RefreshUI;
                bindableData.OnQuestChanged += RefreshUI;
            }
        }

        private void OnDisable()
        {
            UnsubscribeBindableChanges();
            EventBus.Instance.Unsubscribe("ToggleQuestLog", ToggleQuestLog);
            EventBus.Instance.Unsubscribe("CloseQuestLog", CloseQuestLog);
        }

        /// <summary>由 InventoryUIController 互斥关闭任务面板时调用</summary>
        private void CloseQuestLog()
        {
            if (root != null && root.style.display != DisplayStyle.None)
                root.style.display = DisplayStyle.None;
        }

        public void RefreshUI()
        {
            if (bindableData == null || bindableData.currentQuest == null)
            {
                ClearUI();
                return;
            }

            QuestData quest = bindableData.currentQuest;
            List<int> progressList = bindableData.progressList;
            int currentActiveIndex = bindableData.currentActiveIndex;

            if (questTitleLabel != null) questTitleLabel.text = quest.title;
            if (questDescLabel != null) questDescLabel.text = quest.description;

            if (objectivesListContainer != null)
                UpdateObjectives(quest, progressList, currentActiveIndex);
            if (questProgressBar != null)
                UpdateProgressBar(quest, progressList);
            UpdateFooterHint(quest, currentActiveIndex);
        }

        public void RefreshUI(QuestData data, List<int> progressList, int currentActiveIndex)
        {
            if (data == null)
            {
                ClearUI();
                return;
            }

            if (questTitleLabel != null) questTitleLabel.text = data.title;
            if (questDescLabel != null) questDescLabel.text = data.description;

            if (objectivesListContainer != null)
                UpdateObjectives(data, progressList, currentActiveIndex);
            if (questProgressBar != null)
                UpdateProgressBar(data, progressList);
            UpdateFooterHint(data, currentActiveIndex);
        }

        private void UpdateObjectives(QuestData quest, List<int> progressList, int currentActiveIndex)
        {
            objectivesListContainer.Clear();

            for (int i = 0; i < quest.objectives.Count; i++)
            {
                QuestObjective objective = quest.objectives[i];
                VisualElement objectiveItem = CreateObjectiveItem(objective, i, progressList, currentActiveIndex, quest.isOrdered);
                objectivesListContainer.Add(objectiveItem);
            }
        }

        private VisualElement CreateObjectiveItem(QuestObjective objective, int index, List<int> progressList, int currentActiveIndex, bool isOrdered)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("objective-item");

            VisualElement checkbox = new VisualElement();
            checkbox.AddToClassList("objective-checkbox");

            Label textLabel = new Label();
            textLabel.AddToClassList("objective-text");

            string progressText = "";
            if (progressList != null && index < progressList.Count)
            {
                progressText = $" ({progressList[index]}/{objective.requiredAmount})";
            }
            textLabel.text = objective.uiDescription + progressText;

            item.Add(checkbox);
            item.Add(textLabel);

            string statusClass = DetermineObjectiveStatus(index, progressList, currentActiveIndex, objective.requiredAmount, isOrdered);
            item.AddToClassList(statusClass);

            return item;
        }

        private string DetermineObjectiveStatus(int index, List<int> progressList, int currentActiveIndex, int requiredAmount, bool isOrdered)
        {
            bool isCompleted = progressList != null && index < progressList.Count && progressList[index] >= requiredAmount;

            if (isCompleted)
            {
                return "objective-completed";
            }

            if (isOrdered)
            {
                if (index == currentActiveIndex)
                {
                    return "objective-active";
                }
                else if (index > currentActiveIndex)
                {
                    return "objective-locked";
                }
            }
            else
            {
                if (index == currentActiveIndex)
                {
                    return "objective-active";
                }
            }

            return "objective-locked";
        }

        private void UpdateProgressBar(QuestData quest, List<int> progressList)
        {
            int completedCount = 0;
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                if (progressList != null && i < progressList.Count && progressList[i] >= quest.objectives[i].requiredAmount)
                {
                    completedCount++;
                }
            }

            float progressPercent = quest.objectives.Count > 0 ? (float)completedCount / quest.objectives.Count * 100 : 0;
            questProgressBar.value = progressPercent;
            questProgressBar.title = $"{completedCount}/{quest.objectives.Count} ({Mathf.Round(progressPercent)}%)";
        }

        private void UpdateFooterHint(QuestData quest, int currentActiveIndex)
        {
            if (footerHintLabel == null) return;
            if (currentActiveIndex >= 0 && currentActiveIndex < quest.objectives.Count)
            {
                footerHintLabel.text = "下一步: " + quest.objectives[currentActiveIndex].uiDescription;
            }
            else
            {
                footerHintLabel.text = "";
            }
        }

        private void ClearUI()
        {
            if (questTitleLabel != null) questTitleLabel.text = "";
            if (questDescLabel != null) questDescLabel.text = "";
            if (objectivesListContainer != null) objectivesListContainer.Clear();
            if (questProgressBar != null) { questProgressBar.value = 0; questProgressBar.title = "0/0 (0%)"; }
            if (footerHintLabel != null) footerHintLabel.text = "";
        }

        private void OnTrackQuestClicked()
        {
            Debug.Log("[QuestUI] 追踪任务按钮被点击");
        }

        private void OnCloseQuestClicked()
        {
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
                EventBus.Instance.RaiseInputLock(false);
            }
        }

        /// <summary>
        /// 外部调用此方法切换任务日志状态（打开或关闭）
        /// </summary>
        public void ToggleQuestLog()
        {
            if (root != null)
            {
                if (root.style.display == DisplayStyle.None)
                {
                    // 互斥：先关闭背包
                    EventBus.Instance.Raise("CloseInventory");
                    if (viewModel != null)
                        viewModel.RefreshToLatestQuest();
                    root.style.display = DisplayStyle.Flex;
                    EventBus.Instance.RaiseInputLock(true);
                }
                else
                {
                    root.style.display = DisplayStyle.None;
                    EventBus.Instance.RaiseInputLock(false);
                }
            }
        }
    }
}