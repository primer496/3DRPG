using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Model;

namespace InventorySystem.ViewModel
{
    public class InventoryViewModel : MonoBehaviour
    {
        [Header("Model Reference")]
        public InventoryModel inventoryModel;

        [Header("Bindable Data Source (for UI Data Binding)")]
        public InventoryBindableData bindableData;

        private ItemCategory currentCategory = ItemCategory.Item;
        private int selectedSlotIndex = -1;

        private void OnEnable()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnInventoryChanged += HandleModelChanged;
            }
        }

        private void OnDisable()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnInventoryChanged -= HandleModelChanged;
            }
        }

        private void HandleModelChanged()
        {
            RefreshDisplaySlots();
        }

        private void Start()
        {
            if (bindableData == null)
                bindableData = new InventoryBindableData();

            RefreshDisplaySlots();
        }

        private List<InventorySlot> cachedDisplaySlots = new List<InventorySlot>();
        private static readonly InventorySlot emptySlotRef = new InventorySlot(); // 只读全局空引用用于占位

        private void RefreshDisplaySlots()
        {
            cachedDisplaySlots = GetCurrentDisplaySlots();
        }

        public List<InventorySlot> GetCurrentDisplaySlots()
        {
            if (inventoryModel == null) return new List<InventorySlot>(42);

            var filteredItems = inventoryModel.slots
                .Where(s => !s.IsEmpty && s.itemData.category == currentCategory)
                .ToList();

            var displaySlots = new List<InventorySlot>(42);
            for (int i = 0; i < 42; i++)
            {
                if (i < filteredItems.Count)
                    displaySlots.Add(filteredItems[i]);
                else
                    displaySlots.Add(emptySlotRef); // 复用唯一的空引用对象替代 new InventorySlot()
            }

            return displaySlots;
        }

        public InventorySlot GetSlotAt(int index)
        {
            if (index < 0 || index >= cachedDisplaySlots.Count) 
                return emptySlotRef; // 防止越界时分配新内存
            return cachedDisplaySlots[index];
        }

        public string GetItemCountText(int slotIndex)
        {
            var slot = GetSlotAt(slotIndex);
            return slot.IsEmpty ? "" : (slot.amount > 1 ? slot.amount.ToString() : "");
        }

        public string GetIconPath(int slotIndex)
        {
            var slot = GetSlotAt(slotIndex);
            return slot.IsEmpty ? "" : (slot.itemData.iconPath ?? "");
        }

        public void ChangeCategory(int categoryIndex)
        {
            currentCategory = (ItemCategory)(categoryIndex - 1);
            selectedSlotIndex = -1;
            
            // 先更新缓存数据
            RefreshDisplaySlots();
            
            // 然后再更新绑定数据，触发事件
            if (bindableData != null)
            {
                bindableData.currentCategoryIndex = categoryIndex;
                bindableData.activeCategoryTab = categoryIndex;
                bindableData.selectedSlotIndex = -1;
                bindableData.isPreviewVisible = false;
            }
        }

        public void HoverItem(int uiIndex)
        {
            // 保持悬停方法为空，因为我们使用点击显示预览
        }

        public void SelectItem(int uiIndex)
        {
            selectedSlotIndex = uiIndex;
            if (bindableData != null)
                bindableData.selectedSlotIndex = selectedSlotIndex;

            // 直接从当前显示槽获取数据，不依赖缓存
            var slot = GetSlotAt(uiIndex);
            if (!slot.IsEmpty)
            {
                var itemData = slot.itemData;
                if (bindableData != null)
                {
                    bindableData.previewTitle = itemData.itemName;
                    bindableData.previewDescription = itemData.description;
                    bindableData.isPreviewVisible = true;
                }
            }
            else
            {
                if (bindableData != null)
                {
                    bindableData.previewTitle = "";
                    bindableData.previewDescription = "";
                    bindableData.isPreviewVisible = false;
                }
            }
        }

        public void DeleteSelectedItem()
        {
            if (selectedSlotIndex < 0) return;

            var slots = cachedDisplaySlots;
            if (selectedSlotIndex < slots.Count && !slots[selectedSlotIndex].IsEmpty)
            {
                var targetSlot = slots[selectedSlotIndex];
                int actualIndex = inventoryModel.slots.IndexOf(targetSlot);

                if (actualIndex != -1)
                {
                    inventoryModel.RemoveItem(actualIndex, targetSlot.amount);
                }
            }

            selectedSlotIndex = -1;
            if (bindableData != null)
                bindableData.selectedSlotIndex = -1;
            HoverItem(-1);
        }

        public void DeleteItems(IEnumerable<int> uiIndices)
        {
            var slots = cachedDisplaySlots;
            foreach (int uiIndex in uiIndices)
            {
                if (uiIndex >= 0 && uiIndex < slots.Count && !slots[uiIndex].IsEmpty)
                {
                    var targetSlot = slots[uiIndex];
                    int actualIndex = inventoryModel.slots.IndexOf(targetSlot);

                    if (actualIndex != -1)
                    {
                        inventoryModel.RemoveItem(actualIndex, targetSlot.amount);
                    }
                }
            }

            selectedSlotIndex = -1;
            if (bindableData != null)
                bindableData.selectedSlotIndex = -1;
            HoverItem(-1);
        }

        public void SetActiveSortTab(int tabIndex)
        {
            if (bindableData != null)
                bindableData.activeSortTab = tabIndex;

            // 点击排序Tab标签时，立即触发整理
            SortInventory();
        }

        // 调用底层排序逻辑
        public void SortInventory()
        {
            if (inventoryModel != null && bindableData != null)
            {
                inventoryModel.SortInventory(bindableData.activeSortTab);
            }
            selectedSlotIndex = -1;
            if (bindableData != null) bindableData.selectedSlotIndex = -1;
            HoverItem(-1);
        }

        // 调用底层重置逻辑
        public void ResetInventory()
        {
            if (inventoryModel != null)
            {
                inventoryModel.ReloadInitialItems();
            }
            selectedSlotIndex = -1;
            if (bindableData != null) bindableData.selectedSlotIndex = -1;
            HoverItem(-1);
        }

        public void UseItem(int uiIndex)
        {
            if (uiIndex < 0) return;

            var slots = cachedDisplaySlots;
            if (uiIndex >= slots.Count || slots[uiIndex].IsEmpty) return;

            var targetSlot = slots[uiIndex];
            var itemData = targetSlot.itemData;

            Debug.Log($"使用物品: {itemData.itemName}");
            inventoryModel.RemoveItem(inventoryModel.slots.IndexOf(targetSlot), 1);

            selectedSlotIndex = -1;
            if (bindableData != null)
            {
                bindableData.selectedSlotIndex = -1;
                bindableData.isPreviewVisible = false;
            }
        }
    }
}
