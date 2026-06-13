using System;
using UnityEngine;

namespace InventorySystem.ViewModel
{
    public class InventoryBindableData
    {
        private int _currentCategoryIndex = 1;
        private bool _isPreviewVisible;
        private int _activeSortTab;
        private int _activeCategoryTab = 1;

        private string _previewTitle = "";
        private string _previewDescription = "";

        // UI视图的状态数据中心，纯C#事件驱动
        public string previewTitle 
        { 
            get => _previewTitle; 
            set
            {
                if (_previewTitle == value) return;
                _previewTitle = value;
                OnPreviewStateChanged?.Invoke();
            }
        }

        public string previewDescription 
        { 
            get => _previewDescription; 
            set
            {
                if (_previewDescription == value) return;
                _previewDescription = value;
                OnPreviewStateChanged?.Invoke();
            }
        }

        public int selectedSlotIndex { get; set; } = -1;

        // 对控制器层级的界面重绘事件通知
        public event Action OnCategoryChanged;
        public event Action OnPreviewStateChanged;
        public event Action OnTabChanged;

        // 以下属性涉及复杂面板层级与CSS切换，通过事件广播通知View层
        public int currentCategoryIndex
        {
            get => _currentCategoryIndex;
            set
            {
                if (_currentCategoryIndex == value) return;
                _currentCategoryIndex = value;
                OnCategoryChanged?.Invoke();
            }
        }

        public bool isPreviewVisible
        {
            get => _isPreviewVisible;
            set
            {
                if (_isPreviewVisible == value) return;
                _isPreviewVisible = value;
                OnPreviewStateChanged?.Invoke();
            }
        }

        public int activeSortTab
        {
            get => _activeSortTab;
            set
            {
                if (_activeSortTab == value) return;
                _activeSortTab = value;
                OnTabChanged?.Invoke();
            }
        }

        public int activeCategoryTab
        {
            get => _activeCategoryTab;
            set
            {
                if (_activeCategoryTab == value) return;
                _activeCategoryTab = value;
                OnTabChanged?.Invoke();
            }
        }
    }
}
