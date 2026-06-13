using UnityEngine;
using UnityEngine.UIElements;
using TaskManager;

/// <summary>
/// 玩家 HUD 控制器。
/// 订阅 PlayerStatsProvider.Stats 事件驱动 UI。
/// 监听 EventBus.OnInputLockStateChanged：任何面板/对话打开时自动隐藏。
/// </summary>
public class PlayerHUDController : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;

    private ProgressBar _hpBar;
    private ProgressBar _expBar;
    private Label _levelLabel;
    private Label _goldLabel;

    private PlayerStats _stats;
    private bool _locked;

    private void Awake()
    {
        _uiDoc = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        var provider = FindFirstObjectByType<PlayerStatsProvider>();
        _stats = provider != null ? provider.Stats : null;

        if (_stats != null)
        {
            _stats.OnHPChanged  += RefreshHP;
            _stats.OnExpChanged += RefreshExp;
            _stats.OnGoldChanged += RefreshGold;
            _stats.OnLevelUp    += RefreshLevel;
        }

        EventBus.Instance.OnInputLockStateChanged += OnInputLockChanged;

        CacheElements();
        FullRefresh();
    }

    private void OnDisable()
    {
        if (_stats != null)
        {
            _stats.OnHPChanged  -= RefreshHP;
            _stats.OnExpChanged -= RefreshExp;
            _stats.OnGoldChanged -= RefreshGold;
            _stats.OnLevelUp    -= RefreshLevel;
            _stats = null;
        }

        EventBus.Instance.OnInputLockStateChanged -= OnInputLockChanged;
    }

    private void OnInputLockChanged(bool locked)
    {
        _locked = locked;
        ApplyVisibility();
    }

    /// <summary>LateUpdate 兜底：消除事件顺序导致的偶发同帧闪烁。</summary>
    private void LateUpdate()
    {
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (_root != null)
            _root.style.display = _locked ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void CacheElements()
    {
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;
        if (_root == null) return;

        _hpBar      = _root.Q<ProgressBar>("hp-bar");
        _expBar     = _root.Q<ProgressBar>("exp-bar");
        _levelLabel = _root.Q<Label>("level-label");
        _goldLabel  = _root.Q<Label>("gold-label");
    }

    private void FullRefresh()
    {
        if (_stats == null) return;
        RefreshHP(_stats.CurrentHP, _stats.maxHP);
        RefreshExp(_stats.Exp, _stats.expToNextLevel);
        RefreshGold(_stats.Gold);
        RefreshLevel(_stats.Level);
    }

    private void RefreshHP(int current, int max)
    {
        if (_hpBar == null) return;
        _hpBar.value = max > 0 ? (float)current / max * 100f : 0f;
        _hpBar.title = $"{current}/{max}";
    }

    private void RefreshExp(int exp, int expToNext)
    {
        if (_expBar == null) return;
        _expBar.value = expToNext > 0 ? (float)exp / expToNext * 100f : 0f;
        _expBar.title = $"{exp}/{expToNext}";
    }

    private void RefreshGold(int gold)
    {
        if (_goldLabel != null)
            _goldLabel.text = gold.ToString();
    }

    private void RefreshLevel(int level)
    {
        if (_levelLabel != null)
            _levelLabel.text = $"Lv.{level}";
    }
}
