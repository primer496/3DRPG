using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack Float List")]
    public class PackFloatListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<float> inputList;

        [Output(name = "List")] public List<float> outputList;

        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<float>), typeof(float) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            ProcessInputList(edges);
        }

        private void ProcessInputList(List<SerializableEdge> edges)
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
            }
        }

        public override bool isRenamable => true;

        public override void Process()
        {
            outputList = inputList ?? new List<float>();
        }
    }
}