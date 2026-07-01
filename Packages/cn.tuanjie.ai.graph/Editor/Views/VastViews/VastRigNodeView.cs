using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastRigNode))]
public class VastRigNodeView : TJAIBaseAssetNodeView
{
    private VastRigNode node;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as VastRigNode;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var uxml = Resources.Load<VisualTreeAsset>(
            "uxml/VastRigNodeView");
        uxml.CloneTree(controlsContainer);

        BindProperty<DropdownField, string, VastRigNode>("modelVersionDropdown", nameof(node.modelVersion));
        BindProperty<DropdownField, string, VastRigNode>("rigMethodDropdown", nameof(node.rigMethod));
        previewSettings.Add("Rigging");

        base.Enable();

        RefreshExpandedState();
    }
}