using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyLowpolyNode))]
public class HyLowpolyNodeView : TJAIBaseAssetNodeView
{
    private HyLowpolyNode node;
    private DropdownField polygonTypeDropdown;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as HyLowpolyNode;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);
        
        polygonTypeDropdown = new DropdownField(node.polygonTypeChoices, 0)
        {
            label = "Polygon Type", name = "polygonTypeDropdown",
            tooltip = "多边形类型，表示模型的表面由几边形图案构成。"
        };
        polygonTypeDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(polygonTypeDropdown);

        BindProperty<DropdownField, string, HyLowpolyNode>("polygonTypeDropdown", nameof(node.polygonType));

        base.Enable();
        RefreshExpandedState();
    }
}