using UnityEngine;

namespace FinalRPG.Utils
{
    /// <summary>
    /// 项目级日志封装 — 替代裸调 Debug.Log*。
    ///
    /// 用法：
    ///   RPGLog.Debug("Combat", "命中敌人");
    ///   RPGLog.Warning("Save", "存档不存在");
    ///   RPGLog.Error("Quest", "任务数据为空");
    ///
    /// 频道开关：
    ///   RPGLog.Combat = false;  // 一秒关闭战斗日志
    ///   也可通过 RPGLogSettings (ScriptableObject) 在 Inspector 批量配置，
    ///   启动时由 SaveSystem 自动加载并同步。
    ///
    /// 发布版行为：
    ///   Debug 级别 — [Conditional] 剥离整个调用点（含字符串插值），零 GC 零开销
    ///   Warning / Error — 始终保留，用于线上诊断
    /// </summary>
    public static class RPGLog
    {
        // ================================================================
        // 频道开关（默认全开，启动时由 RPGLogSettings 覆盖）
        // ================================================================

        public static bool Combat    = true;
        public static bool Save      = true;
        public static bool Quest     = true;
        public static bool Dialogue  = true;
        public static bool HSM       = true;
        public static bool Player    = true;
        public static bool Inventory = true;
        public static bool UI        = true;

        // ================================================================
        // 公开 API
        // ================================================================

        /// <summary>
        /// 调试日志。仅 Editor / Development Build 中生效，受频道开关控制。
        /// 发布版中调用点和参数求值（含字符串插值）均由编译器完全剥离，零 GC。
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string channel, string message)
        {
            if (!IsChannelEnabled(channel)) return;
            UnityEngine.Debug.Log($"[{channel}] {message}");
        }

        /// <summary>
        /// 警告日志。所有构建版本均保留，不受频道开关影响。
        /// </summary>
        public static void Warning(string channel, string message)
        {
            UnityEngine.Debug.LogWarning($"[{channel}] {message}");
        }

        /// <summary>
        /// 错误日志。所有构建版本均保留，不受频道开关影响。
        /// </summary>
        public static void Error(string channel, string message)
        {
            UnityEngine.Debug.LogError($"[{channel}] {message}");
        }

        // ================================================================
        // 内部
        // ================================================================

        private static bool IsChannelEnabled(string channel)
        {
            switch (channel)
            {
                case "Combat":    return Combat;
                case "Save":      return Save;
                case "Quest":     return Quest;
                case "Dialogue":  return Dialogue;
                case "HSM":       return HSM;
                case "Player":    return Player;
                case "Inventory": return Inventory;
                case "UI":        return UI;
                default:          return true; // 未知频道默认放行
            }
        }

        /// <summary>
        /// 从 RPGLogSettings SO 同步频道开关。由 SaveSystem.Start() 调用。
        /// </summary>
        public static void ApplySettings(RPGLogSettings settings)
        {
            if (settings == null) return;
            Combat    = settings.combatEnabled;
            Save      = settings.saveEnabled;
            Quest     = settings.questEnabled;
            Dialogue  = settings.dialogueEnabled;
            HSM       = settings.hsmEnabled;
            Player    = settings.playerEnabled;
            Inventory = settings.inventoryEnabled;
            UI        = settings.uiEnabled;
        }
    }
}
