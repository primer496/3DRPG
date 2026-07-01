using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Unpack Float List")]
    public class UnpackFloatListNode : UnpackListNode<float>
    {
        [CustomPortInput(nameof(inputList), new Type[]
        {
            typeof(List<float>), typeof(float),
            typeof(int), typeof(List<int>)
        })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<float>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(float).IsAssignableFrom(edgeType))
                {
                    inputList.Add((float)edge.passThroughBuffer);
                }
                else if (typeof(List<float>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<float> { Count: > 0 } list)
                        inputList.AddRange(list);
                }
                // 支持从int自动转换为float
                else if (typeof(int).IsAssignableFrom(edgeType))
                {
                    inputList.Add((int)edge.passThroughBuffer);
                }
                else if (typeof(List<int>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<int> { Count: > 0 } intList)
                    {
                        foreach (var value in intList)
                        {
                            inputList.Add(value);
                        }
                    }
                }
            }
        }
        [CustomPortOutput(nameof(outputList), typeof(float))]
        protected void PushOutputList(List<SerializableEdge> edges, NodePort port)
        {
            // do nothing, but must have custom port output
        }
    }
}