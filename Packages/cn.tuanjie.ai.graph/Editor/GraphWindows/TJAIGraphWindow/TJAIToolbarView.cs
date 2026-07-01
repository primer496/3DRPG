using GraphProcessor;
using UnityEngine.UIElements;
using NodeStatus = GraphProcessor.NodeStatus;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace UnityEditor.AIGraph
{
    public class TJAIToolbarView : ToolbarView
    {
        TJAIGraphProcessorView processorView;

        public TJAIToolbarView(BaseGraphView graphView) : base(graphView)
        {
        }

        protected override void AddButtons()
        {
            
            //AddButton(LocalizationManager.Instance.GetLocalizedText("Save"), graphView.SaveGraphToDisk);
            //AddToggle("Always Update", false, OnAlwaysUpdate);

            bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
            showParameters = AddToggle(LocalizationManager.Instance.GetLocalizedText("Show Parameters"), exposedParamsVisible, (v) => graphView.ToggleView< ExposedParameterView>());

            bool historyAssetsVisible = graphView.GetPinnedElementStatus<HistoryAssetsView>() != Status.Hidden;
            showParameters = AddToggle(LocalizationManager.Instance.GetLocalizedText("Show History Assets"), historyAssetsVisible, (v) => graphView.ToggleView<HistoryAssetsView>());

            graphView.OpenPinned(typeof(TJAIGraphProcessorView));
            var processorView = graphView.Q<TJAIGraphProcessorView>();
            AddButton(LocalizationManager.Instance.GetLocalizedText("Show In Project"), () => EditorGUIUtility.PingObject(graphView.graph), 2);
            AddButton(LocalizationManager.Instance.GetLocalizedText("Center"), graphView.ResetPositionAndZoom, 2);

            AddButton(LocalizationManager.Instance.GetLocalizedText("Run Step"), () => processorView.OnRunStep(), 1);
            AddButton(LocalizationManager.Instance.GetLocalizedText("Run All"), () => processorView.OnRunAll(), 1);
            AddButton(LocalizationManager.Instance.GetLocalizedText("Pause"), () => processorView.OnPause(), 1);
            AddButton(LocalizationManager.Instance.GetLocalizedText("Reset"), () => processorView.OnReset(), 1);


            
            //AddButton(LocalizationManager.Instance.GetLocalizedText("Debug"), () => {  }, false);
        }

        public void DebugReport(BaseGraph graph, NodeStatus ns)
        {
            UnityEngine.Debug.Log("debug report");
            foreach (var node in graph.nodes)
            {
                string msg = ns == NodeStatus.Error ? $"{node.GetCustomName()} error" : "";
                node.UpdateStatus(ns, msg);
            }
        }
    }
}