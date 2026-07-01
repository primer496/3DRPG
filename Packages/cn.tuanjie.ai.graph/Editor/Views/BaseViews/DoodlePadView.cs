using System;
using Unity.AppUI.UI;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraphs
{
    public class DoodlePadView : ExVisualElement
    {
        DoodlePadNode node;
        public Texture mask;
        readonly Vector2Int k_DefaultDoodleSize = new Vector2Int(512, 512);


        VisualElement m_Body;
        VisualElement m_DoodleGroupL;
        VisualElement m_DoodleGroupR;
        VisualElement m_ImageContainer;
        VisualElement m_ContextMenuAnchor;

        DoodlePadManipulator m_DoodlePadManipulator;
        Texture2D m_DoodleBuffer;
        TouchSliderFloat m_RadiusSlider;
        Image m_PreviewImage;

        ActionGroup m_DoodleActionGroupL;
        ActionButton m_BrushBtn;
        ActionButton m_EraserBtn;

        //ActionGroup m_DoodleActionGroupR;
        ActionButton m_DeleteBtn;
        ActionButton m_DestroyBtn;

        public event Action onDestroyDoodlePadView;

        float brushRadius { get; set; }
        float eraseRadius { get; set; }


        public DoodlePadView(DoodlePadNode node)
        {
            this.node = node;
            this.name = "doodle";
            SDEditorUtils.SetEnableAppUI(this, true);
            AddDraggableBehaviour(this);
            CreateGUI();
        }

        public void CreateGUI()
        {
            passMask = Passes.Clear | Passes.OutsetShadows;
            styleSheets.Add(Resources.Load<StyleSheet>("uss/TJAIStyle"));
            styleSheets.Add(Resources.Load<StyleSheet>("uss/DoodlePadView"));
            AddToClassList("doodle-pad-view");

            var outlineRow = new VisualElement() { name = "ControlBar" };
            Add(outlineRow);
            outlineRow.AddToClassList("row");

            m_DoodleGroupL = new VisualElement() { name = "ControlBarLeft" };
            m_DoodleGroupL.AddToClassList("row");
            outlineRow.Add(m_DoodleGroupL);
            m_DoodleActionGroupL = new ActionGroup { compact = true };
            m_BrushBtn = new ActionButton(OnBrushBtnClicked) { icon = "paint-brush", accent = true, tooltip = "brush" };
            m_EraserBtn = new ActionButton(OnEraserBtnClicked) { icon = "eraser", accent = true, tooltip = "erase" };
            m_DeleteBtn = new ActionButton(OnDeleteBtnClicked) { icon = "delete", tooltip = "delete" };
            m_DoodleActionGroupL.Add(m_BrushBtn);
            m_DoodleActionGroupL.Add(m_EraserBtn);
            m_DoodleActionGroupL.Add(m_DeleteBtn);
            m_DoodleGroupL.Add(m_DoodleActionGroupL);


            m_DoodleGroupR = new VisualElement() { name = "ControlBarRight" };
            m_DoodleGroupR.AddToClassList("row");
            outlineRow.Add(m_DoodleGroupR);
            //m_DoodleActionGroupR = new ActionGroup { compact = true };

            m_DestroyBtn = new ActionButton(OnDestroyBtnClicked) { icon = "check", tooltip = "save and close" };
            //m_DoodleActionGroupR.Add(m_DestroyBtn);
            //m_DoodleGroupR.Add(m_DoodleActionGroupR);
            m_DoodleGroupR.Add(m_DestroyBtn);

            m_ImageContainer = new VisualElement() { name = "PreviewContainer", pickingMode = PickingMode.Position, focusable = true };
            Add(m_ImageContainer);
            m_ImageContainer.RegisterCallback<GeometryChangedEvent>(evt => m_ImageContainer.style.height = evt.newRect.width);

            m_PreviewImage = new Image { pickingMode = PickingMode.Ignore };
            m_PreviewImage.AddToClassList("TJAI-dropzone__image");
            m_ImageContainer.Add(m_PreviewImage);
            
            m_RadiusSlider = new TouchSliderFloat();
            m_RadiusSlider.label = "粗细";
            m_RadiusSlider.tooltip = "调整画笔和橡皮的粗细";
            m_RadiusSlider.incrementFactor = 0.1f;
            m_RadiusSlider.formatString = "F1";
            m_RadiusSlider.style.marginLeft = 10;
            m_RadiusSlider.style.width = 100;
            m_RadiusSlider.RegisterValueChangedCallback(OnSetRadius);
            m_DoodleGroupL.Add(m_RadiusSlider);

            OnImageUpdated();
            RegisterDoodlePad();
        }

        public void OnImageUpdated()
        {
            var newImage = node.GetTexture();
            if (m_PreviewImage.image == newImage) return;
            m_PreviewImage.image = newImage;
            m_DoodlePadManipulator = new DoodlePadManipulator(new Vector2Int(m_PreviewImage.image.width, m_PreviewImage.image.height), opacity: 0.7f);
            m_DoodlePadManipulator.onModifierStateChanged += OnDoodleModifierChanged;
            m_DoodlePadManipulator.onValueChanged += OnDoodleChanged;
            var maskImage = node.GetMask();
            if (maskImage != null && maskImage.width == newImage.width && maskImage.height == newImage.height)
                m_DoodlePadManipulator.SetValue(maskImage.EncodeToPNG());

            m_RadiusSlider.lowValue = Math.Min(0.1f, m_DoodlePadManipulator.minRadius);
            m_RadiusSlider.highValue = Math.Max(1f, m_DoodlePadManipulator.maxRadius);
            m_RadiusSlider.SetValueWithoutNotify(m_DoodlePadManipulator.initRadius);
            
            brushRadius = m_RadiusSlider.value;
            eraseRadius = m_RadiusSlider.highValue;
        }

        void RegisterDoodlePad()
        {
            m_ImageContainer.AddManipulator(m_DoodlePadManipulator);
            m_DoodlePadManipulator.SetNone();
        }

        void UnregisterDoodlePad()
        {
            m_ImageContainer.RemoveManipulator(m_DoodlePadManipulator);
            m_DoodlePadManipulator.SetNone();
        }

        void OnBrushBtnClicked()
        {
            m_DoodlePadManipulator.ToggleBrush();

        }

        void OnEraserBtnClicked()
        {
            m_DoodlePadManipulator.ToggleEraser();
        }

        void OnDeleteBtnClicked()
        {
            m_DoodlePadManipulator.ClearPainting();
            m_DoodlePadManipulator.SetBrush();
        }

        void OnDestroyBtnClicked()
        {
            if (m_DoodleBuffer)
                node.SaveMask(m_DoodleBuffer);
            // else
            //     node.SaveTexture(node.GetMask());
            m_DoodlePadManipulator.SetValueWithoutNotify(null);
            UnregisterDoodlePad();
            this.parent.Remove(this);
            onDestroyDoodlePadView?.Invoke();
        }

        void OnDoodleModifierChanged(DoodleModifierState state)
        {
            m_BrushBtn.selected = state == DoodleModifierState.Brush;
            m_EraserBtn.selected = state == DoodleModifierState.Erase;

            if (state == DoodleModifierState.Brush)
            {
                m_RadiusSlider.style.display = DisplayStyle.Flex;
                m_RadiusSlider.value = brushRadius;
            }
            else if (state == DoodleModifierState.Erase)
            {
                m_RadiusSlider.style.display = DisplayStyle.Flex;
                m_RadiusSlider.value = eraseRadius;
            }
            else
            {
                m_RadiusSlider.style.display = DisplayStyle.None;
            }

        }

        void OnDoodleChanged(byte[] bytes)
        {
            m_DoodleBuffer = TextureUtils.ConvertToGrayscale(bytes.ToTexture2D());
        }


        void OnSetRadius(ChangeEvent<float> evt)
        {
            float newValue = evt.newValue;
            if (m_DoodlePadManipulator.currentState == DoodleModifierState.Brush)
            {
                brushRadius = newValue;
            }
            else if (m_DoodlePadManipulator.currentState == DoodleModifierState.Erase)
            {
                eraseRadius = newValue;
            }
            m_DoodlePadManipulator.SetRadius(newValue);
        }


        private void AddDraggableBehaviour(VisualElement element)
        {
            Vector2 startMousePosition = Vector2.zero;
            Vector2 startPosition = Vector2.zero;

            element.RegisterCallback<MouseDownEvent>(evt =>
            {
                startMousePosition = evt.mousePosition;
                startPosition = element.transform.position;
                element.CaptureMouse();
                evt.StopPropagation();
            });

            element.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (element.HasMouseCapture())
                {
                    Vector2 delta = evt.mousePosition - startMousePosition;
                    element.transform.position = startPosition + delta;

                    evt.StopPropagation();
                }
            });

            element.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (element.HasMouseCapture())
                {
                    element.ReleaseMouse();
                    evt.StopPropagation();
                }
            });
        }
    }


}