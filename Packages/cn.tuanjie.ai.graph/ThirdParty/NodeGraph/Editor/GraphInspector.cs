using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class GraphInspector : Editor
    {
        protected VisualElement root;
        protected BaseGraph     graph;
        protected ExposedParameterFieldFactory exposedParameterFactory;

        VisualElement           parameterContainer;
        VisualElement descriptionContainer;

        protected virtual void OnEnable()
        {
            graph = target as BaseGraph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
            graph.onExposedParameterModified += UpdateExposedParameters;
            if (exposedParameterFactory == null)
                exposedParameterFactory = new ExposedParameterFieldFactory(graph);
        }

        protected virtual void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
            graph.onExposedParameterModified -= UpdateExposedParameters;
            exposedParameterFactory?.Dispose(); //  Graphs that created in GraphBehaviour sometimes gives null ref.
            exposedParameterFactory = null;
        }

        public sealed override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            CreateInspector();
            return root;
        }

        protected virtual void CreateInspector()
        {
            root.styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/GraphInspectorView"));
            descriptionContainer = new VisualElement { name = "description"};
            root.Add(descriptionContainer);

            Label labelDes = new Label();
            labelDes.name = "label-description";
            if(string.IsNullOrEmpty(graph.description))
                labelDes.text = "( You can describe the graph here with double click.)";
            else
                labelDes.text = graph.description;
            descriptionContainer.Add(labelDes);
            UnityEngine.UIElements.TextField textDes = new UnityEngine.UIElements.TextField();
            textDes.multiline = true;
            textDes.name = "textfield-description";
            textDes.style.display = DisplayStyle.None;
            labelDes.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    textDes.value = labelDes.text;
                    labelDes.style.display = DisplayStyle.None;
                    textDes.style.display = DisplayStyle.Flex;
                    textDes.Focus();
                }
            });
            textDes.RegisterCallback<FocusOutEvent>(evt =>
            {
                labelDes.text = textDes.value;
                graph.description = labelDes.text;
                textDes.style.display = DisplayStyle.None;
                labelDes.style.display = DisplayStyle.Flex;
            });
            descriptionContainer.Add(textDes);



            parameterContainer = new VisualElement{
                name = "ExposedParameters"
            };
            FillExposedParameters(parameterContainer);

            root.Add(parameterContainer);
        }

        protected void FillExposedParameters(VisualElement parameterContainer)
        {
            if (graph.exposedParameters.Count != 0)
                parameterContainer.Add(new Label("Exposed Parameters:"));

            foreach (var param in graph.exposedParameters)
            {
                if (param.settings.isHidden)
                    continue;

                var field = exposedParameterFactory.GetParameterValueField(param, (newValue) => {
                    param.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                    graph.NotifyExposedParameterValueChanged(param);
                });
                parameterContainer.Add(field);
            }
        }

        void UpdateExposedParameters(ExposedParameter param) => UpdateExposedParameters();

        void UpdateExposedParameters()
        {
            if (parameterContainer == null)
                return;
            parameterContainer.Clear();
            FillExposedParameters(parameterContainer);
        }

        // Don't use ImGUI
        public sealed override void OnInspectorGUI() {}

    }
}