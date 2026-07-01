using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Pack Texture2D List")]
    public class PackTexture2DListNode : BaseTJAINode
    {
        [Input(name = "Elements", allowMultiple = true)]
        public List<Texture2D> inputList;

        [Output(name = "List")] public List<Texture2D> outputList;

        [CustomPortInput(nameof(inputList), new Type[] { typeof(List<Texture2D>), typeof(Texture2D) })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<Texture2D>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(Texture2D).IsAssignableFrom(edgeType))
                {
                    inputList.Add((Texture2D)edge.passThroughBuffer);
                }
                else if (typeof(List<Texture2D>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Texture2D> { Count: > 0 } list)
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