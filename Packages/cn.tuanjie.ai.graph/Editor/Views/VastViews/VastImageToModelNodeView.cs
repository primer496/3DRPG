using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastImageToModelNode))]
public class VastImageToModelNodeView : TJAIBaseAssetNodeView
{
    private VastImageToModelNode node;
    private Toggle quadToggle;
    private Toggle textureToggle;
    private VisualElement textureContainer;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as VastImageToModelNode;
        
//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var uxml = Resources.Load<VisualTreeAsset>(
            "uxml/VastModelNodeView");
        uxml.CloneTree(controlsContainer);
        
        textureContainer = controlsContainer.Q<VisualElement>("textureContainer");        
        quadToggle = controlsContainer.Q<Toggle>("quadToggle");
        textureToggle = controlsContainer.Q<Toggle>("textureToggle");
        
        BindProperty<Toggle, bool, VastImageToModelNode>("quadToggle", nameof(node.enableQuadMesh));
        // BindProperty<Toggle, bool, VastImageToModelNode>("generatePartsToggle", nameof(node.generateParts),
        //     OnGeneratePartsToggleChanged);
        BindProperty<DropdownField, string, VastImageToModelNode>("modelVersionDropdown", nameof(node.modelVersion));
        BindProperty<DropdownField, string, VastImageToModelNode>(
            "compressionTypeDropdown", nameof(node.compressionType), OnCompressionTypeChanged);
        BindProperty<DropdownField, string, VastImageToModelNode>("modelStyleDropdown", 
            nameof(node.modelStyle), OnModelStyleChanged);
        BindProperty<DropdownField, string, VastImageToModelNode>("orientationDropdown", nameof(node.orientation));
        BindProperty<Toggle, bool, VastImageToModelNode>(
            "textureToggle", nameof(node.enableTexturing), OnTextureToggleChanged);
        BindProperty<DropdownField, string, VastImageToModelNode>("textureQualityDropdown", nameof(node.textureQuality));
        BindProperty<DropdownField, string, VastImageToModelNode>("textureAlignmentDropdown", nameof(node.textureAlignment));
        BindProperty<IntegerField, int, VastImageToModelNode>("textureSeedField", nameof(node.textureSeed));
        
        base.Enable();
        RefreshExpandedState();
    }

    void OnCompressionTypeChanged(ChangeEvent<string> evt)
    {
        node.compressionType = evt.newValue == "None" ? "" : evt.newValue;
        NotifyNodeChanging();
    }

    void OnTextureToggleChanged(ChangeEvent<bool> evt)
    {
        textureContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        node.enableTexturing = evt.newValue;
        NotifyNodeChanging();
    }
    
    void OnGeneratePartsToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            node.enableQuadMesh = false;
            quadToggle.SetValueWithoutNotify(false);
            quadToggle.SetEnabled(false);
            node.enableTexturing = false;
            textureToggle.SetValueWithoutNotify(false);
            textureContainer.style.display = DisplayStyle.None;
            textureToggle.SetEnabled(false);
        }
        else
        {
            quadToggle.SetEnabled(true);
            textureToggle.SetEnabled(true);
        }
        node.generateParts = evt.newValue;
        NotifyNodeChanging();
    }
    void OnModelStyleChanged(ChangeEvent<string> evt)
    {
        node.modelStyle = evt.newValue == "None" ? string.Empty : evt.newValue;
        NotifyNodeChanging();
    }
}