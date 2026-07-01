using GraphProcessor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

/// <summary>
/// Runtime TJAI Graph Custom Inspector
/// Reference: GraphInspector.cs
/// </summary>
[CustomEditor(typeof(RuntimeTJAIGraph))]
public class RuntimeTJAIGraphEditor : UnityEditor.Editor
{
    protected GameObject tgtObj;
    protected RuntimeTJAIGraph graphMono;
    protected BaseGraph graph;
    protected VisualElement root;
    protected ExposedParameterFieldFactory exposedParamFactory;

    VisualElement parameterContainer;
    static bool selectOnPlayMode = false;

    protected virtual void OnEnable()
    {
        graphMono = target as RuntimeTJAIGraph;
        tgtObj = graphMono.gameObject;
        graph = graphMono.Graph;
        if (graph != null)
            graphMono.RefreshParams();
        AddGraph();
        if (Application.isPlaying)
            selectOnPlayMode |= graphMono != null;
    }

    protected virtual void OnDisable()
    {
        //RemoveGraph();
    }

    /// <summary>
    /// process when component is removed in edit mode
    /// </summary>
    protected void OnDestroy()
    {
        if (Selection.activeGameObject == tgtObj)
        {
            if (Application.isPlaying)
            {
                selectOnPlayMode = true;
                return;
            }
            else if (selectOnPlayMode)
            {
                selectOnPlayMode = false;
                return;
            }
            if (graphMono == null)
            {
                Debug.Log($"Releasing asset of {graph?.name}");
                if (exposedParamFactory != null)
                    exposedParamFactory.ReleaseResource();
            }
        }
        RemoveGraph();
    }

    /// <summary>
    /// use UIElement instead of IMGUI
    /// </summary>
    /// <returns></returns>
    public sealed override VisualElement CreateInspectorGUI()
    {
        root = new VisualElement();
        // create exposed parameter gui
        parameterContainer = new VisualElement
        {
            name = "ExposedParameters"
        };
        FillExposedParameters(parameterContainer);
        root.Add(parameterContainer);
        // create graph gui
        SerializedObject serializedTarget = new SerializedObject(target);
        SerializedProperty graphProp = serializedTarget.FindProperty("graph");
        PropertyField graphPropField = new PropertyField(graphProp);
        root.Insert(0, graphPropField);
        // for repeated property
        SerializedProperty repeatProp = serializedTarget.FindProperty("repeat");
        PropertyField repeatPropField = new PropertyField(repeatProp);
        root.Insert(1, repeatPropField);
        graphPropField.RegisterCallback<SerializedPropertyChangeEvent>((evt) =>
        {
            if (evt.currentTarget != evt.target)
                return;
            if (evt.changedProperty.objectReferenceValue is TJAIGraph newGraph && newGraph != graph)
            {
                // remove original graph
                RemoveGraph();
                graphMono.Graph = newGraph;
                graph = graphMono.Graph;
                AddGraph();
                UpdateExposedParameters();
                evt.StopPropagation();
                //Debug.Log($"RegisterCallback: {graphMono.Graph.name}");
            }
        });
        return root;
    }

    /// <summary>
    /// invoke when user select a graph
    /// </summary>
    void AddGraph()
    {
        if (graph == null)
            return;
        graph.onExposedParameterListChanged -= UpdateExposedParameters;
        graph.onExposedParameterDisplayChanged -= UpdateExposedParameters;
        graph.onExposedParameterListChanged += UpdateExposedParameters;
        graph.onExposedParameterDisplayChanged += UpdateExposedParameters;
        if (exposedParamFactory == null)
            exposedParamFactory = new ExposedParameterFieldFactory(graph, graphMono.exposedParams);
    }

    /// <summary>
    /// invoken when user select a new graph or disable component
    /// </summary>
    void RemoveGraph()
    {
        if (graph == null)
            return;
        graph.onExposedParameterListChanged -= UpdateExposedParameters;
        graph.onExposedParameterDisplayChanged -= UpdateExposedParameters;
        exposedParamFactory?.Dispose(); //  Graphs that created in GraphBehaviour sometimes gives null ref.
        exposedParamFactory = null;
    }

    /// <summary>
    /// draw ExposedParam inspector
    /// </summary>
    /// <param name="parameterContainer"></param>
    protected void FillExposedParameters(VisualElement parameterContainer)
    {
        if (graph == null)
            return;
        if (parameterContainer == null)
            return;
        parameterContainer.Clear();
        if (graphMono.exposedParams?.Count > 0)
            parameterContainer.Add(new Label(" Exposed Parameters:"));

        foreach (var param in graphMono.exposedParams)
        {
            if (param.settings.isHidden)
                continue;

            var field = exposedParamFactory.GetParameterValueField(param, (newValue) =>
            {
                param.value = newValue;
                serializedObject.ApplyModifiedProperties();
                //graph.NotifyExposedParameterValueChanged(param);
            });
            // distinguish input and output
            if (param.settings.accessor == ParameterAccessor.Set)
                field.SetEnabled(false);
            parameterContainer.Add(field);
        }
    }

    void UpdateExposedParameters(ExposedParameter param)
    {
        exposedParamFactory.UpdateParamsDisplay(param);
        FillExposedParameters(parameterContainer);
    }

    void UpdateExposedParameters()
    {
        exposedParamFactory.UpdateParamsDisplay();
        FillExposedParameters(parameterContainer);
    }

    // Don't use ImGUI
    public sealed override void OnInspectorGUI() { }
}