using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastMultiviewToModelNode))]
public class VastMultiviewToModelNodeView : TJAIBaseAssetNodeView
{
    private VastMultiviewToModelNode node;
    private Toggle quadToggle;
    private Toggle textureToggle;
    private VisualElement textureContainer;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as VastMultiviewToModelNode;
        
//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var uxml = Resources.Load<VisualTreeAsset>(
            "uxml/VastModelNodeView");
        uxml.CloneTree(controlsContainer);
        
        textureContainer = controlsContainer.Q<VisualElement>("textureContainer");      
        quadToggle = controlsContainer.Q<Toggle>("quadToggle");
        textureToggle = controlsContainer.Q<Toggle>("textureToggle");
        
        BindProperty<Toggle, bool, VastMultiviewToModelNode>("quadToggle", nameof(node.enableQuadMesh));
        // BindProperty<Toggle, bool, VastMultiviewToModelNode>("generatePartsToggle", nameof(node.generateParts),
        //     OnGeneratePartsToggleChanged);
        
        BindProperty<DropdownField, string, VastMultiviewToModelNode>("modelVersionDropdown", nameof(node.modelVersion));
        BindProperty<DropdownField, string, VastMultiviewToModelNode>(
            "compressionTypeDropdown", nameof(node.compressionType), OnCompressionTypeChanged);
        BindProperty<DropdownField, string, VastMultiviewToModelNode>("orientationDropdown", nameof(node.orientation));
        BindProperty<Toggle, bool, VastMultiviewToModelNode>(
            "textureToggle", nameof(node.enableTexturing), OnTextureToggleChanged);
        BindProperty<DropdownField, string, VastMultiviewToModelNode>("textureQualityDropdown", nameof(node.textureQuality));
        BindProperty<DropdownField, string, VastMultiviewToModelNode>("textureAlignmentDropdown", nameof(node.textureAlignment));
        BindProperty<IntegerField, int, VastMultiviewToModelNode>("textureSeedField", nameof(node.textureSeed));
        
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
    }
}