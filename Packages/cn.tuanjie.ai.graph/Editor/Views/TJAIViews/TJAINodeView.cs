using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(TJAIBaseAssetNode))]
    public class TJAIBaseAssetNodeView : SDNodeView
    {
        protected new TJAIBaseAssetNode nodeTarget => base.nodeTarget as TJAIBaseAssetNode;

        protected VisualElement historyContainer;
        protected Toggle saveHistoryToggle;
        protected Button locateHistoryButton;
        protected Button clearHistoryButton;
        protected Label costTimeLabel;
        protected Label costTokenLabel;

        public override void Enable()
        {
            // previewSettings.Add("Footnote");
            base.Enable();

            historyContainer = new VisualElement() { name = "historyContainer" };
            historyContainer.style.flexDirection = FlexDirection.Row;
            historyContainer.style.width = StyleKeyword.Auto;
            historyContainer.style.flexGrow = 1;
            historyContainer.style.marginLeft = 0;
            historyContainer.style.marginRight = 0;

            if (nodeTarget.allowHistory)
            {
                saveHistoryToggle = new Toggle(LocalizationManager.Instance.GetLocalizedText("Save History"));
                saveHistoryToggle.style.alignSelf = Align.Center;
                saveHistoryToggle.style.marginLeft = 5;
                saveHistoryToggle.Q<Label>().style.minWidth = 60;
                saveHistoryToggle.SetValueWithoutNotify(nodeTarget.saveHistory);
                saveHistoryToggle.RegisterValueChangedCallback(ToggleHistoryRegistration);
                historyContainer.Add(saveHistoryToggle);

                VisualElement historyRowRight = new VisualElement();
                historyRowRight.style.flexDirection = FlexDirection.Row;
                historyRowRight.style.justifyContent = Justify.FlexEnd;
                historyRowRight.style.flexGrow = 1;
                locateHistoryButton = new Button(LocateHistory);
                locateHistoryButton.text = "Locate";
                locateHistoryButton.style.visibility = saveHistoryToggle.value ? Visibility.Visible : Visibility.Hidden;
                historyRowRight.Add(locateHistoryButton);
                clearHistoryButton = new Button(ClearHistory);
                clearHistoryButton.text = "Clear";
                clearHistoryButton.style.visibility = saveHistoryToggle.value ? Visibility.Visible : Visibility.Hidden;
                historyRowRight.Add(clearHistoryButton);
                historyContainer.Add(historyRowRight);

                controlsContainer.Insert(0, historyContainer);
            }
            // time cost tooltip
            if (nodeTarget.taskCostTime > 0)
            {
                costTimeLabel = new Label($"This task may take {nodeTarget.taskCostTime} minutes to run.")
                {
                    name = "costTimeLabel",
                    style = { display = DisplayStyle.None }
                };
                mainContainer.Add(costTimeLabel);
            }
            // token cost tooltip
            if (nodeTarget.taskCostToken > 0)
            {
                costTokenLabel = new Label($"{nodeTarget.taskCostToken} token/task")
                {
                    name = "costTokenLabel"
                };
                Insert(0, costTokenLabel);

                VisualElement selectionBorder = this.Q("selection-border");
                VisualElement nodeBorder = this.Q("node-border");

                if (selectionBorder != null && nodeBorder != null && nodeBorder.parent != null)
                {
                    schedule.Execute(() => {
                        selectionBorder.style.height = nodeBorder.parent.localBound.height;
                    }).Every(17);
                }
            }
        }

        protected override void OnSave()
        {
            nodeTarget.TryExportCurrent();
        }

        void ToggleHistoryRegistration(ChangeEvent<bool> evt)
        {
            owner.RegisterCompleteObjectUndo("Toggle History Registration");
            nodeTarget.saveHistory = evt.newValue;
            if (nodeTarget.saveHistory)
            {
                locateHistoryButton.style.visibility = Visibility.Visible;
                clearHistoryButton.style.visibility = Visibility.Visible;
                nodeTarget.RegisterToHistory();
            }
            else
            {
                locateHistoryButton.style.visibility = Visibility.Hidden;
                clearHistoryButton.style.visibility = Visibility.Hidden;
                nodeTarget.UnregisterFromHistory();
            }
        }

        void LocateHistory()
        {
            if(owner.GetPinnedElementStatus<HistoryAssetsView>() == DropdownMenuAction.Status.Hidden)
            {
                owner.OpenPinned<HistoryAssetsView>();
            }

            var view = owner.Q<HistoryAssetsView>();
            if (view == null)
                return;

            view.schedule.Execute(() => view.SelectNodeTitle(nodeTarget)).ExecuteLater(1);
        }

        void ClearHistory()
        {
            owner.RegisterCompleteObjectUndo("Clear History Assets");
            nodeTarget.ClearHistory();
        }
    }
}
