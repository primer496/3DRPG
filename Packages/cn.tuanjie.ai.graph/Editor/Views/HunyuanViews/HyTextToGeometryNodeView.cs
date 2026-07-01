using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(HyTextToGeometryNode))]
    public class HyTextToGeometryNodeView : TJAIBaseAssetNodeView
    {
        private HyTextToGeometryNode node;
        private SliderRangeIntegerField faceCountSlider;

        public override void Enable()
        {
            node = nodeTarget as HyTextToGeometryNode;
            if (node == null) return;

            faceCountSlider = new SliderRangeIntegerField("Face Count", 1000, 500000)
            {
                value = node.faceCount,
                tooltip = "The desired polygon count for the generated geometric model"
            };
            controlsContainer.Add(faceCountSlider);

            BindProperty<SliderRangeIntegerField, int, HyTextToGeometryNode>(faceCountSlider.name, nameof(node.faceCount));

            base.Enable();
            RefreshExpandedState();
        }
    }
}