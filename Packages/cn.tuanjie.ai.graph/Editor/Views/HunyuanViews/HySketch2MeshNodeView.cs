using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(HySketch2MeshNode))]
    public class HySketch2MeshNodeView : TJAIBaseAssetNodeView
    {
        private HySketch2MeshNode node;
        private SliderRangeIntegerField faceCountSlider;

        public override void Enable()
        {
            node = nodeTarget as HySketch2MeshNode;
            if (node == null) return;

            faceCountSlider = new SliderRangeIntegerField("Face Count", 50000, 500000)
            {
                value = node.faceCount,
                tooltip = "The desired polygon count for the generated geometric model"
            };
            controlsContainer.Add(faceCountSlider);

            BindProperty<SliderRangeIntegerField, int, HySketch2MeshNode>(faceCountSlider.name, nameof(node.faceCount));

            base.Enable();
            RefreshExpandedState();
        }
    }
}