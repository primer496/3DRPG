using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyImageFlexibilityConsistencyNode))]
public class HyImageFlexibilityConsistencynNodeView : TJAIBaseAssetNodeView
{
    private HyImageFlexibilityConsistencyNode node;
    private IntegerField widthField;
    private IntegerField heightField;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as HyImageFlexibilityConsistencyNode;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        widthField = new IntegerField("Width")
        {
            name = "widthField"
        };
        heightField = new IntegerField("Height")
        {
            name = "heightField"
        };
        controlsContainer.Add(widthField);
        controlsContainer.Add(heightField);

        BindProperty<IntegerField, int, HyImageFlexibilityConsistencyNode>(widthField.name, nameof(node.width));
        BindProperty<IntegerField, int, HyImageFlexibilityConsistencyNode>(heightField.name, nameof(node.height));

        base.Enable();
        RefreshExpandedState();
    }
}