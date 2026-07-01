using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [Serializable, UseProcessAsync, NodeMenuItem("Hyper3D-Rodin/Generate Skybox(Rodin3D)")]
    public class RodinGenerateSkyboxNode : TJAIBaseAssetNode
    {
        // input parameters
        [Input(name = "Prompt")] public string prompt;

        [Input(name = "Images", allowMultiple = true)]
        public List<string> images;

        [Input(name = "High Resolution"), ShowAsDrawer]
        public bool highRes = false;
        
        // preview
        [Preview, SerializeField, HideInInspector]
        [Save(ReceivedDataType = typeof(RodinMaterialData))]
        [Output("Material")]
        private Material m_Material;

        public Material material
        {
            get => m_Material;
            set
            {
                if (m_Material != value)
                {
                    m_Material = value;
                    this?.NotifyFieldChanged("m_Material");
                }
            }
        }

        [CustomPortInput(nameof(images), new Type[] { typeof(List<Texture2D>), typeof(Texture2D) })]
        public void PullInputImages(List<SerializableEdge> edges, NodePort outputPort = null)
        {
            if (edges == null || edges.Count == 0) return;
            images ??= new List<string>();
            images.Clear();
            foreach (var e in edges)
            {
                if (e.passThroughBuffer == null)
                    continue;
                if (typeof(Texture2D).IsAssignableFrom(e.passThroughBuffer.GetType()))
                {
                    if (e.passThroughBuffer is Texture2D tex)
                        images.Add(tex.ToBase64());
                }
                else if (typeof(List<Texture2D>).IsAssignableFrom(e.passThroughBuffer.GetType()))
                {
                    if (e.passThroughBuffer is List<Texture2D> inputImages)
                    {
                        inputImages.RemoveAll(i => i == null);
                        images.AddRange(inputImages.Select(i => i.ToBase64()));
                    }
                }
            }
        }

        public override bool needTrigger => true;
        public override bool isRenamable => true;
        public override string name => LocalizationManager.Instance.GetLocalizedText("Generate Skybox(Rodin3D)");
        
        public override string description => "Generate skybox with Rodin API";

        protected override void Enable()
        {
            base.Enable();
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 3;
        }

        public override IEnumerator RestoreHistory(string Guid)
        {
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/rodin_{Guid}";
            yield return base.RestoreHistory(Guid);
        }

        public override void UpdateOutputPorts()
        {
            var cachedObj = artifact.GetCacheUnityObject() as Material;
            material = cachedObj;
        }

        protected BaseArtifact<Material, RodinMaterialData> artifact =>
            (BaseArtifact<Material, RodinMaterialData>)currentArtifact;

        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(prompt) && (images == null || images.Count == 0))
            {
                if (string.IsNullOrEmpty(prompt))
                    throw new ArgumentNullException(nameof(prompt));
                if (images == null || images.Count == 0)
                    throw new ArgumentNullException(nameof(images));
            }

            var request = new RodinSkyboxRequest
            {
                prompt = prompt, imageBase64s = images, high_res = highRes
            };
            var restCall = new RodinSkyboxRestCall(serverConfig, serverConfig.serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        internal IEnumerator GenerateRestCall<TReq, TRestCall>(TReq req, TRestCall restCall)
            where TRestCall : TJAIRestCall<TReq, TaskSubmitResponse>
        {
            if (string.IsNullOrEmpty(taskID))
            {
                yield return restCall.MakeServerRequest(req);
                var response = restCall.Result;
                if (!restCall.Success)
                    throw new Exception(
                        $"Failed to generate artifact, task id: {response.taskId}, error message: {response.message}");
                taskID = response.taskId;
            }

            var data = new RodinMaterialData()
            {
                assetPath = $"{GetResourceFolder()}/rodin_skybox_{taskID}",
                ID = taskID, progressCallback = UpdateStatus
            };

            var processor = new CoroutineProcessor<GameObject>();
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverConfig.serverIndex));
            processor.HandleException();

            if (status == NodeStatus.Init)
                yield break;
            UpdateOutputPorts();

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
            yield return null;
        }
    }
}