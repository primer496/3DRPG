using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(HyViewsToGeometryNode))]
    public class HyViewsToGeometryNodeView : TJAIBaseAssetNodeView
    {
        private HyViewsToGeometryNode node;
        private SliderRangeIntegerField faceCountSlider;

        public override void Enable()
        {
            node = nodeTarget as HyViewsToGeometryNode;
            if (node == null) return;

            faceCountSlider = new SliderRangeIntegerField("Face Count", 1000, 500000)
            {
                value = node.faceCount,
                tooltip = "The desired polygon count for the generated geometric model"
            };
            controlsContainer.Add(faceCountSlider);

            BindProperty<SliderRangeIntegerField, int, HyViewsToGeometryNode>(faceCountSlider.name, nameof(node.faceCount));

            base.Enable();
            RefreshExpandedState();
        }
    }
}