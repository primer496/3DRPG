using System;
using UnityEngine;
using FinalRPG.Utils;

/// <summary>
/// 玩家属性—纯数据类 (非 MonoBehaviour)。
/// 由 PlayerStatsProvider 创建并托管，外部通过 Provider.Stats 获取实例。
/// </summary>
[System.Serializable]
public class PlayerStats
{
    // ========== 可序列化字段（Inspector 中由 Provider 展示） ==========
    public int maxHP = 100;
    public int expToNextLevel = 100;

    public int CurrentHP { get; private set; }
    public int Exp        { get; private set; }
    public int Gold       { get; private set; }
    public int Level      { get; private set; } = 1;

    // ========== UI 绑定事件 ==========
    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnExpChanged;
    public event Action<int>       OnGoldChanged;
    public event Action<int>       OnLevelUp;

    // ========== 公开修改接口 ==========

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHP <= 0) return;
        CurrentHP -= amount;
        if (CurrentHP < 0) CurrentHP = 0;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        RPGLog.Debug("Player", $"-{amount} HP → {CurrentHP}/{maxHP}");
        if (CurrentHP <= 0) RPGLog.Debug("Player", "玩家死亡");
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHP <= 0) return;
        CurrentHP += amount;
        if (CurrentHP > maxHP) CurrentHP = maxHP;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;
        if (expToNextLevel <= 0) expToNextLevel = 100;   // 防序列化归零
        Exp += amount;
        RPGLog.Debug("Player", $"+{amount} Exp → {Exp}/{expToNextLevel} (Lv.{Level})");
        while (Exp >= expToNextLevel) { Exp -= expToNextLevel; LevelUp(); }
        OnExpChanged?.Invoke(Exp, expToNextLevel);
    }

    private void LevelUp()
    {
        Level++;
        expToNextLevel = Level * 100;
        maxHP += 20;
        CurrentHP = maxHP;
        OnLevelUp?.Invoke(Level);
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        OnExpChanged?.Invoke(Exp, expToNextLevel);
        RPGLog.Debug("Player", $"Level Up! Lv.{Level} HP {CurrentHP}/{maxHP} 下级 {expToNextLevel}");
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
        RPGLog.Debug("Player", $"+{amount} Gold → {Gold}");
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0 || Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public void Init()
    {
        CurrentHP = maxHP;
    }

    // ========== 存档接口 ==========

    /// <summary>
    /// 导出当前属性为存档数据结构（供 SaveSystem 调用）。
    /// </summary>
    public PlayerSaveData GetSaveData()
    {
        return new PlayerSaveData
        {
            currentHP = CurrentHP,
            maxHP = maxHP,
            exp = Exp,
            expToNextLevel = expToNextLevel,
            gold = Gold,
            level = Level
        };
    }

    /// <summary>
    /// 从存档数据恢复属性（供 SaveSystem 调用）。
    /// 恢复后触发所有 UI 绑定事件，确保界面同步刷新。
    /// </summary>
    public void RestoreFromSave(PlayerSaveData data)
    {
        if (data == null) return;

        maxHP = data.maxHP;
        expToNextLevel = data.expToNextLevel;
        CurrentHP = data.currentHP;
        Exp = data.exp;
        Gold = data.gold;
        Level = data.level;

        // 通知 UI 刷新
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        OnExpChanged?.Invoke(Exp, expToNextLevel);
        OnGoldChanged?.Invoke(Gold);
        OnLevelUp?.Invoke(Level);
    }
}
