using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    public abstract class UnpackListNode<T> : BaseTJAINode
    {
        [Input(name = "List", allowMultiple = true)]
        public List<T> inputList;

        [Output(name = "List")] public List<T> outputList;

        [CustomPortBehavior(nameof(outputList))]
        protected IEnumerable<PortData> OutputPortBehavior(List<SerializableEdge> edges)
        {
            if (outputList == null || outputList.Count == 0) yield break;

            for (var i = 0; i < outputList.Count; i++)
            {
                var identifier = $"{typeof(T)}_list_{i}";
                var portEdges = edges.FindAll(e => e.outputPortIdentifier == identifier);
                foreach (var edge in portEdges)
                {
                    if (edge.passThroughBuffer == null)
                        continue;
                    edge.passThroughBuffer = outputList[i];
                }
                yield return new PortData
                {
                    displayName = outputList[i].ToString(),
                    displayType = typeof(T),
                    identifier = identifier,
                    acceptMultipleEdges = true,
                    sizeInPixel = 8
                };
            }
        }

        public override bool needTrigger => true;

        public override void Process()
        {
            outputList = inputList;
            UpdatePortsForField(nameof(outputList));
        }
    }


    [Serializable, NodeMenuItem("Tools/Unpack String List")]
    public class UnpackStringListNode : UnpackListNode<string>
    {
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
        
        [CustomPortOutput(nameof(outputList), typeof(string))]
        protected void PushOutputList(List<SerializableEdge> edges, NodePort port)
        {
            // do nothing, but must have custom port output
        }
    }
}