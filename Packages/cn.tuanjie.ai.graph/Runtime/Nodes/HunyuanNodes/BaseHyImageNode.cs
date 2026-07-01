using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [UseProcessAsync]
    public class BaseHyImageNode : TJAIBaseAssetNode
    {
        [Save(ReceivedDataType = typeof(HyImageData))]
        [Preview, SerializeField, HideInInspector]
        [Output(name = "Image")]
        protected Texture2D m_OutputTexture;

        public Texture2D outputTexture
        {
            get => m_OutputTexture;
            set
            {
                if (m_OutputTexture != value)
                {
                    m_OutputTexture = value;
                    this?.NotifyFieldChanged("m_OutputTexture");
                }
            }
        }
        [Output(name = "Image Url")] public string outputImageUrl;

        public override bool needTrigger => true;
        public override bool isRenamable => true;
        protected const int serverIndex = 3;
        protected BaseArtifact<Texture2D, HyImageData> artifact => (BaseArtifact<Texture2D, HyImageData>)currentArtifact;

        protected override void Enable()
        {
            hasSettings = false;
            base.Enable();
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 2;
        }

        public override void UpdateOutputPorts()
        {
            var cachedTex = currentArtifact.GetCacheUnityObject() as Texture2D;
            if (cachedTex)
                outputTexture = cachedTex;
            outputImageUrl = artifact.m_ReceivedData.url;
        }

        private void ReleaseTexture()
        {
            if (m_OutputTexture == null) return;
#if UNITY_EDITOR
            Object.DestroyImmediate(m_OutputTexture, true);
#else
            Texture2D.Destroy(m_OutputTexture);
#endif
            m_OutputTexture = null;
        }

        private void CopyTexture(Texture2D value)
        {
            // FIX: Convert input value to RenderTexture and then convert back to Texture,
            // by this way can we import Textures that Read/Write property is False.
            RenderTexture rt = RenderTexture.GetTemporary(
                value.width, value.height,
                0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            Graphics.Blit(value, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(value.width, value.height);
            tex.ReadPixels(new Rect(0, 0, value.width, value.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            m_OutputTexture = tex;
        }

        public override IEnumerator RestoreHistory(string Guid)
        {
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/tjai_{Guid}.png";
            return base.RestoreHistory(Guid);
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
            
            var data = new HyImageData()
            {
                assetPath = $"{GetResourceFolder()}/tjai_{taskID}.png",
                ID = taskID, progressCallback = UpdateStatus
            };

            var processor = new CoroutineProcessor<Texture2D>();
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));

            if (status == NodeStatus.Init)
                yield break;
            UpdateOutputPorts();

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
        }
    }
}