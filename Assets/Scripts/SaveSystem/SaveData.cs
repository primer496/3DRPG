using System;
using System.Collections.Generic;

/// <summary>
/// 玩家属性存档数据。
/// 对应 PlayerStats 中需要持久化的字段。
/// </summary>
[Serializable]
public class PlayerSaveData
{
    public int currentHP;
    public int maxHP;
    public int exp;
    public int expToNextLevel;
    public int gold;
    public int level;
}

/// <summary>
/// 背包槽位存档数据。
/// 仅保存非空格子的 itemID（字符串）、数量和最近使用时间。
/// 加载时通过 Resources.Load&lt;ItemData&gt; 按 itemID 还原 ScriptableObject 引用。
/// </summary>
[Serializable]
public class InventorySlotSaveData
{
    public string itemId;
    public int amount;
    public float lastUsedTime;
}

/// <summary>
/// 任务存档数据。
/// 对应 QuestInstance 的运行时状态。
/// questId 用于匹配 QuestData.id 还原 ScriptableObject 引用。
/// </summary>
[Serializable]
public class QuestSaveData
{
    public string questId;
    public List<int> progressList;
    public int currentActiveIndex;
    public bool isCompleted;
}

/// <summary>
/// Yarn 对话变量存档数据（键值对）。
/// 用于持久化 InMemoryVariableStorage 中的变量，
/// 确保读档后对话从正确的分支节点开始。
/// </summary>
[Serializable]
public class YarnVariableEntry
{
    public string name;
    public float value;
    public bool wasBool; // Yarn 中的 bool 变量以 bool 类型存储，需标记以正确还原
}

/// <summary>
/// 存档根数据容器。
/// 由 SaveSystem 序列化为 JSON 写入 persistentDataPath。
/// </summary>
[Serializable]
public class SaveData
{
    public string saveTime;
    public PlayerSaveData player = new PlayerSaveData();
    public List<InventorySlotSaveData> inventorySlots = new List<InventorySlotSaveData>();
    public List<QuestSaveData> quests = new List<QuestSaveData>();
    public List<YarnVariableEntry> yarnVariables = new List<YarnVariableEntry>();
}
