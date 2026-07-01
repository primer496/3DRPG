using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.AIGraph.Cache;

namespace UnityEngine.AIGraph
{
    internal struct HyGameImageEditPromptRewriteRequest
    {
        public string query;
        public string[] images;
        public string[] imageUrls;

        public override string ToString()
        {
            return $"HyImageEditRequest(query={DebugUtils.ToString(query)}, " +
$"image={DebugUtils.ToString(images)}, imageUrl={DebugUtils.ToString(imageUrls)}";
        }
    }

    internal class HyGameImageEditPromptRewriteRestCall : TJAIRestCall<HyGameImageEditPromptRewriteRequest, TaskSubmitResponse>
    {
        public HyGameImageEditPromptRewriteRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-game-edit-vlm-rewriter";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    public struct HyGameImageEditPromptRewriteOutput
    {
        [CanBeNull] public string text;

        public override string ToString()
        {
            return $"HyGameImageEditPromptRewriteOutput(text={text})";
        }
    }

    internal struct HyImageGameEditRequest
    {
        public string prompt;
        public string[] images;
        public string[] imageUrls;
        public string size;

        public override string ToString()
        {
            return $"HyImageEditRequest(prompt={DebugUtils.ToString(prompt)}, " +
$"image={DebugUtils.ToString(images)}, imageUrl={DebugUtils.ToString(imageUrls)}, " +
$"size={size})";
        }
    }

    internal class HyImageGameEditRestCall : TJAIRestCall<HyImageGameEditRequest, TaskSubmitResponse>
    {
        public HyImageGameEditRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-game-edit";
        public override string baseUrl => "";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    public struct HyImageGameEditOutput
    {
        [CanBeNull] public string url;

        public override string ToString()
        {
            return $"HyImageGameEditOutput(url={url})";
        }
    }
    [System.Serializable, NodeMenuItem("Hunyuan/Edit Image Specially For Game(Hunyuan)")]
    [UseProcessAsync]
    internal class HyImageGameEditNode : BaseHyImageNode
    {
        [Input(name = "Prompt")]
        [Tooltip("文本prompt，token数限制为256以下。")]
        public string prompt;
        [Input(name = "Image")]
        [Tooltip("图base64编码，大小不超过10M,支持jpg/jpeg格式，和image_url二选一，两者都存在时优先使用base64")]
        public Texture2D image;
        [Input(name = "Image Url")]
        [Tooltip("上传图片url，大小不超过10M，支持jpg/jpeg/png格式，尺寸在2048x2048以内")]
        public string imageUrl;
        [Output(name = "Rewrite Prompt")] public string queryResult;

        [HideInInspector]
        public string size = "1024x1024";
        [HideInInspector]
        public List<string> sizes = new List<string>
        {
            "1024x1024",
            "512x2048",
            "512x1984",
            "512x1920",
            "512x1856",
            "512x1792",
            "512x1728",
            "512x1664",
            "512x1600",
            "512x1536",
            "576x1472",
            "640x1408",
            "704x1344",
            "768x1280",
            "832x1216",
            "896x1152",
            "960x1088",
            "1088x960",
            "1152x896",
            "1216x832",
            "1280x768",
            "1344x704",
            "1408x640",
            "1472x576",
            "1536x512",
            "1600x512",
            "1664x512",
            "1728x512",
            "1792x512",
            "1856x512",
            "1920x512",
            "1984x512",
            "2048x512",
        };
        public override string name => LocalizationManager.Instance.GetLocalizedText("ImageGameEditing(Hunyuan)");
        public override string description => "输入文字修改图片，专为游戏定制";

        public override IEnumerator ProcessAsync()
        {
            if (ParaUtils.IsNull(prompt))
                throw new ArgumentException("prompt can not be null.");
            if (ParaUtils.IsNull(image) && ParaUtils.IsNull(imageUrl))
                throw new ArgumentException("image/imageUrl can not be null at the same time.");

            var requestPrompt = new HyGameImageEditPromptRewriteRequest
            {
                query = prompt,
                imageUrls = String.IsNullOrEmpty(imageUrl) ? null : new string[] { imageUrl },
                images = image != null ? new string[] { image.ToBase64() } : null
            };

            var restCallPrompt = new HyGameImageEditPromptRewriteRestCall(serverConfig, serverIndex);
            yield return restCallPrompt.MakeServerRequest(requestPrompt);
            var formatConvertRsp = restCallPrompt.Result;
            if (!restCallPrompt.Success)
                throw new ArgumentException("Fail to query improved pormpt");

            var jobStatusRestCall = new GetJobStatusRestCall<TaskStatusRequest, HyGameImageEditPromptRewriteOutput>(
     serverConfig, serverIndex, formatConvertRsp.taskId);
            var jobInfoRequest = new TaskStatusRequest();

            var processor = new CoroutineProcessor<HyGameImageEditPromptRewriteOutput>();
            yield return processor.ProcessAsync(BackendUtils.RetrieveFromBackendCommon(
                jobStatusRestCall, jobInfoRequest));
            var output = processor.Result;
            queryResult = GetInstructionAndRemoveQuotes(output.text);
            var request = new HyImageGameEditRequest
            {
                prompt = output.text,
                imageUrls = new string[] { imageUrl },
                size = size
            };
            if (image != null)
                request.images = new string[] { image.ToBase64() };
            Debug.Log(request);
            Debug.Log(output.text);
            var restCall = new HyImageGameEditRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();

        public override bool needTrigger => true;
        public override bool isRenamable => true;

        protected override void Enable()
        {
            base.Enable();
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 2;
        }

        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
$"prompt: {prompt}", $"imageUrl: {imageUrl}",
$"size: {size}"
            };
            return true;
        }


        [Serializable]
        private class PromptDto
        {
            public string think_instruction;
            public string instruction;
        }
        private static string GetInstructionAndRemoveQuotes(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";

            var json = raw.Trim();

            if (json.Length >= 2 && json[0] == '"' && json[json.Length - 1] == '"')
            {
                json = json.Substring(1, json.Length - 2);
                json = Regex.Unescape(json);
            }

            var dto = JsonUtility.FromJson<PromptDto>(json);
            var instruction = dto?.instruction ?? "";

            return instruction.Replace("\"", "").Replace("“", "").Replace("”", "");
        }

    }
}
