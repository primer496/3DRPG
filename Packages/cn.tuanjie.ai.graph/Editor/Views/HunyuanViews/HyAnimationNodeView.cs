using System.Linq;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyAnimationWFNode))]
public class HyAnimationNodeView : TJAIBaseAssetNodeView
{
    private HyAnimationWFNode m_WfNode;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        m_WfNode = nodeTarget as HyAnimationWFNode;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var motionTypeDropdown = new DropdownField(m_WfNode.motionTypeMap.Keys.ToList(), 0)
        {
            label = "Motion Type", name = "motionTypeDropdown",
            tooltip = "角色动作类型（Stride-跨步，Fall-摔倒，Jump-跳跃，Kick-踢腿，Swing-挥击，Walk-步行，Run-跑步，Dance-跳舞）"
        };
        motionTypeDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(motionTypeDropdown);

        BindProperty<DropdownField, string, HyAnimationWFNode>("motionTypeDropdown", nameof(m_WfNode.motionType));

        base.Enable();
        RefreshExpandedState();
    }
}