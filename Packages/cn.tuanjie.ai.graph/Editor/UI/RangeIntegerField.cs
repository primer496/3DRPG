using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    public class RangeIntegerField : IntegerField
    {
        public int minValue { get; set; }
        public int maxValue { get; set; }
        
        public override int value
        {
            get => base.value;
            set => base.value = Mathf.Clamp(value, minValue, maxValue);
        }

        private Coroutine validationCoroutine;
        private bool isEditing = false;

        public RangeIntegerField(string label, int min, int max) : base(label)
        {
            minValue = min;
            maxValue = max;

            // 多种事件监听
            RegisterCallback<InputEvent>(OnInput);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            this.RegisterValueChangedCallback(OnValueChanged);
        }

        private void OnInput(InputEvent evt) => StartEditing();
        private void OnFocusIn(FocusInEvent evt) => StartEditing();

        private void OnFocusOut(FocusOutEvent evt)
        {
            StopEditing();
            ValidateImmediately();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                StopEditing();
                ValidateImmediately();
                evt.StopPropagation();
            }
        }

        private void OnValueChanged(ChangeEvent<int> evt)
        {
            if (isEditing) return;
            ValidateImmediately();
        }

        private void StartEditing()
        {
            isEditing = true;
        }

        private void StopEditing() => isEditing = false;


        private void ValidateImmediately()
        {
            int validatedValue = Mathf.Clamp(value, minValue, maxValue);
            if (value != validatedValue)
            {
                value = validatedValue;
            }
        }
    }

    public class SliderRangeIntegerField : BaseField<int>
    {
        public int MinValue
        {
            get => slider.lowValue;
            set
            {
                slider.lowValue = value;
                rangeField.minValue = value;
            }
        }

        public int MaxValue
        {
            get => slider.highValue;
            set
            {
                slider.highValue = value;
                rangeField.maxValue = value;
            }
        }

        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public new static readonly string ussClassName = "slider-range-integer-field";
        /// <summary>
        /// USS class name of slider in elements of this type.
        /// </summary>
        public static readonly string sliderUssClassName = ussClassName + "__slider";
        /// <summary>
        /// USS class name of range field elements in elements of this type.
        /// </summary>
        public static readonly string rangeUssClassName = ussClassName + "__range";
        
        private readonly SliderInt slider;
        private readonly RangeIntegerField rangeField;
        
        public SliderRangeIntegerField(string label, int min, int max): 
            base(label, null)
        {
            slider = new SliderInt("", min, max);
            rangeField = new RangeIntegerField("", min, max);
            slider.value = rangeField.value = min;
            slider.RegisterValueChangedCallback(OnValueChanged);
            rangeField.RegisterValueChangedCallback(OnValueChanged);
            AddToClassList(ussClassName);
            slider.AddToClassList(sliderUssClassName);
            rangeField.AddToClassList(rangeUssClassName);
            Add(slider);
            Add(rangeField);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            slider.SetValueWithoutNotify(newValue);
            rangeField.SetValueWithoutNotify(newValue);
        }

        public new int value
        {
            get => slider.value;
            set
            {
                slider.value = value;
                rangeField.value = value;
            }
        }

        private void OnValueChanged(ChangeEvent<int> evt)
        {
            value = evt.newValue;
        }
    }
}