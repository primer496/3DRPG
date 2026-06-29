using UnityEngine;
using UnityEngine.UIElements;
using HSM;
using TaskManager;

namespace FinalRPG.UI
{
    /// <summary>
    /// 移动端输入桥接器。
    /// 实现 IIntentProvider 将虚拟摇杆和屏幕按钮注入 HSM 输入管道，
    /// 同时包装 PlayerInputProvider 作为键盘/鼠标回落。
    /// 挂载在 MobileHUD UIDocument 所在 GameObject 上。
    /// </summary>
    public class MobileInputBridge : MonoBehaviour, IIntentProvider
    {
        [Header("References")]
        [SerializeField] private UIDocument _uiDoc;
        [SerializeField] private PlayerStateDriver _playerStateDriver;

        [Header("Joystick Sprites")]
        [SerializeField] private Sprite _joystickBaseSprite;
        [SerializeField] private Sprite _joystickHandleSprite;

        // UI 元素
        private VisualElement _attackBtn;
        private VisualElement _dodgeBtn;
        private VisualElement _inventoryBtn;
        private VisualElement _questBtn;
        private VisualElement _interactBtn;
        private Label _interactLabel;

        // 摇杆
        private UIToolkitJoystick _joystick;

        // 帧内输入标记
        private bool _mobileAttackPressed;
        private bool _mobileDodgePressed;

        // 键盘/鼠标回落
        private PlayerInputProvider _keyboardFallback;

        private void Awake()
        {
            var root = _uiDoc.rootVisualElement;
            _attackBtn = root.Q<VisualElement>("attack-btn");
            _dodgeBtn = root.Q<VisualElement>("dodge-btn");
            _inventoryBtn = root.Q<VisualElement>("inventory-btn");
            _questBtn = root.Q<VisualElement>("quest-btn");
            _interactBtn = root.Q<VisualElement>("interact-btn");
            _interactLabel = root.Q<Label>("interact-label");

            // 组装摇杆
            var joystickArea = root.Q<VisualElement>("joystick-area");
            var joystickBase = root.Q<VisualElement>("joystick-base");
            var joystickHandle = root.Q<VisualElement>("joystick-handle");

            if (_joystickBaseSprite != null)
                joystickBase.style.backgroundImage = new StyleBackground(_joystickBaseSprite);
            if (_joystickHandleSprite != null)
                joystickHandle.style.backgroundImage = new StyleBackground(_joystickHandleSprite);

            _joystick = new UIToolkitJoystick(joystickArea, joystickHandle);

            // 必须在 Awake 中设置 override，因为 PlayerStateDriver.Start() 会读取它
            if (_playerStateDriver != null)
            {
                _keyboardFallback = new PlayerInputProvider
                {
                    moveAction = _playerStateDriver.moveAction,
                    jumpAction = _playerStateDriver.jumpAction,
                    runAction = _playerStateDriver.runAction,
                    dodgeAction = _playerStateDriver.dodgeAction,
                    attackAction = _playerStateDriver.attackAction
                };
                _playerStateDriver.intentProviderOverride = this;
            }
        }

        private void Start()
        {
        }

        private void OnEnable()
        {
            // 攻击 / 闪避：用 PointerDown 检测按下（连招窗口需要逐帧精确检测）
            _attackBtn?.RegisterCallback<PointerDownEvent>(OnAttackDown);
            _dodgeBtn?.RegisterCallback<PointerDownEvent>(OnDodgeDown);

            // 背包 / 任务 / 对话入口：用 ClickEvent
            _inventoryBtn?.RegisterCallback<ClickEvent>(OnInventoryClick);
            _questBtn?.RegisterCallback<ClickEvent>(OnQuestClick);
            _interactBtn?.RegisterCallback<ClickEvent>(OnInteractClick);

            // NPC 交互事件
            EventBus.Instance.OnNPCInteractAvailable += OnNPCInteractAvailable;
            EventBus.Instance.OnNPCInteractUnavailable += OnNPCInteractUnavailable;
        }

        private void OnDisable()
        {
            _attackBtn?.UnregisterCallback<PointerDownEvent>(OnAttackDown);
            _dodgeBtn?.UnregisterCallback<PointerDownEvent>(OnDodgeDown);
            _inventoryBtn?.UnregisterCallback<ClickEvent>(OnInventoryClick);
            _questBtn?.UnregisterCallback<ClickEvent>(OnQuestClick);
            _interactBtn?.UnregisterCallback<ClickEvent>(OnInteractClick);

            EventBus.Instance.OnNPCInteractAvailable -= OnNPCInteractAvailable;
            EventBus.Instance.OnNPCInteractUnavailable -= OnNPCInteractUnavailable;

            _joystick?.Dispose();
        }

        /// <summary>IIntentProvider 实现：每帧由 PlayerStateDriver 调用。</summary>
        public void WriteIntent(PlayerContext ctx)
        {
            // 1. 键盘/鼠标回落先写入
            _keyboardFallback?.WriteIntent(ctx);

            // 2. 摇杆覆盖移动方向，位移长度线性映射 0→奔跑速度
            var joyDir = _joystick.InputDirection;
            if (joyDir.sqrMagnitude > 0.001f)
            {
                ctx.moveInput = joyDir;
                ctx.runHeld = true;   // 始终以奔跑为基准，由 inputMagnitude 线性缩放
            }

            // 3. 移动端攻击覆盖
            if (_mobileAttackPressed)
            {
                ctx.attackPressed = true;
                _mobileAttackPressed = false;
            }

            // 4. 移动端闪避覆盖
            if (_mobileDodgePressed)
            {
                ctx.dodgePressed = true;
                _mobileDodgePressed = false;
            }
        }

        // ── 按钮回调 ─────────────────────────────────

        private void OnAttackDown(PointerDownEvent evt)
        {
            _mobileAttackPressed = true;
        }

        private void OnDodgeDown(PointerDownEvent evt)
        {
            _mobileDodgePressed = true;
        }

        private void OnInventoryClick(ClickEvent evt)
        {
            EventBus.Instance.Raise("ToggleInventory");
        }

        private void OnQuestClick(ClickEvent evt)
        {
            EventBus.Instance.Raise("ToggleQuestLog");
        }

        private void OnInteractClick(ClickEvent evt)
        {
            EventBus.Instance.Raise("TriggerNPCInteract");
        }

        // ── NPC 交互 UI ──────────────────────────────

        private void OnNPCInteractAvailable(string npcName)
        {
            if (_interactLabel != null)
                _interactLabel.text = npcName;
            if (_interactBtn != null)
                _interactBtn.style.display = DisplayStyle.Flex;
        }

        private void OnNPCInteractUnavailable()
        {
            if (_interactBtn != null)
                _interactBtn.style.display = DisplayStyle.None;
        }
    }
}
