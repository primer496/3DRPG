using System;

namespace InventorySystem.Model
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData itemData;
        public int amount;
        public float lastUsedTime; // 最近使用时间戳，用于"按最近使用排序"

        public bool IsEmpty => itemData == null || amount <= 0;

        public void Clear()
        {
            itemData = null;
            amount = 0;
            lastUsedTime = 0f;
        }

        /// <summary>
        /// 标记此槽位为"刚使用过"，更新时间为当前游戏时间
        /// </summary>
        public void MarkUsed()
        {
            lastUsedTime = UnityEngine.Time.time;
        }
    }
}
