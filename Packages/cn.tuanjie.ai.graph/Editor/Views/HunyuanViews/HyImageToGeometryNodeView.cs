using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(HyImageToGeometryNode))]
    public class HyImageToGeometryNodeView : TJAIBaseAssetNodeView
    {
        private HyImageToGeometryNode node;
        private SliderRangeIntegerField faceCountSlider;

        public override void Enable()
        {
            node = nodeTarget as HyImageToGeometryNode;
            if (node == null) return;

            faceCountSlider = new SliderRangeIntegerField("Face Count", 1000, 500000)
            {
                value = node.faceCount,
                tooltip = "The desired polygon count for the generated geometric model"
            };
            controlsContainer.Add(faceCountSlider);

            BindProperty<SliderRangeIntegerField, int, HyImageToGeometryNode>(faceCountSlider.name, nameof(node.faceCount));

            base.Enable();
            RefreshExpandedState();
        }
    }
}