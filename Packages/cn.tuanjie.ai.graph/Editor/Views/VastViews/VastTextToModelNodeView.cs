using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastTextToModelNode))]
public class VastTextToModelNodeView : TJAIBaseAssetNodeView
{
    private VastTextToModelNode node;
    private Toggle quadToggle;
    private Toggle textureToggle;
    private VisualElement textureContainer;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as VastTextToModelNode;
        
//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var uxml = Resources.Load<VisualTreeAsset>(
            "uxml/VastModelNodeView");
        uxml.CloneTree(controlsContainer);
        
        textureContainer = controlsContainer.Q<VisualElement>("textureContainer");
        quadToggle = controlsContainer.Q<Toggle>("quadToggle");
        textureToggle = controlsContainer.Q<Toggle>("textureToggle");
        
        BindProperty<Toggle, bool, VastTextToModelNode>("quadToggle", nameof(node.enableQuadMesh));
        // BindProperty<Toggle, bool, VastTextToModelNode>("generatePartsToggle", nameof(node.generateParts),
        //     OnGeneratePartsToggleChanged);
        BindProperty<DropdownField, string, VastTextToModelNode>("modelVersionDropdown", nameof(node.modelVersion));
        BindProperty<DropdownField, string, VastTextToModelNode>(
            "compressionTypeDropdown", nameof(node.compressionType), OnCompressionTypeChanged);
        BindProperty<DropdownField, string, VastTextToModelNode>("modelStyleDropdown", 
            nameof(node.modelStyle), OnModelStyleChanged);
        BindProperty<Toggle, bool, VastTextToModelNode>(
            "textureToggle", nameof(node.enableTexturing), OnTextureToggleChanged);
        BindProperty<DropdownField, string, VastTextToModelNode>("textureQualityDropdown", nameof(node.textureQuality));
        
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