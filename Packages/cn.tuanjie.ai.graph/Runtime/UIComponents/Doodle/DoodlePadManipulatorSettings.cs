using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace UnityEngine.AIGraph
{
    internal class DoodlePadManpulatorSettings
    {
        VisualElement m_Root;
        DoodlePadManipulator m_DoodlePadManipulator;
        bool m_IsInitialized;
        //TJAIShortcut[] m_Shortcuts;
        TouchSliderFloat m_RadiusSlider;
        Toggle m_ToggleErase;

        private DoodlePadManpulatorSettings() { }

        public DoodlePadManpulatorSettings(DoodlePadManipulator paintingManipulator)
        {
            m_DoodlePadManipulator = paintingManipulator;
            Init();
        }

        void Init()
        {
            if (m_IsInitialized)
                return;

            m_Root = new VisualElement();
            m_Root.style.flexDirection = FlexDirection.Row;
            m_RadiusSlider = new TouchSliderFloat();
            m_RadiusSlider.label = "粗细";
            //m_RadiusSlider.tooltip = TextContent.controlMaskBrushSizeTooltip;
            m_RadiusSlider.incrementFactor = 0.1f;
            m_RadiusSlider.formatString = "F1";
            m_RadiusSlider.lowValue = m_DoodlePadManipulator.minRadius;
            m_RadiusSlider.highValue = m_DoodlePadManipulator.maxRadius;
            m_RadiusSlider.value = m_DoodlePadManipulator.initRadius;
            m_RadiusSlider.style.width = 150.0f;

            m_RadiusSlider.RegisterValueChangedCallback(evt =>
            {
                m_DoodlePadManipulator.SetRadius(evt.newValue);
            });

            m_ToggleErase = new Toggle { label = "橡皮擦" };

            m_ToggleErase.RegisterValueChangedCallback(evt =>
            {
                m_DoodlePadManipulator.SetEraserMode(evt.newValue);
            });
            m_ToggleErase.style.width = 100.0f;

            var clearButton = new ActionButton
            {
                name = "refiner-clear-button",
                label = "",
                icon = "delete",
                quiet = true
            };

            clearButton.AddToClassList("TJAI-controltoolbar__actionbutton");
            clearButton.clicked += () =>
            {
                m_DoodlePadManipulator.ClearPainting();
            };
            m_Root.Add(m_ToggleErase);
            m_Root.Add(m_RadiusSlider);
            m_Root.Add(clearButton);

            m_Root.RegisterCallback<AttachToPanelEvent>(OnAttach);
            m_Root.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            m_IsInitialized = true;
        }

        void OnAttach(AttachToPanelEvent evt)
        {
            //m_Shortcuts = new[]
            //{
            //    new TJAIShortcut("Increase Brush Size", OnIncreaseBrushSize, KeyCode.RightBracket, source: m_Root),
            //    new TJAIShortcut("Decrease Brush Size", OnDecreaseBrushSize, KeyCode.LeftBracket, source: m_Root),
            //    new TJAIShortcut("Toggle Brush", ToggleBrush, KeyCode.B, source: m_Root),
            //    new TJAIShortcut("Toggle Eraser", ToggleEraser, KeyCode.E, source: m_Root),
            //    new TJAIShortcut("Clear", ClearDoodle, KeyCode.Delete, source: m_Root)
            //};

            //foreach (var shortcut in m_Shortcuts)
            //    TJAIShortcuts.AddShortcut(shortcut);
        }

        void OnDetach(DetachFromPanelEvent evt)
        {
            //foreach (var shortcut in m_Shortcuts)
            //    TJAIShortcuts.RemoveShortcut(shortcut);
        }

        public VisualElement GetSettings()
        {
            return m_Root;
        }

        void OnIncreaseBrushSize()
        {
            if (!isFocused)
                return;

            m_RadiusSlider.value += k_RadiusStep;
        }

        const float k_RadiusStep = 0.5f;

        void OnDecreaseBrushSize()
        {
            if (!isFocused)
                return;

            m_RadiusSlider.value -= k_RadiusStep;
        }

        void ToggleBrush()
        {
            if (!isFocused)
                return;

            if (m_ToggleErase.value)
                m_ToggleErase.value = false;
        }

        void ToggleEraser()
        {
            if (!isFocused)
                return;

            if (!m_ToggleErase.value)
                m_ToggleErase.value = true;
        }

        void ClearDoodle()
        {
            if (!isFocused)
                return;

            m_DoodlePadManipulator.ClearPainting();
        }

        bool isFocused
        {
            get
            {
                //var focusedElement = m_PaintingManipulator.target.panel.focusController.focusedElement;
                //return focusedElement == m_PaintingManipulator.target;
                return true;
            }
        }
    }
}