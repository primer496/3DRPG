using System;
using Unity.Properties;
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

        // 供UI Toolkit进行数据绑定（SetBinding）的属性，引擎将自动生成变更通知
        [CreateProperty]
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

        [CreateProperty]
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

        [CreateProperty]
        public int selectedSlotIndex { get; set; } = -1;

        // 对控制器层级的界面重绘事件通知
        public event Action OnCategoryChanged;
        public event Action OnPreviewStateChanged;
        public event Action OnTabChanged;

        // 涉及复杂面板层级与CSS切换的属性，依然保持自定义事件的回调
        [CreateProperty]
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

        [CreateProperty]
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

        [CreateProperty]
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

        [CreateProperty]
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
