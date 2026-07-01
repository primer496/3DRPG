using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UnityEngine.AIGraph.Backend
{
    [Serializable]
    public struct MeshyTaskID
    {
        public string id;
        public override string ToString()
        {
            return $"MeshyTaskID(id={id})";
        }
    }
    internal struct MeshyTextTo3DRequest
    {
        public string mode;
        public string prompt;
        public string artStyle;
        public int seed;
        public string aiModel;
        public string topology;
        public int targetPolycount;
        public bool shouldRemesh;
        public string symmetryMode;
        public bool isATPose;
        public bool moderation;

        public override string ToString()
        {
            return $"MeshyTextTo3DRequest(mode={mode}, prompt={prompt}, artStyle={artStyle}, " +
                   $"seed={seed}, aiModel={aiModel}, topology={topology}, targetPolycount={targetPolycount}, " +
                   $"shouldRemesh={shouldRemesh}, symmetryMode={symmetryMode}, isATPose={isATPose}, " +
                   $"moderation={moderation})";
        }
    }
    internal struct MeshyTextTo3DRefineRequest
    {
        public string mode;
        public string previewTaskId;
        public bool enablePbr;
        public string texturePrompt;
        public string textureImage;
        public string aiModel;
        public bool moderation;

        public override string ToString()
        {
            return $"MeshyTextTo3DRefineRequest(mode={mode}, previewTaskId={previewTaskId}, " +
                   $"enablePbr={enablePbr}, texturePrompt={texturePrompt}, textureImage={textureImage?.Length}, " +
                   $"aiModel={aiModel}, moderation={moderation})";
        }
    }
    internal struct MeshyImageTo3DRequest
    {
        public string image;
        public string aiModel;
        public string topology;
        public int targetPolycount;
        public string symmetryMode;
        public bool shouldRemesh;
        public bool shouldTexture;
        public bool enablePbr;
        public bool isATPose;
        public string texturePrompt;
        public string textureImage;
        public bool moderation;

        public override string ToString()
        {
            return $"MeshyImageTo3DRequest(image={image?.Length}, aiModel={aiModel}, topology={topology}, " +
                   $"targetPolycount={targetPolycount}, symmetryMode={symmetryMode}, " +
                   $"shouldRemesh={shouldRemesh}, shouldTexture={shouldTexture}, enablePbr={enablePbr}, " +
                   $"isATPose={isATPose}, texturePrompt={texturePrompt}, textureImage={textureImage?.Length}, " +
                   $"moderation={moderation})";
        }
    }
    internal struct MeshyRemeshRequest
    {
        public string inputTaskId;
        public string modelUrl;
        public List<string> targetFormats;
        public string topology;
        public int targetPolycount;
        public float resizeHeight;
        public string originAt;

        public override string ToString()
        {
            return $"MeshyRemeshRequest(inputTaskId={inputTaskId}, modelUrl={modelUrl}, " +
                   $"targetFormats=[{string.Join(",", targetFormats)}], topology={topology}, " +
                   $"targetPolycount={targetPolycount}, resizeHeight={resizeHeight}, originAt={originAt})";
        }
    }
    internal struct MeshyRiggingRequest
    {
        public string inputTaskId;
        public string modelUrl;
        public float heightMeters;
        public string textureImage;

        public override string ToString()
        {
            return $"MeshyRiggingRequest(inputTaskId={inputTaskId}, modelUrl={modelUrl}, " +
                   $"heightMeters={heightMeters}, textureImage={textureImage?.Length})";
        }
    }
    internal struct MeshyAnimationRequest
    {
        public string rigTaskId;
        public int actionId;
        public MeshyPostProcess postProcess;

        public override string ToString()
        {
            return $"MeshyAnimationRequest(rigTaskId={rigTaskId}, actionId={actionId}, postProcess={postProcess})";
        }
    }
    internal struct MeshyRetextureRequest
    {
        public string inputTaskId;
        public string modelUrl;
        public string textStylePrompt;
        public string imageStyle;
        public string aiModel;
        public bool enableOriginalUv;
        public bool enablePbr;

        public override string ToString()
        {
            return $"MeshyRetextureRequest(inputTaskId={inputTaskId}, modelUrl={modelUrl}, " +
                   $"textStylePrompt={textStylePrompt}, imageStyleUrl={imageStyle?.Length}, " +
                   $"aiModel={aiModel}, enableOriginalUv={enableOriginalUv}, enablePbr={enablePbr})";
        }
    }

    internal struct MeshyPostProcess
    {
        public string operationType;
        public int fps;

        public override string ToString()
        {
            return $"MeshyPostProcess(operationType={operationType}, fps={fps})";
        }
    }

    [Serializable]
    public struct MeshyModelUrl
    {
        [CanBeNull] public string glb;
        [CanBeNull] public string fbx;
        [CanBeNull] public string obj;
        [CanBeNull] public string mtl;
        [CanBeNull] public string usdz;
        public override string ToString()
        {
            return $"MeshModelUrl(glb={glb}, fbx={fbx}, obj={obj}, mtl={mtl}, usdz={usdz})";
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(glb) || string.IsNullOrEmpty(fbx) || string.IsNullOrEmpty(obj) ||
                   string.IsNullOrEmpty(mtl);
        }

        public string GetModelUrl()
        {
            if (!string.IsNullOrEmpty(glb))
                return glb;
            if (!string.IsNullOrEmpty(fbx))
                return fbx;
            if (!string.IsNullOrEmpty(obj))
                return obj;
            if (!string.IsNullOrEmpty(usdz))
                return usdz;
            return string.Empty;
        }
    }

    public struct MeshyTextureUrl
    {
        [CanBeNull] public string base_color;
        [CanBeNull] public string metallic;
        [CanBeNull] public string roughness;
        [CanBeNull] public string normal;

        public override string ToString()
        {
            return $"MeshyTextureUrl(base_color={base_color}, metallic={metallic}, " +
                   $"roughness={roughness}, normal={normal})";
        }
    }

    [Serializable]
    public struct MeshyModelOutput
    {
        public MeshyModelUrl model_urls;
        [CanBeNull] public string thumbnail_url;
        public List<MeshyTextureUrl> texture_urls;
        public override string ToString()
        {
            return $"MeshyModelOutput(model_urls={model_urls}, thumbnail_url={thumbnail_url}, " +
                   $"texture_urls=[{string.Join(",", texture_urls)}])";
        }
    }
    internal class MeshyTextTo3DRestCall : TJAIRestCall<MeshyTextTo3DRequest, TaskSubmitResponse>
    {
        public MeshyTextTo3DRestCall(ServerConfig asset, int mode) : base(asset, mode) {}
        public override string endPoint => "/api/editor/task/meshy-text-to-3d";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class MeshyTextTo3DRefineRestCall : TJAIRestCall<MeshyTextTo3DRefineRequest, TaskSubmitResponse>
    {
        public MeshyTextTo3DRefineRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class MeshyImageTo3DRestCall : TJAIRestCall<MeshyImageTo3DRequest, TaskSubmitResponse>
    {
        public MeshyImageTo3DRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class MeshyRemeshRestCall : TJAIRestCall<MeshyRemeshRequest, TaskSubmitResponse>
    {
        public MeshyRemeshRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class MeshyRiggingRestCall : TJAIRestCall<MeshyRiggingRequest, TaskSubmitResponse>
    {
        public MeshyRiggingRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class MeshyAnimationRestCall : TJAIRestCall<MeshyAnimationRequest, TaskSubmitResponse>
    {
        public MeshyAnimationRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class MeshyRetextureRestCall : TJAIRestCall<MeshyRetextureRequest, TaskSubmitResponse>
    {
        public MeshyRetextureRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
}