using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Unity.AppUI.UI;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;
using GridView = UnityEngine.AIGraph.GridView;
using HistoryChangeType = UnityEngine.AIGraph.HistoryAssets.HistoryChangeType;
using SelectState = UnityEditor.AIGraph.HistoryAssetsContentView.SelectState;

namespace UnityEditor.AIGraph
{
    class HistoryAssetsView : PinnedElementView
    {
        protected TJAIGraphView graphView;
        protected ScrollView nodeScrollView;
        protected Checkbox globalSelector;
        protected ActionButton expandButton;
        protected ActionButton foldupBtn;
        protected ActionButton expandBtn;
        protected ActionButton exportBtn;
        protected ActionButton deleteBtn;
        protected static Vector2 initSize = new Vector2(400, 600);
        protected IReadOnlyList<Rect> blackboardLayouts;
        private VisualElement contextMenuAnchor;
        private bool showContextMenu = false;
        private bool showAll = true;

        protected HistoryAssets history;

        protected static int itemSize = HistoryAssetsContentView.initMinItemSize;
        protected int checkedNodeCount = 0;

        protected Dictionary<BaseNode, TJAIBlackboardRow> nodeRowCache;
        protected Dictionary<BaseNode, IReadOnlyList<int>> globalSelectedIndices;
        protected Dictionary<BaseNode, IReadOnlyList<previewData>> globalSelectedItems;

        protected int globalSelectionCount
        {
            get
            {
                int cnt = 0;
                foreach (var p in globalSelectedIndices)
                    cnt += p.Value.Count;
                return cnt;
            }
        }

        protected (TJAIBaseAssetNode, previewData) globalSelectedPair
        {
            get
            {
                if (globalSelectionCount == 0)
                    return (null, new previewData());
                else
                {
                    var node = globalSelectedItems.Keys.FirstOrDefault() as TJAIBaseAssetNode;
                    if (node == null)
                        return (null, new previewData());

                    var artifact = globalSelectedItems[node].FirstOrDefault();
                    return (node, artifact);
                }
            }
        }

        readonly string historyAssetsViewStyle = "uss/HistoryAssetsView";

        public HistoryAssetsView()
        {
            var ss = Resources.Load<StyleSheet>(historyAssetsViewStyle);
            if (ss != null)
                styleSheets.Add(ss);
            AddToClassList("history-assets-view");

            SDEditorUtils.SetEnableAppUI(this, true);
        }

        protected override void Initialize(BaseGraphView baseGraphView)
        {
            title = "History Assets";
            graphView = baseGraphView as TJAIGraphView;
            history = this.graphView.graph.history;
            scrollable = false;

            pinnedElement.position.size = Vector2.Max(pinnedElement.position.size, initSize);
            SetPosition(pinnedElement.position);
            style.minWidth = initSize.x;
            style.minHeight = initSize.y;

            var btnRow = new VisualElement() { name = "buttonZone" };
            content.Add(btnRow);

            var btnLeft = new VisualElement() { name = "buttonZoneLeft" };
            globalSelector = new Checkbox() { label = "" };
            globalSelector.RegisterValueChangedCallback(evt => OnGlobalChecked(evt.newValue));
            btnLeft.Add(globalSelector);

            expandButton = new ActionButton() { name = "expandBtn" };
            var imageDown = Resources.Load<Texture2D>("GraphView/Nodes/NodeChevronDown");
            var imageRight = Resources.Load<Texture2D>("GraphView/Nodes/NodeChevronRight");
            expandButton.style.backgroundImage = imageDown;
            expandButton.clickable.clicked += () =>
            {
                showAll = !showAll;
                if (showAll)
                    expandButton.style.backgroundImage = imageDown;
                else
                    expandButton.style.backgroundImage = imageRight;
                OnExpand(showAll);
            };
            btnLeft.Add(expandButton);
            Label expandName = new Label();
            expandName.style.marginLeft = 10;
            expandName.text = "All";
            expandName.style.color = new Color(193f / 255f, 193f / 255f, 193f / 255f);
            btnLeft.Add(expandName);

            btnRow.Add(btnLeft);

            var btnRight = new VisualElement() { name = "buttonZoneRight" };

            var sep = new VisualElement() { style = { width = 20 } };
            btnRight.Add(sep);
            exportBtn = new ActionButton(OnExport) { name = "exportBtn", icon = "export" };
            btnRight.Add(exportBtn);
            deleteBtn = new ActionButton(OnDelete) { name = "deleteBtn", icon = "delete" };
            btnRight.Add(deleteBtn);
            btnRow.Add(btnRight);

            nodeScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            nodeScrollView.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            nodeScrollView.AddToClassList("nodeList");
            content.Add(nodeScrollView);

            focusable = true;
            pickingMode = PickingMode.Position;

            void Block(EventBase e)
            {
                e.StopPropagation();
            }

            RegisterCallback<PointerDownEvent>(e => { Focus(); Block(e); });
            RegisterCallback<PointerMoveEvent>(e => Block(e));
            RegisterCallback<PointerUpEvent>(e => Block(e));
            RegisterCallback<MouseDownEvent>(e => Block(e));
            RegisterCallback<MouseUpEvent>(e => Block(e));
            RegisterCallback<ContextClickEvent>(e => Block(e));
 
            nodeScrollView.RegisterCallback<WheelEvent>(e =>
            {
                e.StopPropagation();
                e.PreventDefault(); 
            });

            var statusRow = new VisualElement() { name = "statusZone" };
            content.Add(statusRow);

            var sizeSlider = new Unity.AppUI.UI.SliderInt() { name = "size-slider" };
            sizeSlider.lowValue = HistoryAssetsContentView.lowValueOfMinItemSize;
            sizeSlider.highValue = HistoryAssetsContentView.highValueOfMinItemSize;
            sizeSlider.value = itemSize;
            sizeSlider.tooltip = "modify grid item size";
            sizeSlider.Q(name: "appui-slider__labelcontainer").style.display = DisplayStyle.None;
            sizeSlider.RegisterValueChangingCallback(OnGridItemSizeChanging);
            statusRow.Add(sizeSlider);

            contextMenuAnchor = new VisualElement();
            contextMenuAnchor.style.position = Position.Absolute;
            contextMenuAnchor.Add(new IMGUIContainer(() =>
            {
                if (showContextMenu)
                {
                    GenericMenu menu = new GenericMenu();
                    BuildContextualMenu(menu);
                    menu.ShowAsContext();
                    showContextMenu = false;
                }
            }));
            content.Add(contextMenuAnchor);

            nodeRowCache = new();
            globalSelectedIndices = new();
            globalSelectedItems = new();

            history.onHistoryChanged -= OnHistoryChanged;
            history.onHistoryChanged += OnHistoryChanged;
            Undo.undoRedoPerformed -= Refresh;
            Undo.undoRedoPerformed += Refresh;

            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallback<MouseDownEvent>(evt => UpdateRowLayouts(), TrickleDown.TrickleDown);
            RegisterCallback<DetachFromPanelEvent>(OnViewClosed);
            RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            RegisterCallback<AttachToPanelEvent>(evt => Refresh());

            Refresh();
        }

        void Refresh()
        {
            nodeScrollView.Clear();
            nodeRowCache.Clear();

            foreach (var node in history.orderedNodes)
            {
                AddNodeView(node);
            }

            ClearSelection();
        }

        void AddNodeView(BaseNode node)
        {
            if (nodeRowCache.ContainsKey(node))
                return;

            var expanded = history.assetsCache[node].expanded;
            var fieldView = new HistoryAssetsFieldView(graphView, history, node);
            var contentView = new HistoryAssetsContentView(graphView, history, node);
            var row = new TJAIBlackboardRow(node, fieldView, contentView);
            row.expanded = expanded;
            contentView.minItemSize = itemSize;

            nodeScrollView.Add(row);
            nodeRowCache.Add(node, row);

            row.onNodeChecked -= OnNodeChecked;
            row.onNodeChecked += OnNodeChecked;

            row.onNodeExpanded -= OnNodeExpanded;
            row.onNodeExpanded += OnNodeExpanded;

            contentView.onGridViewSelected -= OnGridViewSelected;
            contentView.onGridViewSelected += OnGridViewSelected;
        }

        void RemoveNodeView(BaseNode node)
        {
            bool succ = nodeRowCache.TryGetValue(node, out TJAIBlackboardRow row);
            if (!succ)
                return;

            row.onNodeChecked -= OnNodeChecked;
            row.onNodeExpanded -= OnNodeExpanded;
            row.contentView.onGridViewSelected -= OnGridViewSelected;

            nodeScrollView.Remove(row);
            nodeRowCache.Remove(node);
        }

        void UpdateNodeView(BaseNode node, bool refreshGridView = true)
        {
            bool succ = nodeRowCache.TryGetValue(node, out TJAIBlackboardRow row);
            if (!succ)
                return;

            row.expanded = history.assetsCache?[node].expanded ?? false;
            row.titleView.Refresh();
            if (refreshGridView)
                row.contentView.Refresh();
        }

        void UpdateRowLayouts()
        {
            blackboardLayouts = nodeScrollView.Children().Select(c => c.layout).ToList();
        }

        int GetInsertIndexFromMousePosition(Vector2 pos, out bool isOutOfBound)
        {
            pos = nodeScrollView.WorldToLocal(pos);
            // We only need to look for y axis;
            float mousePos = pos.y + nodeScrollView.scrollOffset.y;

            isOutOfBound = pos.y > nodeScrollView.layout.height || pos.y < 0;

            if (mousePos < 0)
                return 0;

            int index = 0;
            foreach (var layout in blackboardLayouts)
            {
                if (mousePos > layout.yMin && mousePos < layout.yMax)
                    return index;
                index++;
            }

            return nodeScrollView.childCount - 1;
        }

        void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition, out bool isOutOfBound);
            var graphSelectionDragData = DragAndDrop.GetGenericData("DragSelection");

            if (graphSelectionDragData == null)
                return;

            foreach (var obj in graphSelectionDragData as List<ISelectable>)
            {
                if (obj is HistoryAssetsFieldView view)
                {
                    view.selected = true;
                    var blackBoardRow = view.parent.parent.parent.parent.parent.parent;

                    // Try to find the blackboard row
                    nodeScrollView.Remove(blackBoardRow);
                    nodeScrollView.Insert(newIndex, blackBoardRow);

                    if (isOutOfBound)
                        nodeScrollView.ScrollTo(blackBoardRow);

                    // For the moment, we only support single drag-and-drop once
                    break;
                }
            }
        }

        void OnDragPerform(DragPerformEvent evt)
        {
            graphView.RegisterCompleteObjectUndo("Reorder History Assets");

            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition, out bool isOutOfBound);
            var graphSelectionDragData = DragAndDrop.GetGenericData("DragSelection");
            foreach (var obj in graphSelectionDragData as List<ISelectable>)
            {
                if (obj is HistoryAssetsFieldView view)
                {
                    view.selected = false;

                    BaseNode node = view.node;
                    history.orderedNodes.Remove(node);
                    history.orderedNodes.Insert(newIndex, node);

                    break;
                }
            }
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (nodeScrollView.worldBound.Contains(evt.position))
                capabilities &= ~Capabilities.Movable;
        }

        // Workaround for missing PointerUpEvent when selecting gird items in HistoryAssetsView
        void OnPointerUp(PointerUpEvent evt)
        {
            capabilities |= Capabilities.Movable;

            if (evt.target != this)
                return;

            evt.StopImmediatePropagation();
            Vector2 pos = evt.position;
            int index = GetInsertIndexFromMousePosition(pos, out bool isOutOfBound);
            if (index < 0 || index >= nodeScrollView.childCount)
                return;

            var gridView = nodeScrollView.ElementAt(index).Q<GridView>();
            if (!isOutOfBound && gridView != null && gridView.worldBound.Contains(pos))
            {
                var newEvt = PointerUpEvent.GetPooled(evt);
                newEvt.target = gridView.scrollView.contentContainer;
                SendEvent(newEvt);
            }
            else
            {
                ClearSelection();
            }
        }

        void OnViewClosed(DetachFromPanelEvent evt)
        {
            foreach (var row in nodeRowCache.Values)
            {
                row.onNodeChecked -= OnNodeChecked;
                row.onNodeExpanded -= OnNodeExpanded;
                row.contentView.onGridViewSelected -= OnGridViewSelected;
            }
            Undo.undoRedoPerformed -= Refresh;
        }

        void OnGridItemSizeChanging(ChangingEvent<int> evt)
        {
            itemSize = evt.newValue;
            foreach (var row in nodeRowCache.Values)
            {
                row.contentView.minItemSize = evt.newValue;
            }
        }

        void ClearSelection(params BaseNode[] exceptionNodes)
        {
            globalSelector.SetValueWithoutNotify(CheckboxState.Unchecked);
            foreach (var row in nodeRowCache.Values)
            {
                if (exceptionNodes.Contains(row.node))
                    continue;
                row.nodeSelector.SetValueWithoutNotify(CheckboxState.Unchecked);
                row.contentView.gridView.ClearSelectionWithoutNotify();
            }
            globalSelectedIndices.Clear();
            globalSelectedItems.Clear();
            checkedNodeCount = 0;
        }

        void OnGridViewSelected(BaseNode node, IEnumerable<int> indices, bool additive, SelectState state, bool contextMenu, Vector2 contextMenuPos)
        {
            if (!additive)
                ClearSelection(node);

            if (state == SelectState.None)
            {
                globalSelectedIndices.Remove(node);
                globalSelectedItems.Remove(node);
            }
            else
            {
                var tmpIndices = indices.ToList();
                tmpIndices.Sort();
                globalSelectedIndices[node] = tmpIndices;
                globalSelectedItems[node] = tmpIndices.Select(i => history.assetsCache[node].IDToPreview[i]).ToList();
                // we guarantee the ascending order among selection indices in a node
            }

            // assign node checkbox and global checkbox without notify
            bool succ = nodeRowCache.TryGetValue(node, out TJAIBlackboardRow row);
            if (!succ)
                return;

            if (row.nodeSelector.value == CheckboxState.Checked && state != SelectState.All)
            {
                row.nodeSelector.SetValueWithoutNotify(CheckboxState.Unchecked);
                checkedNodeCount--;
            }
            else if (row.nodeSelector.value == CheckboxState.Unchecked && state == SelectState.All)
            {
                row.nodeSelector.SetValueWithoutNotify(CheckboxState.Checked);
                checkedNodeCount++;
            }

            globalSelector.SetValueWithoutNotify(checkedNodeCount == history.Count ? CheckboxState.Checked : CheckboxState.Unchecked);


            schedule.Execute(() =>
            {
                contextMenuAnchor.transform.position = content.WorldToLocal(contextMenuPos);
                showContextMenu = contextMenu;
            }).ExecuteLater(1);
        }

        void OnNodeChecked(BaseNode node, CheckboxState state)
        {
            bool succ = nodeRowCache.TryGetValue(node, out TJAIBlackboardRow row);
            if (!succ)
                return;

            var view = row.contentView;
            if (state == CheckboxState.Checked)
            {
                int N = view.gridView.itemsSource.Count;
                if (N > 0)
                {
                    IEnumerable<int> indices = Enumerable.Range(0, N);
                    view.gridView.SetSelectionWithoutNotify(indices);
                    globalSelectedIndices[node] = indices.ToList();
                    globalSelectedItems[node] = indices.Select(i => history.assetsCache[node].IDToPreview[i]).ToList();
                }

                checkedNodeCount++;
            }
            else if (state == CheckboxState.Unchecked)
            {
                view.gridView.ClearSelectionWithoutNotify();
                globalSelectedIndices.Remove(node);
                globalSelectedItems.Remove(node);

                checkedNodeCount--;
            }

            // assign global checkbox without notify
            globalSelector.SetValueWithoutNotify(checkedNodeCount == history.Count ? CheckboxState.Checked : CheckboxState.Unchecked);
        }

        void OnGlobalChecked(CheckboxState state)
        {
            foreach (var row in nodeRowCache.Values)
            {
                // Automatically invoke OnNodeChecked for each node
                row.nodeSelector.value = state;
            }
        }

        void OnNodeExpanded(BaseNode node, bool expanded)
        {
            if (history.IsRegistered(node))
            {
                graphView.RegisterCompleteObjectUndo("Expand History Assets");
                history.assetsCache[node].expanded = expanded;
            }
        }

        void BuildContextualMenu(GenericMenu menu)
        {
            if (globalSelectionCount == 0)
                return;

            menu.AddItem(new GUIContent("Export"), false, OnExport);

            if (globalSelectionCount == 1)
                menu.AddItem(new GUIContent("Restore"), false, OnRestore);
            else
                menu.AddDisabledItem(new GUIContent("Restore"));

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Delete"), false, OnDelete);
            //menu.AddItem(new GUIContent("Debug"), false, DebugGlobalSelection);
        }

        void DebugGlobalSelection()
        {
            string report = "[Global Selection] ";
            if (globalSelectedIndices.Count > 0)
            {
                foreach (var p in globalSelectedIndices)
                {
                    report += p.Key.GetCustomName() + ": ";
                    foreach (var i in p.Value)
                    {
                        report += i.ToString() + " ";
                    }
                    report += " || ";
                }
                report = report.Substring(0, report.Length - 4);
            }
            else
            {
                report += "(empty)";
            }

            Debug.Log(report);
        }

        void OnHistoryChanged(HistoryChangeType type, BaseNode[] nodes)
        {
            if (type == HistoryChangeType.Register)
            {
                foreach (var node in nodes)
                    AddNodeView(node);
            }
            else if (type == HistoryChangeType.Unregister)
            {
                foreach (var node in nodes)
                    RemoveNodeView(node);
            }
            else if (type == HistoryChangeType.Modify)
            {
                foreach (var node in nodes)
                    UpdateNodeView(node);
            }
            else if (type == HistoryChangeType.Rename)
            {
                foreach (var node in nodes)
                    UpdateNodeView(node, refreshGridView: false);
            }
            else
            {
                SDUtil.LogError("Unknown History Change Type!");
            }
        }

        void OnExpand(bool expanded)
        {
            graphView.RegisterCompleteObjectUndo("Expand History Assets");
            foreach (var p in nodeRowCache)
            {
                if (history.IsRegistered(p.Key))
                {
                    history.assetsCache[p.Key].expanded = expanded;
                    p.Value.expanded = expanded;
                }
            }
        }

        void OnExport()
        {
            switch (globalSelectionCount)
            {
                case 0: break;
                case 1:
                    {
                        StartCoroutine(ExportUtils.ExportArtifact(globalSelectedPair.Item1, globalSelectedPair.Item2));
                        break;
                    }
                default: StartCoroutine(ExportUtils.ExportArtifacts(globalSelectedItems)); break;
            }

            //DebugGlobalSelection();
            ClearSelection();
        }

        void OnDelete()
        {
            if (globalSelectionCount == 0)
                return;

            graphView.RegisterCompleteObjectUndo("Delete History Assets");
            history.RemoveSelectedAssets(globalSelectedItems);
            //DebugGlobalSelection();
            ClearSelection();
        }

        void OnRestore()
        {
            if (globalSelectionCount == 0)
                return;

            graphView.RegisterCompleteObjectUndo("Restore History Assets");
            (var node, var artifact) = globalSelectedPair;

            Unity.EditorCoroutines.Editor.EditorCoroutine coroutine;
            coroutine = StartCoroutine(node.RestoreHistory(artifact.Guid));

            graphView.graph.NotifyNodeChanged(node);

            ClearSelection();
        }

        public void SelectNodeTitle(BaseNode node)
        {
            if (nodeRowCache.TryGetValue(node, out var row))
            {
                nodeScrollView.ScrollTo(row);
                graphView.AddToSelection(row.titleView);
            }
        }



        Unity.EditorCoroutines.Editor.EditorCoroutine StartCoroutine(IEnumerator routine)
        {
            return Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutine(routine, graphView);
        }

    }

    class TJAIBlackboardRow : BlackboardRow
    {
        public BaseNode node { get; private set; }

        public UnityEngine.UIElements.Button expandButton;

        public Checkbox nodeSelector;

        public HistoryAssetsFieldView titleView;

        public HistoryAssetsContentView contentView;

        public event Action<BaseNode, CheckboxState> onNodeChecked;

        public event Action<BaseNode, bool> onNodeExpanded;

        public TJAIBlackboardRow(BaseNode node, HistoryAssetsFieldView item, HistoryAssetsContentView propertyView)
            : base(item, propertyView)
        {
            this.node = node;
            this.titleView = item;
            this.contentView = propertyView;

            nodeSelector = new Checkbox();
            nodeSelector.RegisterValueChangedCallback(evt => onNodeChecked?.Invoke(node, evt.newValue));
            var itemRow = this.Q(name: "itemRow");
            itemRow.Insert(0, nodeSelector);

            expandButton = this.Q<UnityEngine.UIElements.Button>(name: "expandButton");
            expandButton.clickable.clicked += RefreshState;
        }

        public void RefreshState()
        {
            // we delay the refresh to ensure that <expanded> is updated
            expandButton.schedule.Execute(() => { onNodeExpanded?.Invoke(node, expanded); }).ExecuteLater(1);
        }
    }
}
