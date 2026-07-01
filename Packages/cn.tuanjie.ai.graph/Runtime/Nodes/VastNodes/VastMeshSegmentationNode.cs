using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Mesh Segmentation(Tripo)")]
    [UseProcessAsync]
    public class VastMeshSegmentationNode : BaseVastModelNode
    {
        [Input(name = "Model ID")] public VastTaskID inputModelID;
        [Output(name = "Part Names")] public List<string> partNames;
        
        [HideInInspector]
        public string modelVersion = "v1.0-20250506";
        public override string name => LocalizationManager.Instance.GetLocalizedText("MeshSegmentation(Tripo)");
        
        public override string description => DescriptionConstants.VastMeshSegmentationNode;

        public Action onResultUpdated;
        [SerializeField, HideInInspector]
        public List<Group> createdGroups = new();
        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(inputModelID.id))
                throw new System.ArgumentNullException(nameof(inputModelID));
            var request = new VastMeshSegmentationRequest()
            {
                modelVersion = modelVersion, originalModelTaskId = inputModelID.id
            };
            var restCall = new VastMeshSegmentationRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
            
            // add output node
            if (obj != null)
            {
                onResultUpdated?.Invoke();
                var transforms = obj.transform.GetComponentsInChildren<Transform>().ToList();
                transforms.RemoveAll(t => t == obj.transform);
                partNames = transforms.Select(t => t.name).ToList();
            }
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string> { $"Model Version: {modelVersion}" };
            return true;
        }
    }
}

