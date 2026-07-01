using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack Material List")]
    public class PackMaterialListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<Material> inputList;

        [Output(name = "List")] public List<Material> outputList;

        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<Material>), typeof(Material) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<Material>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(Material).IsAssignableFrom(edgeType))
                {
                    inputList.Add((Material)edge.passThroughBuffer);
                }
                else if (typeof(List<Material>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Material> { Count: > 0 } list)
                        inputList.AddRange(list);
                }
            }
        }

        public override bool isRenamable => true;

        public override void Process()
        {
            outputList = inputList;
        }
    }
}