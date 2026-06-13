using System;
using System.Collections.Generic;
using TaskManager;

namespace QuestSystem.ViewModel
{
    public class QuestBindableData
    {
        private QuestData _currentQuest;
        private List<int> _progressList = new List<int>();
        private int _currentActiveIndex = -1;

        public event Action OnQuestChanged;

        public QuestData currentQuest
        {
            get => _currentQuest;
            set
            {
                if (_currentQuest == value) return;
                _currentQuest = value;
                OnQuestChanged?.Invoke();
            }
        }

        public List<int> progressList
        {
            get => _progressList;
            set
            {
                if (_progressList == value) return;
                _progressList = value;
                OnQuestChanged?.Invoke();
            }
        }

        public int currentActiveIndex
        {
            get => _currentActiveIndex;
            set
            {
                if (_currentActiveIndex == value) return;
                _currentActiveIndex = value;
                OnQuestChanged?.Invoke();
            }
        }

        public int completedCount
        {
            get
            {
                if (_progressList == null || _currentQuest == null) return 0;
                int count = 0;
                for (int i = 0; i < _currentQuest.objectives.Count; i++)
                {
                    if (i < _progressList.Count && _progressList[i] >= _currentQuest.objectives[i].requiredAmount)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public float progressPercent
        {
            get
            {
                if (_currentQuest == null || _currentQuest.objectives.Count == 0) return 0;
                return (float)completedCount / _currentQuest.objectives.Count * 100;
            }
        }

        public string progressText
        {
            get
            {
                if (_currentQuest == null) return "0/0 (0%)";
                return $"{completedCount}/{_currentQuest.objectives.Count} ({Math.Round(progressPercent)}%)";
            }
        }

        public string nextHint
        {
            get
            {
                if (_currentQuest == null || _currentQuest.objectives.Count == 0) return "";
                if (_currentActiveIndex >= 0 && _currentActiveIndex < _currentQuest.objectives.Count)
                {
                    return _currentQuest.objectives[_currentActiveIndex].uiDescription;
                }
                return "";
            }
        }

        public void UpdateProgress(int objectiveIndex, int newAmount)
        {
            while (_progressList.Count <= objectiveIndex)
            {
                _progressList.Add(0);
            }
            _progressList[objectiveIndex] = newAmount;
            OnQuestChanged?.Invoke();
        }

        public void SetActiveQuest(QuestData quest, List<int> progress, int activeIndex)
        {
            _currentQuest = quest;
            _progressList = progress != null ? new List<int>(progress) : new List<int>();
            _currentActiveIndex = activeIndex;
            OnQuestChanged?.Invoke();
        }
    }
}