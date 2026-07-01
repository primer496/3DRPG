using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack GameObject List")]
    public class PackGameObjectListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<GameObject> inputList;

        [Output(name = "List")] public List<GameObject> outputList;

        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<GameObject>), typeof(GameObject) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<GameObject>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(GameObject).IsAssignableFrom(edgeType))
                {
                    inputList.Add((GameObject)edge.passThroughBuffer);
                }
                else if (typeof(List<GameObject>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<GameObject> { Count: > 0 } list)
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