using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Unpack GameObject List")]
    public class UnpackGameObjectListNode : UnpackListNode<GameObject>
    {
        [CustomPortInput(nameof(inputList), new Type[]
        {
            typeof(List<GameObject>), typeof(GameObject),
            typeof(Component), typeof(List<Component>)
        })]
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
                // 支持从Component获取GameObject
                else if (typeof(Component).IsAssignableFrom(edgeType))
                {
                    var component = (Component)edge.passThroughBuffer;
                    if (component != null)
                    {
                        inputList.Add(component.gameObject);
                    }
                }
                else if (typeof(List<Component>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Component> { Count: > 0 } components)
                    {
                        foreach (var component in components)
                        {
                            if (component != null)
                            {
                                inputList.Add(component.gameObject);
                            }
                        }
                    }
                }
            }
        }
        [CustomPortOutput(nameof(outputList), typeof(GameObject))]
        protected void PushOutputList(List<SerializableEdge> edges, NodePort port)
        {
            // do nothing, but must have custom port output
        }
    }
}