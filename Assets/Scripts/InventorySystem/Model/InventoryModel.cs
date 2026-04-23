using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventorySystem.Model
{
    public class InventoryModel : MonoBehaviour
    {
        [SerializeField] private int maxSlots = 42; // UXML有42个格子(ItemSlot0 - ItemSlot41)
        
        [Header("Runtime Info")]
        public List<InventorySlot> slots = new List<InventorySlot>();

        [Header("Test Items")]
        public List<ItemData> initialTestItems = new List<ItemData>();
        public int testItemAmount = 5;
        public bool autoLoadFromPath = true; // 是否自动按路径加载

        public event Action OnInventoryChanged;

        private void Awake()
        {
            // 初始化空槽位
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (autoLoadFromPath)
            {
                string path = "Assets/GameConfigs/PackageModel";
                string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { path });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                    // 过滤掉未配置完整（例如没有ID或名字）的默认或空物品数据
                    if (item != null && !string.IsNullOrEmpty(item.itemID) && !initialTestItems.Contains(item))
                    {
                        initialTestItems.Add(item);
                    }
                }
            }
#endif
            // 添加测试物品
            foreach (var item in initialTestItems)
            {
                if (item != null)
                {
                    AddItem(item, testItemAmount);
                }
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

            fromSlot.itemData = toSlot.itemData;
            fromSlot.amount = toSlot.amount;

            toSlot.itemData = tempItem;
            toSlot.amount = tempAmount;

            OnInventoryChanged?.Invoke();
        }

        // 整理背包 (0: 稀有度, 1: 数量/近期)
        public void SortInventory(int mode)
        {
            var validItems = slots.Where(s => !s.IsEmpty).ToList();

            if (mode == 0) // 按稀有度降序
            {
                validItems = validItems.OrderByDescending(s => s.itemData.rarity)
                                         .ThenBy(s => s.itemData.itemID).ToList();
            }
            else // 按数量降序（或者是默认的获取近期算法）
            {
                validItems = validItems.OrderByDescending(s => s.amount)
                                         .ThenBy(s => s.itemData.itemID).ToList();
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (i < validItems.Count)
                {
                    slots[i].itemData = validItems[i].itemData;
                    slots[i].amount = validItems[i].amount;
                }
                else
                {
                    slots[i].Clear();
                }
            }
            OnInventoryChanged?.Invoke();
        }

        // 重新加载测试数据 (对应重置功能)
        public void ReloadInitialItems()
        {
            foreach (var slot in slots) 
                slot.Clear();

            foreach (var item in initialTestItems)
            {
                if (item != null) AddItem(item, testItemAmount);
            }
            OnInventoryChanged?.Invoke();
        }
    }
}
