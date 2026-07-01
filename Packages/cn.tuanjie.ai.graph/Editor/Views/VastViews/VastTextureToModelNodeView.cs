using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastTextureModelNode))]
public class VastTextureModelNodeView : TJAIBaseAssetNodeView
{
    private VastTextureModelNode node;
    private VisualElement textureContainer;

    public override void Enable()
    {
        node = nodeTarget as VastTextureModelNode;
        if (node == null) return;
        
//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var uxml = Resources.Load<VisualTreeAsset>(
            "uxml/VastModelNodeView");
        uxml.CloneTree(controlsContainer);
                
        textureContainer = controlsContainer.Q<VisualElement>("textureContainer");
        
        var modelVersionDropdown = controlsContainer.Q<DropdownField>("modelVersionDropdown");
        modelVersionDropdown.choices = node.modelVersionChoices;
        BindProperty<DropdownField, string, VastTextureModelNode>("modelVersionDropdown", nameof(node.modelVersion));
        BindProperty<DropdownField, string, VastTextureModelNode>(
            "compressionTypeDropdown", nameof(node.compressionType), OnCompressionTypeChanged);
        BindProperty<Toggle, bool, VastTextureModelNode>(
            "textureToggle", nameof(node.enableTexturing), OnTextureToggleChanged);
        BindProperty<DropdownField, string, VastTextureModelNode>("textureQualityDropdown", nameof(node.textureQuality));
        BindProperty<DropdownField, string, VastTextureModelNode>("textureAlignmentDropdown", nameof(node.textureAlignment));
        BindProperty<IntegerField, int, VastTextureModelNode>("textureSeedField", nameof(node.textureSeed));
        
        base.Enable();
        RefreshExpandedState();
    }

    void OnCompressionTypeChanged(ChangeEvent<string> evt)
    {
        node.compressionType = evt.newValue == "None" ? "" : evt.newValue;
    }

    void OnTextureToggleChanged(ChangeEvent<bool> evt)
    {
        textureContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        node.enableTexturing = evt.newValue;
    }
}