using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FinalRPG.Utils;

namespace InventorySystem.Model
{
    public class InventoryModel
    {
        private int maxSlots;

        public List<InventorySlot> slots = new List<InventorySlot>();
        public event Action OnInventoryChanged;

        public InventoryModel(int maxSlots = 42)
        {
            this.maxSlots = maxSlots;
            InitEmptySlots();
        }

        private void InitEmptySlots()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        // 添加物品
        public bool AddItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;

            // 如果物品可以堆叠，处理相同物品的现有栈
            if (item.isStackable)
            {
                foreach (var slot in slots)
                {
                    if (!slot.IsEmpty && slot.itemData == item && slot.amount < item.maxStack)
                    {
                        int spaceLeft = item.maxStack - slot.amount;
                        int addAmount = Mathf.Min(spaceLeft, amount);
                        slot.amount += addAmount;
                        slot.lastUsedTime = UnityEngine.Time.time; // 更新最近获取时间
                        amount -= addAmount;

                        if (amount <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            // 如果还有剩余物品，尝试寻找新空格子存放
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    slot.itemData = item;
                    int addAmount = item.isStackable ? Mathf.Min(item.maxStack, amount) : 1;
                    slot.amount = addAmount;
                    slot.lastUsedTime = UnityEngine.Time.time; // 记录获得时间
                    amount -= addAmount;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            // 返回false意味着背包已经满了，存不下全部内容
            OnInventoryChanged?.Invoke();
            return false;
        }

        // 移除或消耗物品
        public void RemoveItem(int index, int amount)
        {
            if (index < 0 || index >= slots.Count) return;

            var slot = slots[index];
            if (slot.IsEmpty) return;

            slot.amount -= amount;
            if (slot.amount <= 0)
            {
                slot.Clear();
            }

            OnInventoryChanged?.Invoke();
        }

        // 交换格子的位置
        public void SwapItems(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= slots.Count) return;
            if (toIndex < 0 || toIndex >= slots.Count) return;
            if (fromIndex == toIndex) return;

            var fromSlot = slots[fromIndex];
            var toSlot = slots[toIndex];

            // 存成临时变量交换
            var tempItem = fromSlot.itemData;
            var tempAmount = fromSlot.amount;
            var tempLastUsedTime = fromSlot.lastUsedTime;

            fromSlot.itemData = toSlot.itemData;
            fromSlot.amount = toSlot.amount;
            fromSlot.lastUsedTime = toSlot.lastUsedTime;

            toSlot.itemData = tempItem;
            toSlot.amount = tempAmount;
            toSlot.lastUsedTime = tempLastUsedTime;

            OnInventoryChanged?.Invoke();
        }

        // ========== 存档接口 ==========

        /// <summary>
        /// 导出当前背包内容为存档数据列表（仅保存非空格子）。
        /// </summary>
        public List<InventorySlotSaveData> GetSaveData()
        {
            var result = new List<InventorySlotSaveData>();
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    result.Add(new InventorySlotSaveData
                    {
                        itemId = slot.itemData.itemID,
                        amount = slot.amount,
                        lastUsedTime = slot.lastUsedTime
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// 从存档数据恢复背包内容。
        /// 先清空所有槽位，然后根据 itemId 从 Resources 加载 ItemData 还原。
        /// </summary>
        public void LoadFromSave(List<InventorySlotSaveData> data)
        {
            // 重置所有槽位
            foreach (var slot in slots)
            {
                slot.Clear();
            }

            if (data == null || data.Count == 0)
            {
                OnInventoryChanged?.Invoke();
                return;
            }

            int slotIndex = 0;
            foreach (var saved in data)
            {
                if (slotIndex >= slots.Count) break;
                if (string.IsNullOrEmpty(saved.itemId))
                {
                    RPGLog.Warning("Inventory", "Load: 存档中存在空 itemId，已跳过");
                    continue;
                }

                var itemData = Resources.Load<ItemData>($"GameConfigs/PackageModel/{saved.itemId}");
                if (itemData != null)
                {
                    slots[slotIndex].itemData = itemData;
                    slots[slotIndex].amount = saved.amount;
                    slots[slotIndex].lastUsedTime = saved.lastUsedTime;
                    slotIndex++;
                }
                else
                {
                    RPGLog.Warning("Inventory", $"Load: 无法找到 ItemData: {saved.itemId}，已跳过");
                }
            }

            OnInventoryChanged?.Invoke();
        }

        // 整理背包 (0: 稀有度, 1: 数量/近期)
        public void SortInventory(int mode)
        {
            // 使用值元组创建数据快照，避免原地排序时引用被覆盖导致数据损坏
            var validSnapshots = slots
                .Where(s => !s.IsEmpty)
                .Select(s => (itemData: s.itemData, amount: s.amount, lastUsedTime: s.lastUsedTime))
                .ToList();

            if (mode == 0) // 按稀有度降序
            {
                validSnapshots = validSnapshots.OrderByDescending(s => s.itemData.rarity)
                                                 .ThenBy(s => s.itemData.itemID).ToList();
            }
            else // 按最近使用降序（最近使用的排前面）
            {
                validSnapshots = validSnapshots.OrderByDescending(s => s.lastUsedTime)
                                                 .ThenBy(s => s.itemData.itemID).ToList();
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (i < validSnapshots.Count)
                {
                    slots[i].itemData = validSnapshots[i].itemData;
                    slots[i].amount = validSnapshots[i].amount;
                    slots[i].lastUsedTime = validSnapshots[i].lastUsedTime;
                }
                else
                {
                    slots[i].Clear();
                }
            }
            OnInventoryChanged?.Invoke();
        }
    }
}
