using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    class HistoryAssetsContentView : VisualElement
    {
        public enum SelectState
        {
            None,
            Partial,
            All
        }

        protected TJAIGraphView graphView;

        [SerializeField]
        protected HistoryAssets history;

        [SerializeField]
        protected Dictionary<previewData, HistoryTextureItemView> viewCache;

        protected HashSet<int> uiReadyCache;

        public BaseNode node { get; private set; }

        public GridView gridView;

        public event Action<BaseNode, IEnumerable<int>, bool, SelectState, bool, Vector2> onGridViewSelected;

        public readonly static int initMinItemSize = 100;
        public readonly static int lowValueOfMinItemSize = 50;
        public readonly static int highValueOfMinItemSize = 400;

        protected int m_MinItemSize = initMinItemSize;

        public int minItemSize
        {
            get { return m_MinItemSize; }
            set
            {
                if(m_MinItemSize != value)
                {
                    m_MinItemSize = value;
                    OnGeometryChanged();
                }
            }
        }

        protected bool additive { get; set; }

        protected bool mouseRight { get; set; }

        protected Vector2 mousePosition;

        public HistoryAssetsContentView(TJAIGraphView graphView, HistoryAssets history, BaseNode node)
        {
            this.graphView = graphView;
            this.history = history;
            
            this.node = node;
            AddToClassList("history-assets-grid");

            viewCache = new();
            uiReadyCache = new();
             
            gridView = new GridView();
            Add(gridView);
            gridView.SupportDrag(false);
            gridView.selectionType = SelectionType.Multiple;
            gridView.makeItem = MakeItemView;
            gridView.bindItem = BindGridItem;
            gridView.unbindItem = UnbindGridItem;
            gridView.onSelectionChange += OnGridViewSelectionChanged;
            gridView.columnCount = 4;
            gridView.itemHeight = 150;
            gridView.RegisterCallback<GeometryChangedEvent>(evt => { OnGeometryChanged(); });

            RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);

            RegisterCallback<AttachToPanelEvent>(evt => history.onRefreshHistoryView += Refresh);
            RegisterCallback<DetachFromPanelEvent>(evt => history.onRefreshHistoryView -= Refresh);
            Refresh();
        }

        public void Refresh()
        {
            if (history == null || !history.IsRegistered(node))
                return;
            // We copy the source list to facilitate further extension
            gridView.itemsSource = new List<previewData>(history.assetsCache[node].IDToPreview);           
            gridView.Refresh();
            uiReadyCache.Clear();

            OnGeometryChanged();
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            additive = evt.actionKey || evt.shiftKey;
            mouseRight = evt.button == (int)MouseButton.RightMouse;
            mousePosition = evt.position;
            Vector2 localPos = gridView.WorldToLocal(evt.position);

            if (mouseRight)
            {
                int pointedIndex = gridView.GetIndexByPosition(localPos);
                if (gridView.selectedIndices.Contains(pointedIndex))
                    additive = true;
            }

            uiReadyCache.Clear();
            foreach (var idx in gridView.selectedIndices)
                uiReadyCache.Add(idx);
        }

        VisualElement MakeItemView()
        {
            VisualElement ve = new VisualElement();
            ve.AddToClassList("history-assets-griditem");
            return ve;
        }

        void BindGridItem(VisualElement ve, int index)
        {
            var previewData = gridView.itemsSource[index] as previewData;

            // we use <viewCache> to avoid redundant construction
            if (!viewCache.TryGetValue(previewData, out var view))
            {
                view = new HistoryTextureItemView(previewData.StaticPreview as Texture2D, graphView, previewData.HasInfo, previewData.Settings);
                view.AddToClassList("history-assets-griditem__view");
                viewCache.Add(previewData, view);
            }

            ve.Clear();
            ve.Add(view);

            ve.RegisterCallback<TransitionEndEvent>(OnBorderColorChangeFinished);
        }

        void UnbindGridItem(VisualElement ve, int index)
        {
            ve.UnregisterCallback<TransitionEndEvent>(OnBorderColorChangeFinished);
        }

        void OnBorderColorChangeFinished(TransitionEndEvent evt)
        {
            var target = evt.target as VisualElement;


            // Modifying <border-color> property will trigger 4 Transitions events, namely
            // <border-left-color>, <border-right-color>, <border-top-color> and <border-bottom-color>.
            // We just pick one of them for counting.
            if (evt.stylePropertyNames.First().ToString() == "border-left-color")
            {
                int index = gridView.GetIndexByPosition(gridView.WorldToLocal(target.worldBound.center));
                if (uiReadyCache.Contains(index))
                    uiReadyCache.Remove(index);
                else
                    uiReadyCache.Add(index);
            }
            target.MarkDirtyRepaint();
        }

        int waitTime;
        readonly int maxWaitTime = 50;
        void OnGridViewSelectionChanged(IEnumerable<object> selectedItems)
        {
            SelectState ss = SelectState.Partial;
            if (selectedItems.Count() == gridView.itemsSource.Count)
                ss = SelectState.All;
            else if (selectedItems.Count() == 0)
                ss = SelectState.None;

            waitTime = 0;
            BubbleUpSelectionChanges(ss, selectedItems);
        }

        void BubbleUpSelectionChanges(SelectState ss, IEnumerable<object> selectedItems)
        {
            if (uiReadyCache.Count == selectedItems.Count())
            {
                bool showContextMenu = mouseRight && ValidateMousePosition(mousePosition);
                onGridViewSelected?.Invoke(node, gridView.selectedIndices, additive, ss, showContextMenu, mousePosition);
            }
            else
            {
                // waiting for ui changes done
                if(++waitTime >= maxWaitTime)
                {
                    throw new Exception("Selection UI Error");
                }
                schedule.Execute(() => BubbleUpSelectionChanges(ss, selectedItems)).ExecuteLater(10);
            }
        }

        bool ValidateMousePosition(Vector2 mousePosition)
        {
            int currentIndex = gridView.GetIndexByPosition(gridView.WorldToLocal(mousePosition));

            if (!gridView.worldBound.Contains(mousePosition) || currentIndex < 0 || currentIndex >= gridView.itemsSource.Count)
                return false;

            return true;
        }

        void OnGeometryChanged()
        {
            int columns = Mathf.Max(1, Mathf.FloorToInt(gridView.layout.width / minItemSize));
            int itemSize = Mathf.RoundToInt(gridView.layout.width / columns);
            float viewMinHeight = Mathf.CeilToInt(gridView.itemsSource.Count / (float)columns) * itemSize;

            gridView.columnCount = columns;
            gridView.itemHeight = itemSize;
            gridView.style.minHeight = viewMinHeight;
        }
    }
}
