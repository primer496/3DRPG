using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TaskManager;
using FinalRPG.Utils;

/// <summary>
/// 伤害跳字控制器 — 独立的屏幕空间 UI 叠加层。
/// 订阅 EventBus.OnDamagePopup，在目标世界坐标处生成上飘淡出的伤害数字。
/// 使用 Update 轮询驱动动画（不用 async/await），内置简易 Label 对象池。
/// 挂载在场景中独立的 FloatingTextCanvas GameObject 上。
/// </summary>
public class FloatingTextController : MonoBehaviour
{
    // ========== 内部数据结构 ==========

    private struct FloatingTextInstance
    {
        public Label label;
        public float elapsed;
        public Vector2 startScreenPos;
    }

    // ========== 可调参数 ==========

    [Header("动画参数")]
    [SerializeField] private float _duration = 1.0f;
    [SerializeField] private float _floatDistance = 60f;

    [Header("世界偏移（头顶定位）")]
    [Tooltip("角色脚底到头顶的世界 Y 轴高度。根据角色模型调整，默认 2.0m")]
    [SerializeField] private float _headHeight = 2.0f;
    [Tooltip("X 轴微小随机散布（米），防止同一位置多次命中数字完全重叠")]
    [SerializeField] private float _randomSpreadX = 0.3f;

    [Header("对象池")]
    [SerializeField] private int _poolInitialSize = 5;

    // ========== 运行时状态 ==========

    private UIDocument _uiDoc;
    private VisualElement _root;
    private Queue<Label> _pool;
    private List<FloatingTextInstance> _activeTexts = new List<FloatingTextInstance>();

    // ========== Unity 生命周期 ==========

    private void Awake()
    {
        _uiDoc = GetComponent<UIDocument>();
        _pool = new Queue<Label>(_poolInitialSize);
    }

    private void OnEnable()
    {
        CacheRoot();
        EventBus.Instance.OnDamagePopup += SpawnFloatingText;
    }

    private void Start()
    {
        // 兜底：OnEnable 时 UIDocument 可能尚未加载完 UXML
        if (_root == null)
        {
            RPGLog.Warning("UI", "OnEnable 时 _root 为 null，Start 中重试");
            CacheRoot();
        }
    }

    private void OnDisable()
    {
        EventBus.Instance.OnDamagePopup -= SpawnFloatingText;
    }

    private void Update()
    {
        UpdateActiveTexts();
    }

    // ========== 初始化 ==========

    private void CacheRoot()
    {
        if (_uiDoc == null)
        {
            RPGLog.Error("UI", "UIDocument 组件未找到！");
            return;
        }
        _root = _uiDoc.rootVisualElement;
        if (_root == null)
            RPGLog.Error("UI", "rootVisualElement 为 null，UXML 可能未加载");
        else
            RPGLog.Debug("UI", "初始化完成，根节点就绪");
    }

    // ========== 生成跳字 ==========

    private void SpawnFloatingText(int amount, Vector3 worldPos)
    {
        if (_root == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // 世界坐标 → 头顶位置 + 微小 X 散布
        Vector3 headPos = worldPos;
        headPos.y += _headHeight;
        headPos.x += Random.Range(-_randomSpreadX, _randomSpreadX);

        // UI Toolkit 原生坐标转换（处理 PanelSettings 缩放）
        Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, headPos, cam);

        // 过滤屏幕外/背后的点
        if (panelPos.x < -100 || panelPos.y < -100 ||
            panelPos.x > Screen.width + 100 || panelPos.y > Screen.height + 100)
            return;

        Label label = GetFromPool();
        label.text = amount.ToString();
        label.style.position = Position.Absolute;
        label.style.left = panelPos.x;
        label.style.top = panelPos.y;
        label.style.opacity = 1f;
        _root.Add(label);

        _activeTexts.Add(new FloatingTextInstance
        {
            label = label,
            elapsed = 0f,
            startScreenPos = panelPos
        });
    }

    // ========== 动画更新（Update 中调用） ==========

    private void UpdateActiveTexts()
    {
        if (_activeTexts.Count == 0) return;

        float dt = Time.deltaTime;
        float invDuration = _duration > 0f ? 1f / _duration : 1f;

        for (int i = _activeTexts.Count - 1; i >= 0; i--)
        {
            FloatingTextInstance instance = _activeTexts[i];
            instance.elapsed += dt;

            float t = instance.elapsed * invDuration;

            if (t >= 1f)
            {
                // 动画完成，回池
                ReturnToPool(instance.label);
                _activeTexts.RemoveAt(i);
                continue;
            }

            // 上飘 + 淡出
            float yOffset = _floatDistance * t;
            instance.label.style.left = instance.startScreenPos.x;
            instance.label.style.top = instance.startScreenPos.y - yOffset;
            instance.label.style.opacity = 1f - t;

            // 写回结构体
            _activeTexts[i] = instance;
        }
    }

    // ========== 对象池 ==========

    private Label GetFromPool()
    {
        Label label;
        if (_pool.Count > 0)
        {
            label = _pool.Dequeue();
        }
        else
        {
            label = new Label();
            label.AddToClassList("damage-text");
            label.pickingMode = PickingMode.Ignore;
            // 内联兜底样式（USS 未加载时仍可见）
            label.style.color = new Color(0.86f, 0.24f, 0.24f);
            label.style.fontSize = 22;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
        }
        return label;
    }

    private void ReturnToPool(Label label)
    {
        if (label == null) return;
        label.RemoveFromHierarchy();
        label.text = string.Empty;
        label.style.opacity = 1f;
        _pool.Enqueue(label);
    }
}
