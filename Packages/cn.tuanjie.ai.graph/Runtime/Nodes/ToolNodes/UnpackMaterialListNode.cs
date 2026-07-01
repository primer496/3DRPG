using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Unpack Material List")]
    public class UnpackMaterialListNode : UnpackListNode<Material>
    {
        [CustomPortInput(nameof(inputList), new Type[]
        {
            typeof(List<Material>), typeof(Material),
            typeof(GameObject), typeof(List<GameObject>)
        })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<Material>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(Material).IsAssignableFrom(edgeType))
                {
                    inputList.Add((Material)edge.passThroughBuffer);
                }
                else if (typeof(List<Material>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Material> { Count: > 0 } list)
                        inputList.AddRange(list);
                }
                // 支持从Renderer获取材质
                else if (typeof(GameObject).IsAssignableFrom(edgeType))
                {
                    var gameObject = (GameObject)edge.passThroughBuffer;
                    if (gameObject.TryGetComponent<Renderer>(out var renderer))
                    {
                        inputList.AddRange(renderer.sharedMaterials);
                    }
                }
                else if (typeof(List<GameObject>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<GameObject> { Count: > 0 } gameObjects)
                    {
                        foreach (var go in gameObjects)
                        {
                            if (go.TryGetComponent<Renderer>(out var renderer))
                            {
                                inputList.AddRange(renderer.sharedMaterials);
                            }
                        }
                    }
                }
            }
        }
        [CustomPortOutput(nameof(outputList), typeof(Material))]
        protected void PushOutputList(List<SerializableEdge> edges, NodePort port)
        {
            // do nothing, but must have custom port output
        }
    }
}