using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;

[NodeCustomEditor(typeof(HyAutoRiggingNode))]
public class HyAutoRiggingNodeView : TJAIBaseAssetNodeView
{
    private HyAutoRiggingNode node;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as HyAutoRiggingNode;
        previewSettings.Add("Rigging");

        base.Enable();

        RefreshExpandedState();
    }
}