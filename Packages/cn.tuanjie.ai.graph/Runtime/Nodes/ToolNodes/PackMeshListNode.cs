using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack Mesh List")]
    public class PackMeshListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<Mesh> inputList;

        [Output(name = "List")] public List<Mesh> outputList;

        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<Mesh>), typeof(Mesh) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<Mesh>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(Mesh).IsAssignableFrom(edgeType))
                {
                    inputList.Add((Mesh)edge.passThroughBuffer);
                }
                else if (typeof(List<Mesh>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Mesh> { Count: > 0 } list)
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