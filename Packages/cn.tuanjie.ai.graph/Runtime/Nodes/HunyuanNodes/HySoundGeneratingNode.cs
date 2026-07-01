#if UNITY_EDITOR
using Codice.CM.Common;
#endif
using GraphProcessor;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AIGraph.Backend;
using UnityEngine.AIGraph.Cache;

namespace UnityEngine.AIGraph
{
    internal struct HySoundGenerateRequest
    {
        public string prompt;
        public string negativePrompt;
        public int nSamples;
        public float duration;
        public int inferSteps;
        public bool revise;
        public float cfgScale;

        public override string ToString()
        {
            return $"HyImageEditRequest(prompt={prompt ?? "(null)"}, " +
                   $"negativePrompt={negativePrompt}, inferSteps={inferSteps}" +
                   $"nSamples={nSamples}, duration={duration}, revise={revise}, cfgScale={cfgScale})";
        }
    }
    internal class HySoundGenerateRestCall : TJAIRestCall<HySoundGenerateRequest, TaskSubmitResponse>
    {
        public HySoundGenerateRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-video-aries-gametta";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    public struct HySoundGenerateOutput
    {
        [CanBeNull] public string audio_url;

        public override string ToString()
        {
            return $"HySoundGenerateOutput(url={audio_url})";
        }
    }

    [Serializable]
    public class HySoundGenerateData : IReceivedData
    {
        public HySoundGenerateOutput output;

        public HySoundGenerateData() { }

        public override Object transferToUnityObject()
        {
            return ImportUtils.Import<AudioClip>(assetPath);
        }

        public override IEnumerator RetrieveFromBackend(int serverIndex)
        {
            var jobStatusRestCall = new GetJobStatusRestCall<TaskStatusRequest, HySoundGenerateOutput>(
                serverConfig, serverIndex, ID);
            var jobInfoRequest = new TaskStatusRequest();

            var processor = new CoroutineProcessor<HySoundGenerateOutput>();
            yield return processor.ProcessAsync(BackendUtils.RetrieveFromBackendCommon(
                jobStatusRestCall, jobInfoRequest, progressCallback));
            output = processor.Result;

            tokenRemaining = jobStatusRestCall.Result.creditBalance;
            var url = string.Empty;

            if (!string.IsNullOrEmpty(output.audio_url) && output.audio_url.StartsWith("http"))
                url = output.audio_url;
            if (string.IsNullOrEmpty(url))
                throw new NullReferenceException($"No valid download url in response, task: {ID}");
            var ext = PathUtils.GetUrlExtension(url);
            if (!assetPath.EndsWith(ext))
                assetPath += ext;
            var downloadCoroutine = BackendUtils.DownloadFromUrl(url, serverIndex);
            yield return downloadCoroutine;
            var bytes = downloadCoroutine.Current as byte[];
            if (!string.IsNullOrEmpty(assetPath))
                BackendUtils.SaveBytesToFile(bytes, assetPath);
            yield return null;
        }
    }

    [System.Serializable, NodeMenuItem("Hunyuan/Sound Generating(Hunyuan)")]
    [UseProcessAsync]
    public class HySoundGeneratingNode : TJAIBaseAssetNode
    {
        [Input(name = "Prompt")]
        [Tooltip("文本prompt，token数限制为256以下，必须提供。")]
        public string prompt;
        [Input(name = "Negative Prompt")]
        [Tooltip("文本prompt，token数限制为256以下。")]
        public string negativePrompt;

        [Input(name = "Duration"), ShowAsDrawer]
        [Tooltip("生成音频时长，0.5～30s，默认5s")]
        private float duration = 5.0f;

        [Input(name = "Revise"), ShowAsDrawer]
        [Tooltip("是否对prompt进行改写")]
        public bool revise = true;


        [Input(name = "Sample Count"), ShowAsDrawer]
        [Tooltip("取值1-5，默认为5，值越大效果越好，值越小速度越快")]
        public int nSamples = 5;

        [Input(name = "Infer Steps"), ShowAsDrawer]
        [Tooltip("取值25-150，默认50")]
        public int inferSteps = 50;

        [Input(name = "CFG Scale"), ShowAsDrawer]
        [Tooltip("文本增强程度，0~10，默认4.5")]
        private float cfgScale = 4.5f;

        [Output(name = "Sound Url")] public string url;

        public override string name => LocalizationManager.Instance.GetLocalizedText("SoundGenerating(Hunyuan)");
        public override string description => "输入文字，生成音频";

        [Save(ReceivedDataType = typeof(HySoundGenerateData))]
        [Preview, SerializeField, HideInInspector]
        [Output(name = "AudioClip")]
        protected AudioClip m_AudioClip;

        public AudioClip audioClip
        {
            get => m_AudioClip;
            set
            {
                if (m_AudioClip != value)
                {
                    m_AudioClip = value;
                    this?.NotifyFieldChanged("m_AudioClip");
                }
            }
        }

        protected BaseArtifact<AudioClip, HySoundGenerateData> artifact => (BaseArtifact<AudioClip, HySoundGenerateData>)currentArtifact;

        public override bool needTrigger => true;
        public override bool isRenamable => true;
        protected const int serverIndex = 3;

        protected override void Enable()
        {
            base.Enable();
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 2;
        }

        public override void UpdateOutputPorts()
        {
            var cachedObj = currentArtifact.GetCacheUnityObject() as AudioClip;
            if (cachedObj)
                audioClip = cachedObj;
            url = artifact.m_ReceivedData.output.audio_url;
        }

        public override IEnumerator RestoreHistory(string Guid)
        {
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/tjai_{GetType().Name}_{Guid}";

            return base.RestoreHistory(Guid);
        }

        public override IEnumerator ProcessAsync()
        {
            if (ParaUtils.IsNull(prompt))
                throw new ArgumentException("prompt can not be null.");

            var request = new HySoundGenerateRequest
            {
                prompt = prompt,
                negativePrompt = negativePrompt,
                nSamples = Math.Clamp(nSamples, 1, 5),
                duration = Mathf.Clamp(duration, 0.5f, 30f),
                revise = revise,
                inferSteps = Math.Clamp(inferSteps, 25, 150),
                cfgScale = Mathf.Clamp(cfgScale, 0.0f, 10.0f)
            };

            var restCall = new HySoundGenerateRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();

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
            var data = new HySoundGenerateData
            {
                assetPath = $"{GetResourceFolder()}/tjai_{GetType().Name}_{taskID}",
                ID = taskID,
                progressCallback = UpdateStatus
            };
            var processor = new CoroutineProcessor<AudioClip>();
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));

            if (status == NodeStatus.Init)
                yield break;
            UpdateOutputPorts();

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
        }

        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"prompt: {prompt}", $"negativePrompt: {negativePrompt}", $"revise: {revise}",
                $"nSamples: {nSamples}", $"duration: {duration}", $"inferSteps: {inferSteps}", $"cfgScale: {cfgScale}"
            };
            return true;
        }
    }
}
