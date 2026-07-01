using System.Linq;
using GraphProcessor;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(HyImageGameEditNode))]
    public class HyImageGameEditNodeView : TJAIBaseAssetNodeView
    {
        private HyImageGameEditNode node;
        private UnityEngine.UIElements.TextField textArea;

        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as HyImageGameEditNode;
            var sizeDropdown = new DropdownField(node.sizes, 0)
            {
                label = "Target Language Type",
                name = "TargetDropdown",
                tooltip = "选择输出图片的大小"
            };
            sizeDropdown.AddToClassList("vast-dropdown");
            controlsContainer.Add(sizeDropdown);
            sizeDropdown.RegisterValueChangedCallback(evt =>
            {
                node.size = evt.newValue;
            });

            var foldout = new Foldout()
            {
                text = "Output Revised Prompt",
                name = "revisedPromptFoldout"
            };
            textArea = new UnityEngine.UIElements.TextField(-1, true, false, '*')
            {
                name = "revisedPromptField",
                isReadOnly = true,
                value = node.prompt
            };
            foldout.Add(textArea);
            controlsContainer.Add(foldout);

            node.onProcessed += OnRevisedPromptChanged;
        }

        void OnRevisedPromptChanged()
        {
            textArea.value = node.queryResult;
        }
    }
}
