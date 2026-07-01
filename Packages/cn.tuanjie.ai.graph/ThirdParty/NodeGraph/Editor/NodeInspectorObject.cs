using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
    /// <summary>
    /// Custom editor of the node inspector, you can inherit from this class to customize your node inspector.
    /// </summary>
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        NodeInspectorObject inspector;
        protected VisualElement root;
        protected VisualElement selectedNodeList;
        protected VisualElement placeholder;

        protected virtual void OnEnable()
        {
            inspector = target as NodeInspectorObject;
            inspector.nodeSelectionUpdated += UpdateNodeInspectorList;
            root = new VisualElement();
            selectedNodeList = new VisualElement();
            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/InspectorView"));
            root.Add(selectedNodeList);
            placeholder = new Label("Select a node to show it's settings in the inspector");
            placeholder.AddToClassList("PlaceHolder");
            UpdateNodeInspectorList();
        }

        protected virtual void OnDisable()
        {
            inspector.nodeSelectionUpdated -= UpdateNodeInspectorList;
        }

        public override VisualElement CreateInspectorGUI() => root;

        protected virtual void UpdateNodeInspectorList()
        {
            selectedNodeList.Clear();

            if (inspector.selectedNodes.Count == 0)
                selectedNodeList.Add(placeholder);

            foreach (var nodeView in inspector.selectedNodes)
                selectedNodeList.Add(CreateNodeBlock(nodeView));
        }

        /// <summary>
        /// Create inspector UI. If the node has inspector right now (actions such as change node/refresh), return original inspector.
        /// Or, a new inspector container will be created with node namne on it. (It will be activated when node created or graph reopen)
        /// </summary>
        /// <param name="nodeView"></param>
        /// <returns></returns>
        protected VisualElement CreateNodeBlock(BaseNodeView nodeView)
        {
            if(nodeView.inspectorContainer != null)
                return nodeView.inspectorContainer;

            nodeView.inspectorContainer = new VisualElement();
            nodeView.inspectorContainer.Add(new Label(nodeView.nodeTarget.GetCustomName()));
            if (!string.IsNullOrEmpty(nodeView.nodeTarget.description))
            {
                Label description = new Label("Description: "+nodeView.nodeTarget.description);
                description.name = "description";
                nodeView.inspectorContainer.Add(description);
            }
            nodeView.Enable(true);
            nodeView.inspectorContainer.AddToClassList("NodeControls");
            return nodeView.inspectorContainer;
            //var view = new VisualElement();

            //view.Add(new Label(nodeView.nodeTarget.GetCustomName()));
            //var tmp = nodeView.controlsContainer;
            //nodeView.controlsContainer = view;
            //nodeView.Enable(true);
            //nodeView.controlsContainer.AddToClassList("NodeControls");
            //var block = nodeView.controlsContainer;
            //nodeView.controlsContainer = tmp;
            
            //return block;
        }
    }

    /// <summary>
    /// Node inspector object, you can inherit from this class to customize your node inspector.
    /// </summary>
    public class NodeInspectorObject : ScriptableObject
    {
        /// <summary>Previously selected object by the inspector</summary>
        public Object previouslySelectedObject;
        /// <summary>List of currently selected nodes</summary>
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new HashSet<BaseNodeView>();

        /// <summary>Triggered when the selection is updated</summary>
        public event Action nodeSelectionUpdated;

        /// <summary>Updates the selection from the graph</summary>
        public virtual void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            selectedNodes = views;
            nodeSelectionUpdated?.Invoke();
        }

        public virtual void RefreshNodes() => nodeSelectionUpdated?.Invoke();

        public virtual void NodeViewRemoved(BaseNodeView view)
        {
            selectedNodes.Remove(view);
            nodeSelectionUpdated?.Invoke();
        }
    }
}