using UnityEngine;

namespace FinalRPG.Utils
{
    /// <summary>
    /// RPGLog 频道开关的 ScriptableObject 配置。
    /// 放在 Assets/Resources/GameConfigs/ 下，运行时由 SaveSystem 自动加载。
    ///
    /// 创建方式：Assets → Create → RPG → Log Settings
    /// </summary>
    [CreateAssetMenu(menuName = "RPG/Log Settings", fileName = "RPGLogSettings")]
    public class RPGLogSettings : ScriptableObject
    {
        [Header("频道开关 — 勾选 = 输出 Debug 日志")]
        [Tooltip("战斗：武器检测、伤害、敌人血量、跳字")]
        public bool combatEnabled = true;

        [Tooltip("存档：读档、存档、数据恢复")]
        public bool saveEnabled = true;

        [Tooltip("任务：接受、完成、目标推进")]
        public bool questEnabled = true;

        [Tooltip("对话：Yarn 适配器、Presenter")]
        public bool dialogueEnabled = true;

        [Tooltip("状态机：Activity 切换、TransitionSequencer、状态检测")]
        public bool hsmEnabled = true;

        [Tooltip("玩家属性：HP/Exp/Gold 变化")]
        public bool playerEnabled = true;

        [Tooltip("背包：物品增删、VM 变化")]
        public bool inventoryEnabled = true;

        [Tooltip("UI 诊断：FloatingText、UI 初始化")]
        public bool uiEnabled = true;
    }
}
