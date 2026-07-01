using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyImageStyleSwitchNode))]
public class HyImageStyleSwitchNodeView : TJAIBaseAssetNodeView
{
    private HyImageStyleSwitchNode node;
    private DropdownField styleDropdown;
    
    public override void Enable()
    {
        node = nodeTarget as HyImageStyleSwitchNode;
        if (node == null) return;

        styleDropdown = new DropdownField(node.styleChoices, 0)
        {
            label = "Style", name = "styleDropdown"
        };
        styleDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(styleDropdown);

        BindProperty<DropdownField, string, HyImageStyleSwitchNode>(styleDropdown.name, nameof(node.style));

        base.Enable();
        RefreshExpandedState();
    }
}