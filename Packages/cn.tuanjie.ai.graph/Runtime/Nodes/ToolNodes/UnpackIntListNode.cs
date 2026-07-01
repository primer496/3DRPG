using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Unpack Int List")]
    public class UnpackIntListNode : UnpackListNode<int>
    {
        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<int>), typeof(int) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<int>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(int).IsAssignableFrom(edgeType))
                {
                    inputList.Add((int)edge.passThroughBuffer);
                }
                else if (typeof(List<int>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<int> { Count: > 0 } list)
                        inputList.AddRange(list);
                }
            }
        }
        [CustomPortOutput(nameof(outputList), typeof(int))]
        protected void PushOutputList(List<SerializableEdge> edges, NodePort port)
        {
            // do nothing, but must have custom port output
        }
    }
}