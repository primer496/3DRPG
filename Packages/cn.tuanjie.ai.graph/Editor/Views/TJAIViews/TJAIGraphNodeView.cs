/******************************************************************************
* Company:         UnityChina
* Author:          may.luo
* CreateTime:      2024-09-29 18:31:13
* Version:         0.0.1   
* UnityVersion:    2022.3.17f1c1
* Description:
******************************************************************************/

using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

/// <summary>
/// 
/// </summary>
[NodeCustomEditor(typeof(TJAIGraphNode))]
public class TJAIGraphNodeView : BaseTJAINodeView
{
    private new TJAIGraphNode nodeTarget => base.nodeTarget as TJAIGraphNode;

    public override void Enable()
    {
        if (nodeTarget.subGraph != null)
        {
            title = nodeTarget.subGraph.name;
            nodeTarget.subGraph.onExposedParameterListChanged -= OnSubGraphParamChanged;
            nodeTarget.subGraph.onExposedParameterListChanged += OnSubGraphParamChanged;
            nodeTarget.subGraph.onExposedParameterDisplayChanged -= OnSubGraphParamChanged;
            nodeTarget.subGraph.onExposedParameterDisplayChanged += OnSubGraphParamChanged;
        }
        base.Enable();
        this.AddManipulator(new DoubleClickManipulator());
    }

    public override void Disable()
    {
        base.Disable();
        if (nodeTarget.subGraph != null)
        {
            nodeTarget.subGraph.onExposedParameterListChanged -= OnSubGraphParamChanged;
            nodeTarget.subGraph.onExposedParameterDisplayChanged -= OnSubGraphParamChanged;
        }
    }

    public void OnSubGraphParamChanged(ExposedParameter param) => OnSubGraphParamChanged();

    /// <summary>
    /// update GraphNodeView when ref subGraph's exposedParam is changed
    /// </summary>
    public void OnSubGraphParamChanged()
    {
        Debug.Log($"detect subgraph param changed");
        nodeTarget.UpdateAllPorts();
        RefreshPorts();
    }

    public void OpenTargetNodeAsset()
    {
        TJAIGraphWindow.Open(nodeTarget.subGraph);
    }
    
}


/// <summary>
/// double click event to open the graph
/// </summary>
public class DoubleClickManipulator : MouseManipulator
{
    //private VisualElement targetNode;
    private int clickCount = 0;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.5f; // 双击间隔时间  

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            float currentTime = Time.realtimeSinceStartup;
            if (clickCount == 1 && (currentTime - lastClickTime) < doubleClickThreshold)
            {
                // 双击事件处理  
                clickCount = 0;
                OpenAsset();
            }
            else
            {
                clickCount = 1; 
                lastClickTime = currentTime;
            }
        }
    }

    private void OpenAsset()
    {
        //Debug.Log("Node double-clicked, opening asset...");
        if (target is TJAIGraphNodeView customNode)
        {
            customNode.OpenTargetNodeAsset();
        }
    }
}