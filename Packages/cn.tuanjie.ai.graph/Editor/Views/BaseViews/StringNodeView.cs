using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(StringNode))]
    public class StringNodeView : BaseTJAINodeView
    {
        private TextField textArea;
        private StringNode node;
        Button togglePreviewButton = null;

        public override void Enable()
        {
            base.Enable();

            node = nodeTarget as StringNode;

            togglePreviewButton = new Button(() =>
            {
                node.isShowString = !node.isShowString;
                UpdatePreviewCollapseState();
            });
            togglePreviewButton.ClearClassList();
            togglePreviewButton.AddToClassList("PreviewToggleButton");
            controlsContainer.Add(togglePreviewButton);

            textArea = new TextField(-1, true, false, '*') { value = node.textFiledValue };
            textArea.Children().First().style.unityTextAlign = TextAnchor.UpperLeft;
            textArea.style.whiteSpace = WhiteSpace.Normal;
            textArea.style.height = float.NaN;
            textArea.UnregisterValueChangedCallback(OnTextChanged);
            textArea.RegisterValueChangedCallback(OnTextChanged);
            nodeTarget.onProcessed += () => textArea.value = node.textFiledValue;
            controlsContainer.Add(textArea);
        }

        void OnTextChanged(ChangeEvent<string> evt)
        {
            
            owner.RegisterCompleteObjectUndo("Edit string node");
            node.textFiledValue = evt.newValue;
            NotifyNodeChanging();
        }

        void UpdatePreviewCollapseState()
        {
            if (!node.isShowString)
            {
                if (controlsContainer.Contains(textArea))
                {
                    controlsContainer.Remove(textArea);
                }

                togglePreviewButton.RemoveFromClassList("Collapsed");
            }
            else
            {
                if (!controlsContainer.Contains(textArea))
                {
                    controlsContainer.Add(textArea);
                }

                togglePreviewButton.AddToClassList("Collapsed");
            }
        }

        public override void Disable()
        {
            base.Disable();
            OnExpandAction = null;
        }
    }
}