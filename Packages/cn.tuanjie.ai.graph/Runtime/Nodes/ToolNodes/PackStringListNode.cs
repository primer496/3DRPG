using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack String List")]
    public class PackStringListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<string> inputList;

        [Output(name = "List")] public List<string> outputList;

        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<string>), typeof(string) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<string>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(string).IsAssignableFrom(edgeType))
                {
                    inputList.Add((string)edge.passThroughBuffer);
                }
                else if (typeof(List<string>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<string> { Count: > 0 } list)
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