using GraphProcessor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    class HistoryAssetsFieldView : BlackboardField
    {
        protected TJAIGraphView graphView;

        protected HistoryAssets history;

        protected Label rightLabel;

        protected ContextualMenuManipulator contextualMenuManipulator;

        public BaseNode node { get; private set; }

        public HistoryAssetsFieldView(TJAIGraphView graphView, HistoryAssets history, BaseNode node) : base(null, node.GetCustomName(), "")
        {
            this.graphView = graphView;
            this.history = history;
            this.node = node;
            capabilities &= ~Capabilities.Deletable & ~Capabilities.Renamable;
            this.AddToClassList("history-assets-field");

            var titleLabel = this.Q<Label>(name: "title-label");
            titleLabel.style.fontSize = 14;
            if (node.isRenamable)
            {
                titleLabel.RegisterValueChangedCallback(evt =>
                {
                    if (graphView.nodeViewsPerNode.TryGetValue(node, out var nodeView))
                    {
                        string newTitle = evt.newValue;

                        var titleLabel = nodeView.Q<Label>("title-label");
                        var titleTextFeild = titleLabel.parent.ElementAt(0) as TextField;
                        if (titleTextFeild != null)
                            titleTextFeild.SetValueWithoutNotify(newTitle);
                        
                        graphView.RegisterCompleteObjectUndo("Renamed node " + newTitle);
                        node.SetUniqueCustomName(newTitle);
                        nodeView.title = (node.GetCustomName() == null) ? node.GetType().Name : node.GetCustomName();
                    }
                });
            }


            rightLabel = this.Q<Label>(name: "typeLabel");
            rightLabel.text = history.assetsCache[node].Count.ToString();
            rightLabel.style.flexGrow = 0;
            rightLabel.style.minWidth = 15;
            rightLabel.style.fontSize = 12;

            contextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            Refresh();
        }

        public void Refresh()
        {
            string newLabel = history.assetsCache[node].Count.ToString();
            if(rightLabel.text != newLabel)
            {
                rightLabel.text = newLabel;
                rightLabel.experimental.animation
                    .Start(Color.yellow, Color.clear, 1000, (ve, c) => { ve.style.backgroundColor = c; });
            }
            text = node.GetCustomName();
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            this.RegisterCallback<MouseDownEvent>(ExpandGridView);
            this.AddManipulator(contextualMenuManipulator);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            this.UnregisterCallback<MouseDownEvent>(ExpandGridView);
            this.RemoveManipulator(contextualMenuManipulator);
        }

        void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), node.isRenamable ? DropdownMenuAction.AlwaysEnabled : DropdownMenuAction.AlwaysDisabled);
            evt.menu.AppendAction("Locate", (a) => LocateNodeInGraph(), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }

        void LocateNodeInGraph()
        {
            if(graphView.nodeViewsPerNode.TryGetValue(node, out var nodeView))
            {
                graphView.AddToSelection(nodeView);
            }
        }

        void ExpandGridView(MouseDownEvent evt)
        {
            if(evt.clickCount == 2 && evt.button == 0)
            {
                var blackBoardRow = parent.parent.parent.parent.parent.parent as TJAIBlackboardRow;
                blackBoardRow.expanded = !blackBoardRow.expanded;
                blackBoardRow.RefreshState();
            }
        }
    }
}
