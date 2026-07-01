using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastStylizeModelNode))]
public class VastStylizeModelNodeView : TJAIBaseAssetNodeView
{
    private VastStylizeModelNode node;
    private IntegerField blockSizeField;
    private DropdownField modelStyleDropdown;

    public override void Enable()
    {
        node = nodeTarget as VastStylizeModelNode;
        if (node == null) return;
        
//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        modelStyleDropdown = new DropdownField(node.modelStyleChoices, 0)
        {
            label = "Model Style", name = "modelStyleDropdown"
        };
        modelStyleDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(modelStyleDropdown);
        
        blockSizeField = new IntegerField("Block Size")
        {
            value = node.blockSize, name = "blockSizeField",
            style =
            {
                display = node.modelStyle == "minecraft" ? DisplayStyle.Flex : DisplayStyle.None
            }
        };
        controlsContainer.Add(blockSizeField);

        BindProperty<DropdownField, string, VastStylizeModelNode>("modelStyleDropdown", nameof(node.modelStyle),
            OnModelStyleChanged);
        BindProperty<IntegerField, int, VastStylizeModelNode>("blockSizeField", nameof(node.blockSize));
        
        base.Enable();
        RefreshExpandedState();
    }

    void OnModelStyleChanged(ChangeEvent<string> evt)
    {
        blockSizeField.style.display = evt.newValue == "minecraft" ? DisplayStyle.Flex : DisplayStyle.None;
        node.modelStyle = evt.newValue;
    }
}