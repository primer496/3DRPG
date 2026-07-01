using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack Int List")]
    public class PackIntListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<int> inputList;

        [Output(name = "List")] public List<int> outputList;

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

        public override bool isRenamable => true;

        public override void Process()
        {
            outputList = inputList;
        }
    }
}