using System;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [Serializable, NodeMenuItem("Tools/Unpack Mesh List")]
    public class UnpackMeshListNode : UnpackListNode<Mesh>
    {
        [CustomPortInput(nameof(inputList), new Type[]
        {
            typeof(List<Mesh>), typeof(Mesh),
            typeof(GameObject), typeof(List<GameObject>)
        })]
        private void PullInputList(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputList ??= new List<Mesh>();
            inputList.Clear();

            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();

                if (typeof(Mesh).IsAssignableFrom(edgeType))
                {
                    inputList.Add((Mesh)edge.passThroughBuffer);
                }
                else if (typeof(List<Mesh>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Mesh> { Count: > 0 } list)
                        inputList.AddRange(list);
                }
                // 支持从GameObject获取MeshFilter的mesh
                else if (typeof(GameObject).IsAssignableFrom(edgeType))
                {
                    var gameObject = (GameObject)edge.passThroughBuffer;
                    if (gameObject.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
                    {
                        inputList.Add(meshFilter.sharedMesh);
                    }
                }
                else if (typeof(List<GameObject>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<GameObject> { Count: > 0 } gameObjects)
                    {
                        foreach (var go in gameObjects)
                        {
                            if (go.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
                            {
                                inputList.Add(meshFilter.sharedMesh);
                            }
                        }
                    }
                }
            }
        }
        [CustomPortOutput(nameof(outputList), typeof(Mesh))]
        protected void PushOutputList(List<SerializableEdge> edges, NodePort port)
        {
            // do nothing, but must have custom port output
        }
    }
}