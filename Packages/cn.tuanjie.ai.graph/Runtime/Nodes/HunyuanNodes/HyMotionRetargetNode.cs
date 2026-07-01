using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    public enum HunyuanMotionType
    {
        Stride = 9,    // 跨步
        Fall = 10,     // 摔倒
        Jump = 11,     // 跳跃
        Kick = 12,     // 踢腿
        Swing = 13,    // 挥击
        Walk = 14,     // 步行
        Run = 15,      // 跑步
        Dance = 16     // 跳舞
    }
    
    [System.Serializable, NodeMenuItem("Hunyuan/Motion Retarget(Hunyuan)")]
    [UseProcessAsync]
    public class HyMotionRetargetNode : BaseHyAnimationNode
    {

        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;

        [HideInInspector] public string motionType = "Stride";

        [HideInInspector] public Dictionary<string, int> motionTypeMap = new()
        {
            { "Stride", 9 }, { "Fall", 10 }, { "Jump", 11 }, { "Kick", 12 }, { "Swing", 13 },
            { "Walk", 14 }, { "Run", 15 }, { "Dance", 16 }
        };
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("MotionRetarget(Hunyuan)");
        
        public override string description => DescriptionConstants.HyMotionRetargetNode;

        public override IEnumerator ProcessAsync()
        {
            // check input valid
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            if (!motionTypeMap.ContainsKey(motionType))
                motionType = "Run";
            var request = new HyMotionRetargetRequest()
            {
                n = 1, motionType = motionTypeMap[motionType]
            };
            if (!string.IsNullOrEmpty(inputModelUrl.fbx_url))
                request.fbxUrl = inputModelUrl.fbx_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_zip_url))
            {
                // call format conversion
                var formatConvertReq = new Hy3DFormatConversionRequest
                {
                    objZipUrl = inputModelUrl.obj_zip_url, responseFormat = "fbx"
                };
                var formatConvertRestCall = new Hy3DFormatConversionRestCall(serverConfig, serverIndex);
                yield return formatConvertRestCall.MakeServerRequest(formatConvertReq);
                var formatConvertRsp = formatConvertRestCall.Result;
                if (!formatConvertRestCall.Success)
                    throw new ArgumentException("Invalid obj model", nameof(inputModelUrl));
                var formatConvertTaskID = formatConvertRsp.taskId;
                var jobStatusRestCall = new GetJobStatusRestCall<TaskStatusRequest, HyModelOutput>(
                    serverConfig, serverIndex, formatConvertTaskID);
                var jobInfoRequest = new TaskStatusRequest();

                var processor = new CoroutineProcessor<Hy3DFormatConversionOutput>();
                yield return processor.ProcessAsync(BackendUtils.RetrieveFromBackendCommon(
                    jobStatusRestCall, jobInfoRequest));
                var output = processor.Result;
                if (string.IsNullOrEmpty(output.fbx_url))
                    throw new ArgumentException("Invalid obj model", nameof(inputModelUrl));
                request.fbxUrl = output.fbx_url;
            }
            else
                throw new ArgumentException("Only fbx and obj model is supported", nameof(inputModelUrl));
            var restCall = new HyMotionRetargetRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();

        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>(1) { "Motion: " + motionType };

            return true;
        }
    }
}

