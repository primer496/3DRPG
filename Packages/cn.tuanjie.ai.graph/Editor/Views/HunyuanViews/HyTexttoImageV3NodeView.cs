using System.Collections.Generic;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyTexttoImageV3Node))]
public class HyTexttoImageV3NodeView : TJAIBaseAssetNodeView
{
    private HyTexttoImageV3Node node;
    private DropdownField sizeDropdown;
    private TextField revisedPromptField;

    private readonly List<string> sizeChoices = new()
    {
        "640x1408", "704x1344", "768x1280", "832x1216", "896x1152",
        "960x1088", "1024x1024", "1088x960", "1152x896",
        "1216x832", "1280x768", "1344x704", "1408x640"
    };

    public override void Enable()
    {
        node = nodeTarget as HyTexttoImageV3Node;
        if (node == null) return;

        sizeDropdown = new DropdownField(sizeChoices, 0)
        {
            label = "Size",
            name = "sizeDropdown"
        };
        sizeDropdown.AddToClassList("vast-dropdown");

        controlsContainer.Add(sizeDropdown);

        BindProperty<DropdownField, string, HyTexttoImageV3Node>(sizeDropdown.name, nameof(node.size));

        // show revised prompt
        var divider = new VisualElement();
        divider.AddToClassList("control-container-divider");
        var foldout = new Foldout()
        {
            text = "Output Revised Prompt",
            name = "revisedPromptFoldout"
        };
        revisedPromptField = new TextField(-1, true, false, '*')
        {
            name = "revisedPromptField",
            isReadOnly = true,
            value = node.revisedPrompt
        };
        foldout.Add(revisedPromptField);
        controlsContainer.Add(divider);
        controlsContainer.Add(foldout);
        node.onProcessed += OnRevisedPromptChanged;

        base.Enable();
        RefreshExpandedState();
    }

    void OnRevisedPromptChanged()
    {
        revisedPromptField.value = node.revisedPrompt;
    }
}