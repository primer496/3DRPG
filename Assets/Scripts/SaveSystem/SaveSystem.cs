using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Yarn.Unity;
using TaskManager;
using InventorySystem.Model;
using InventorySystem.ViewModel;
using FinalRPG.Utils;

/// <summary>
/// 存档系统核心协调器 — MonoBehaviour 单例（DontDestroyOnLoad）。
///
/// 负责：
/// - 收集各子系统数据 → SaveData → JsonUtility.ToJson → 写入文件
/// - 从文件读取 JSON → 分发给各子系统恢复状态
/// - 3 个手动槽位 (slot 0-2) + 1 个自动存档 (autosave.json)
/// - 通过 EventBus string 事件 "AutoSave" 响应自动存档请求
///
/// 文件路径：Application.persistentDataPath/save_slot_{slot}.json / autosave.json
/// </summary>
public class SaveSystem : MonoBehaviour
{
    private const string SAVE_FILE_PREFIX = "save_slot_";
    private const string AUTOSAVE_FILE = "autosave";
    private const string FILE_EXTENSION = ".json";

    // ========================================================================
    // 单例
    // ========================================================================
    private static SaveSystem _instance;

    /// <summary>
    /// 游戏启动时自动创建 SaveSystem 单例，无需场景中手动放置。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        // 访问 Instance 属性即可触发懒加载创建
        _ = Instance;
    }

    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[SaveSystem]");
                _instance = go.AddComponent<SaveSystem>();
            }
            return _instance;
        }
    }

    // ========================================================================
    // 异步工具
    // ========================================================================
    private readonly AsyncRunner _asyncRunner = new AsyncRunner();

    // ========================================================================
    // Unity 生命周期
    // ========================================================================
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 加载日志频道配置（需在 Unity 中手动创建:
        //   Assets → Create → RPG → Log Settings → 放入 Resources/GameConfigs/）
        var logSettings = Resources.Load<RPGLogSettings>("GameConfigs/RPGLogSettings");
        if (logSettings != null)
        {
            RPGLog.ApplySettings(logSettings);
        }

        CacheReferences();

        // 启动时异步恢复存档，避免 File.ReadAllText 阻塞首帧
        if (HasAutoSave())
        {
            LoadAutoSaveAsync();
        }
    }

    private void Update()
    {
        _asyncRunner.Tick();
    }

    private void OnApplicationQuit()
    {
        // 退出时自动保存
        AutoSave();
    }

    private void OnEnable()
    {
        // 使用 EventBus 已有的 string-based 事件字典，无需修改 TaskManager 命名空间
        EventBus.Instance.Subscribe("AutoSave", HandleAutoSaveRequest);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe("AutoSave", HandleAutoSaveRequest);
    }

    // ========================================================================
    // 外部引用缓存
    // ========================================================================
    private PlayerStatsProvider _playerStatsProvider;
    private InventoryViewModel _inventoryViewModel;
    private InMemoryVariableStorage _yarnStorage;

    /// <summary>
    /// 延迟查找场景中的必需组件。若场景尚未加载完毕则静默失败，
    /// 后续每次 Save/Load 前会重试查找。
    /// </summary>
    private void CacheReferences()
    {
        if (_playerStatsProvider == null)
        {
            _playerStatsProvider = FindObjectOfType<PlayerStatsProvider>();
        }
        if (_inventoryViewModel == null)
        {
            _inventoryViewModel = FindObjectOfType<InventoryViewModel>();
        }
        if (_yarnStorage == null)
        {
            _yarnStorage = FindFirstObjectByType<InMemoryVariableStorage>();
        }
    }

    /// <summary>
    /// 需要持久化的 Yarn 变量定义：变量名 → 是否为 bool 类型。
    /// true = Yarn 中为布尔值（<<set $Var = true>>），false = 数值（<<set $Var = 1>>）。
    /// 新增 Yarn 变量时在此追加即可，避免类型推断异常。
    /// </summary>
    private static readonly Dictionary<string, bool> YarnVarDefs = new Dictionary<string, bool>
    {
        { "$QuestCompleted", true },
        { "$QuestAccepted", true },
        { "$QuestRejected", true },
        { "$InvestigationProgress", false },
        { "$Phase2Accepted", true }
    };

    // ========================================================================
    // 公开 API：异步读档
    // ========================================================================

    /// <summary>
    /// 从指定槽位 (0-2) 异步加载存档。
    /// File.ReadAllText 在后台线程执行，避免阻塞主线程；
    /// 解析和恢复在主线程回调中完成。加载期间锁定玩家输入。
    /// </summary>
    public void LoadGameAsync(int slot)
    {
        if (slot < 0 || slot > 2)
        {
            RPGLog.Error("Save", $"无效存档槽位: {slot}，合法值为 0-2");
            return;
        }

        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            RPGLog.Warning("Save", $"存档不存在 槽位 {slot}: {path}");
            return;
        }

        EventBus.Instance.RaiseInputLock(true);

        string json = null;
        _asyncRunner.RunSequential(new Func<CancellationToken, Task>[] {
            // Step 1: 线程池读取文件，不阻塞主线程
            ct => Task.Run(() => {
                json = File.ReadAllText(path);
            }, ct),
            // Step 2: 主线程解析 JSON + 恢复游戏状态
            ct => {
                var saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData == null)
                {
                    RPGLog.Error("Save", $"存档解析失败 槽位 {slot}");
                    EventBus.Instance.RaiseInputLock(false);
                    return Task.CompletedTask;
                }

                CacheReferences();
                RestoreFromSaveData(saveData);
                EventBus.Instance.RaiseInputLock(false);
                RPGLog.Debug("Save", $"读档成功 ← 槽位 {slot}: {saveData.saveTime}");
                return Task.CompletedTask;
            }
        });
    }

    /// <summary>
    /// 异步加载自动存档。File.ReadAllText 在后台线程执行。
    /// </summary>
    public void LoadAutoSaveAsync()
    {
        string path = GetAutoSavePath();
        if (!File.Exists(path))
        {
            RPGLog.Warning("Save", $"自动存档不存在: {path}");
            return;
        }

        EventBus.Instance.RaiseInputLock(true);

        string json = null;
        _asyncRunner.RunSequential(new Func<CancellationToken, Task>[] {
            ct => Task.Run(() => {
                json = File.ReadAllText(path);
            }, ct),
            ct => {
                var saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData == null)
                {
                    RPGLog.Error("Save", "自动存档解析失败");
                    EventBus.Instance.RaiseInputLock(false);
                    return Task.CompletedTask;
                }

                CacheReferences();
                RestoreFromSaveData(saveData);
                EventBus.Instance.RaiseInputLock(false);
                RPGLog.Debug("Save", $"读档成功 ← 自动存档: {saveData.saveTime}");
                return Task.CompletedTask;
            }
        });
    }

    // ========================================================================
    // 公开 API：自动存档
    // ========================================================================

    /// <summary>
    /// 自动存档（写入 autosave.json）。
    /// 通常由 Yarn 命令 &lt;&lt;AutoSave&gt;&gt; 或关键剧情节点触发。
    /// </summary>
    public void AutoSave()
    {
        CacheReferences();

        var saveData = CollectSaveData();
        saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string json = JsonUtility.ToJson(saveData, prettyPrint: true);
        string path = GetAutoSavePath();

        try
        {
            File.WriteAllText(path, json);
            RPGLog.Debug("Save", $"自动存档完成: {path} ({saveData.saveTime})");
        }
        catch (Exception e)
        {
            RPGLog.Error("Save", $"自动存档写入失败: {e.Message}");
        }
    }

    // ========================================================================
    // 公开 API：存档管理
    // ========================================================================

    /// <summary>
    /// 检查指定槽位 (0-2) 是否存在存档文件。
    /// </summary>
    public bool HasSave(int slot)
    {
        if (slot < 0 || slot > 2) return false;
        return File.Exists(GetSlotPath(slot));
    }

    /// <summary>
    /// 检查自动存档是否存在。
    /// </summary>
    public bool HasAutoSave()
    {
        return File.Exists(GetAutoSavePath());
    }

    /// <summary>
    /// 获取槽位存档信息（仅读取 saveTime，不加载完整数据）。
    /// 供存档选择 UI 展示时间戳。无存档时返回 null。
    /// </summary>
    public string GetSlotInfo(int slot)
    {
        if (slot < 0 || slot > 2) return null;
        string path = GetSlotPath(slot);
        return GetSaveTimeFromFile(path);
    }

    /// <summary>
    /// 获取自动存档信息。
    /// </summary>
    public string GetAutoSaveInfo()
    {
        return GetSaveTimeFromFile(GetAutoSavePath());
    }

    /// <summary>
    /// 删除指定槽位 (0-2) 的存档文件。
    /// </summary>
    public void DeleteSave(int slot)
    {
        if (slot < 0 || slot > 2) return;
        string path = GetSlotPath(slot);
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                RPGLog.Debug("Save", $"已删除存档 槽位 {slot}");
            }
        }
        catch (Exception e)
        {
            RPGLog.Error("Save", $"删除存档失败 槽位 {slot}: {e.Message}");
        }
    }

    /// <summary>
    /// 删除自动存档。
    /// </summary>
    public void DeleteAutoSave()
    {
        string path = GetAutoSavePath();
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                RPGLog.Debug("Save", "已删除自动存档");
            }
        }
        catch (Exception e)
        {
            RPGLog.Error("Save", $"删除自动存档失败: {e.Message}");
        }
    }

    // ========================================================================
    // 私有：数据收集 & 恢复
    // ========================================================================

    private SaveData CollectSaveData()
    {
        var saveData = new SaveData();

        // 玩家属性
        if (_playerStatsProvider != null && _playerStatsProvider.Stats != null)
        {
            saveData.player = _playerStatsProvider.Stats.GetSaveData();
        }

        // 背包（仅保存非空格子）
        if (_inventoryViewModel != null && _inventoryViewModel.inventoryModel != null)
        {
            saveData.inventorySlots = _inventoryViewModel.inventoryModel.GetSaveData();
        }

        // 任务进度（仅保存未完成任务，避免已完成任务累积）
        var activeQuests = QuestManager.Instance.activeQuests;
        if (activeQuests != null)
        {
            foreach (var quest in activeQuests)
            {
                if (quest.isCompleted) continue; // 已完成任务不存，避免读档后重复累积
                saveData.quests.Add(new QuestSaveData
                {
                    questId = quest.questData.id,
                    progressList = new List<int>(quest.progressList),
                    currentActiveIndex = quest.currentActiveIndex,
                    isCompleted = quest.isCompleted
                });
            }
        }

        // Yarn 对话变量（持久化对话进度状态）
        if (_yarnStorage != null)
        {
            foreach (var kvp in YarnVarDefs)
            {
                string varName = kvp.Key;
                bool isBool = kvp.Value;

                if (isBool)
                {
                    if (_yarnStorage.TryGetValue<bool>(varName, out bool boolVal))
                    {
                        saveData.yarnVariables.Add(new YarnVariableEntry
                        {
                            name = varName,
                            value = boolVal ? 1f : 0f,
                            wasBool = true
                        });
                    }
                }
                else
                {
                    if (_yarnStorage.TryGetValue<float>(varName, out float floatVal))
                    {
                        saveData.yarnVariables.Add(new YarnVariableEntry
                        {
                            name = varName,
                            value = floatVal,
                            wasBool = false
                        });
                    }
                }
            }
        }

        return saveData;
    }

    private void RestoreFromSaveData(SaveData data)
    {
        // 1. 恢复玩家属性
        if (_playerStatsProvider != null && _playerStatsProvider.Stats != null)
        {
            _playerStatsProvider.Stats.RestoreFromSave(data.player);
        }
        else
        {
            RPGLog.Warning("Save", "PlayerStatsProvider 未找到，跳过属性恢复");
        }

        // 2. 恢复背包
        if (_inventoryViewModel != null && _inventoryViewModel.inventoryModel != null)
        {
            _inventoryViewModel.inventoryModel.LoadFromSave(data.inventorySlots);
        }
        else
        {
            RPGLog.Warning("Save", "InventoryViewModel 未找到，跳过背包恢复");
        }

        // 3. 恢复任务进度
        RestoreQuestData(data.quests);

        // 4. 恢复 Yarn 对话变量（确保对话从正确分支开始）
        if (_yarnStorage != null && data.yarnVariables != null)
        {
            foreach (var entry in data.yarnVariables)
            {
                if (entry.wasBool)
                {
                    _yarnStorage.SetValue(entry.name, entry.value != 0f);
                }
                else
                {
                    _yarnStorage.SetValue(entry.name, entry.value);
                }
            }
            RPGLog.Debug("Save", $"Yarn 变量恢复完成: 共 {data.yarnVariables.Count} 个");
        }
    }

    private void RestoreQuestData(List<QuestSaveData> data)
    {
        var activeQuests = QuestManager.Instance.activeQuests;
        activeQuests.Clear();

        if (data == null || data.Count == 0) return;

        // 构建 questId → QuestData 字典（从 Resources 加载所有 QuestData SO）
        var allQuestData = Resources.LoadAll<QuestData>("GameConfigs/Quest");
        var questDataDict = new Dictionary<string, QuestData>();
        foreach (var qd in allQuestData)
        {
            if (!string.IsNullOrEmpty(qd.id))
            {
                questDataDict[qd.id] = qd;
            }
        }

        foreach (var saved in data)
        {
            if (string.IsNullOrEmpty(saved.questId))
            {
                RPGLog.Warning("Save", "存档中存在空 questId，已跳过");
                continue;
            }

            if (!questDataDict.TryGetValue(saved.questId, out var questData))
            {
                RPGLog.Warning("Save", $"无法找到 QuestData: {saved.questId}，已跳过");
                continue;
            }

            var instance = new QuestInstance(questData);

            // 恢复进度列表（处理长度不匹配的边界情况）
            int progressCount = Mathf.Min(saved.progressList.Count, instance.progressList.Count);
            for (int i = 0; i < progressCount; i++)
            {
                instance.progressList[i] = saved.progressList[i];
            }

            instance.currentActiveIndex = Mathf.Clamp(saved.currentActiveIndex, 0, questData.objectives.Count);
            instance.isCompleted = saved.isCompleted;

            activeQuests.Add(instance);
        }

        // 通过 EventBus string 事件通知 UI 刷新（QuestViewModel 可订阅 "QuestsRestored"）
        EventBus.Instance.Raise("QuestsRestored");
        RPGLog.Debug("Save", $"任务进度恢复完成: 共 {activeQuests.Count} 个任务");
    }

    // ========================================================================
    // 私有：文件路径 & 工具
    // ========================================================================

    private static string GetSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_PREFIX}{slot}{FILE_EXTENSION}");
    }

    private static string GetAutoSavePath()
    {
        return Path.Combine(Application.persistentDataPath, $"{AUTOSAVE_FILE}{FILE_EXTENSION}");
    }

    /// <summary>
    /// 从存档文件中仅读取 saveTime 字段（轻量操作，不解析完整 SaveData）。
    /// </summary>
    private static string GetSaveTimeFromFile(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            return data?.saveTime;
        }
        catch
        {
            return null;
        }
    }

    // ========================================================================
    // EventBus 回调
    // ========================================================================
    private void HandleAutoSaveRequest()
    {
        AutoSave();
    }
}
