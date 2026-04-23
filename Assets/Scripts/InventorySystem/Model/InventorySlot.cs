using System;

namespace InventorySystem.Model
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData itemData;
        public int amount;

        public bool IsEmpty => itemData == null || amount <= 0;

        public void Clear()
        {
            itemData = null;
            amount = 0;
        }
    }
}
