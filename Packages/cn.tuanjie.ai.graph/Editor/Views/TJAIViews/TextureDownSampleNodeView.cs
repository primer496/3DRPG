using System.Linq;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(TextureDownSampleNode))]
public class TextureDownSampleNodeView : SDNodeView
{
    private TextureDownSampleNode m_Node;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        m_Node = nodeTarget as TextureDownSampleNode;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        var maxSizeDropdown = new DropdownField(m_Node.maxSize.Keys.ToList(), 5)
        {
            label = "Max Size",
            name = "maxSizeDropdown",
            tooltip = "Maximum Image Size after Down Sampling"
        };
        maxSizeDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(maxSizeDropdown);

        BindProperty<DropdownField, string, TextureDownSampleNode>("maxSizeDropdown", nameof(m_Node.currMaxSize));

        base.Enable();
        RefreshExpandedState();
    }
}