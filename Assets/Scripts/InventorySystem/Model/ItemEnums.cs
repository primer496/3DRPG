namespace InventorySystem.Model
{
    public enum ItemCategory
    {
        Item,       // 物品背包
        Equipment,  // 装备背包
        QuestItem,  // 任务物品
        Consumable, // 消耗品
        Material,   // 材料
        Other       // 其他
    }

    public enum ItemRarity
    {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }
}