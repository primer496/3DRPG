using System.Linq;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyMotionRetargetNode))]
public class HyMotionRetargetNodeView : TJAIBaseAssetNodeView
{
    private HyMotionRetargetNode node;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as HyMotionRetargetNode;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var motionTypeDropdown = new DropdownField(node.motionTypeMap.Keys.ToList(), 0)
        {
            label = "Motion Type", name = "motionTypeDropdown",
            tooltip = "角色动作类型（9-跨步，10-摔倒，11-跳跃，12-踢腿，13-挥击，14-步行，15-跑步，16-跳舞）"
        };
        motionTypeDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(motionTypeDropdown);

        BindProperty<DropdownField, string, HyMotionRetargetNode>("motionTypeDropdown", nameof(node.motionType));

        base.Enable();
        RefreshExpandedState();
    }
}