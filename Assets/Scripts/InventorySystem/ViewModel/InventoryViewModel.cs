using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Model;
using TaskManager;
using FinalRPG.Utils;

namespace InventorySystem.ViewModel
{
    public class InventoryViewModel : MonoBehaviour
    {
        // 核心数据模型由ViewModel直接创建并管理，不再挂载在GameObject上
        public InventoryModel inventoryModel;

        [Header("Bindable Data Source (for UI Data Binding)")]
        public InventoryBindableData bindableData;

        private ItemCategory currentCategory = ItemCategory.Item;
        private int selectedSlotIndex = -1;

        /// <summary>
        /// View 层应订阅此事件来刷新格子显示，而不是直接订阅 inventoryModel.OnInventoryChanged。
        /// 这样 View 不需要知道 Model 的存在，只依赖 ViewModel。
        /// </summary>
        public event Action OnDisplayChanged;

        private void Awake()
        {
            inventoryModel = new InventoryModel();
        }

        private void OnEnable()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnInventoryChanged += HandleModelChanged;
            }
            EventBus.Instance.OnItemRewarded += HandleItemReward;
        }

        private void OnDisable()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnInventoryChanged -= HandleModelChanged;
            }
            EventBus.Instance.OnItemRewarded -= HandleItemReward;
        }

        /// <summary>
        /// 监听 EventBus 的物品奖励事件，从 Resources 加载 ItemData 并加入背包。
        /// </summary>
        private void HandleItemReward(string itemId, int amount)
        {
            var itemData = Resources.Load<ItemData>($"GameConfigs/PackageModel/{itemId}");
            if (itemData != null)
            {
                inventoryModel?.AddItem(itemData, amount);
                RPGLog.Debug("Inventory", $"收到物品: {itemId} x{amount}");
            }
            else
            {
                RPGLog.Warning("Inventory", $"ItemData not found: GameConfigs/PackageModel/{itemId}");
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
        private static readonly InventorySlot emptySlotRef = new InventorySlot(); // 鍙鍏ㄥ眬绌哄紩鐢ㄧ敤浜庡崰浣�

        private void RefreshDisplaySlots()
        {
            cachedDisplaySlots = GetCurrentDisplaySlots();
            OnDisplayChanged?.Invoke();
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
                    displaySlots.Add(emptySlotRef); // 澶嶇敤鍞竴鐨勭┖寮曠敤瀵硅薄鏇夸唬 new InventorySlot()
            }

            return displaySlots;
        }

        public InventorySlot GetSlotAt(int index)
        {
            if (index < 0 || index >= cachedDisplaySlots.Count) 
                return emptySlotRef; // 闃叉瓒婄晫鏃跺垎閰嶆柊鍐呭瓨
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
            
            // 鍏堟洿鏂扮紦瀛樻暟鎹�
            RefreshDisplaySlots();
            
            // 鐒跺悗鍐嶆洿鏂扮粦瀹氭暟鎹紝瑙﹀彂浜嬩欢
            if (bindableData != null)
            {
                bindableData.currentCategoryIndex = categoryIndex;
                bindableData.activeCategoryTab = categoryIndex;
                bindableData.selectedSlotIndex = -1;
                bindableData.isPreviewVisible = false;
            }
        }

        public void SelectItem(int uiIndex)
        {
            selectedSlotIndex = uiIndex;
            if (bindableData != null)
                bindableData.selectedSlotIndex = selectedSlotIndex;

            // 鐩存帴浠庡綋鍓嶆樉绀烘Ы鑾峰彇鏁版嵁锛屼笉渚濊禆缂撳瓨
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
        }

        public void SetActiveSortTab(int tabIndex)
        {
            if (bindableData != null)
                bindableData.activeSortTab = tabIndex;

            // 鐐瑰嚮鎺掑簭Tab鏍囩鏃讹紝绔嬪嵆瑙﹀彂鏁寸悊
            SortInventory();
        }

        // 璋冪敤搴曞眰鎺掑簭閫昏緫
        public void SortInventory()
        {
            if (inventoryModel != null && bindableData != null)
            {
                inventoryModel.SortInventory(bindableData.activeSortTab);
            }
            // 鏄惧紡鍒锋柊缂撳瓨锛岄伩鍏嶄簨浠惰Е鍙戦『搴忓鑷翠笅娆I璇诲彇鍒版棫鏁版嵁
            RefreshDisplaySlots();
            selectedSlotIndex = -1;
            if (bindableData != null) bindableData.selectedSlotIndex = -1;
        }

        // 璋冪敤搴曞眰閲嶇疆閫昏緫 (娓呯┖鑳屽寘)
        public void ResetInventory()
        {
            if (inventoryModel != null)
            {
                foreach (var slot in inventoryModel.slots)
                    slot.Clear();
                // 鐢变簬Model灞備笉鍐嶆湁涓撻棬鐨凴eload鏂规硶锛岃繖閲屽彂閫佷竴娆″彉鍔ㄩ€氱煡鍗冲彲锛堜綘涔熷彲鍦∕odel灞傞澶栧姞ClearAll鏂规硶锛�
                inventoryModel.SortInventory(0); // 鏆備笖鐢⊿ort瑙﹀彂涓€涓嬫洿鏂颁簨浠�
            }
            selectedSlotIndex = -1;
            if (bindableData != null) bindableData.selectedSlotIndex = -1;
        }

        public void UseItem(int uiIndex)
        {
            if (uiIndex < 0) return;

            var slots = cachedDisplaySlots;
            if (uiIndex >= slots.Count || slots[uiIndex].IsEmpty) return;

            var targetSlot = slots[uiIndex];
            var itemData = targetSlot.itemData;

            // 标记最近使用时间，用于"按最近使用排序"
            targetSlot.MarkUsed();

            RPGLog.Debug("Inventory", $"使用物品: {itemData.itemName}");
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
