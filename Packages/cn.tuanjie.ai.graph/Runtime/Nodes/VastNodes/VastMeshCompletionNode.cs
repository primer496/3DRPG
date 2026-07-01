using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Mesh Completion(Tripo)")]
    [UseProcessAsync]
    public class VastMeshCompletionNode : BaseVastModelNode
    {
        [Input(name = "Model ID")] public VastTaskID inputModelID;
        [Input(name = "Part Names", allowMultiple = true)] public List<string> inputPartNames;

        [HideInInspector] public string modelVersion = "v1.0-20250506";
        public override string name => LocalizationManager.Instance.GetLocalizedText("MeshCompletion(Tripo)");

        public override string description => DescriptionConstants.VastMeshCompletionNode;
        
        [CustomPortInput(nameof(inputPartNames), new Type[] { typeof(List<string>), typeof(string) })]
        private void PullInputPartNames(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            inputPartNames ??= new List<string>();
            inputPartNames.Clear();
            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();
                if (typeof(string).IsAssignableFrom(edgeType))
                {
                    inputPartNames.Add(edge.passThroughBuffer as string);
                } else if (typeof(List<string>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<string> { Count: > 0 } list)
                        inputPartNames.AddRange(list);
                }
            }
            inputPartNames.RemoveAll(string.IsNullOrEmpty);
        }

        public override IEnumerator ProcessAsync()
        {
            var request = new VastMeshCompletionRequest()
            {
                modelVersion = modelVersion, originalModelTaskId = inputModelID.id,
                partNames = inputPartNames
            };
            var restCall = new VastMeshCompletionRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();

        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Model Version: {modelVersion}",
                $"Part Names: {DebugUtils.ToString(inputPartNames)}"
            };
            return true;
        }
    }
}