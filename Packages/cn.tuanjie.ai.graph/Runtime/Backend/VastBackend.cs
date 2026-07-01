using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace UnityEngine.AIGraph.Backend
{
    internal struct VastTextToModelRequest
    {
        public bool autoSize;
        public string compress;
        public int faceLimit;
        public bool generateParts;
        public int imageSeed;
        public int modelSeed;
        public string modelVersion;
        public string negativePrompt;
        public bool pbr;
        public string prompt;
        public bool quad;
        public bool smartLowPoly;
        public string style;
        public bool texture;
        public string textureQuality;
        public int textureSeed;

        public override string ToString()
        {
            return $"VastTextToModelRequest(autoSize={autoSize}, compress={compress}, faceLimit={faceLimit}, " +
                   $"generateParts={generateParts}, imageSeed={imageSeed}, modelSeed={modelSeed}, " +
                   $"modelVersion={modelVersion}, negativePrompt={negativePrompt}, pbr={pbr}, " +
                   $"prompt={prompt}, quad={quad}, smartLowPoly={smartLowPoly}, style={style}, " +
                   $"texture={texture}, textureQuality={textureQuality}, textureSeed={textureSeed})";
        }
    }

    internal struct VastImageToModelRequest
    {
        public bool autoSize;
        public string compress;
        public int faceLimit;
        public bool generateParts;
        public string imageBase64;
        public string imageName;
        public int modelSeed;
        public string modelVersion;
        public string orientation;
        public bool pbr;
        public bool quad;
        public bool smartLowPoly;
        public string style;
        public bool texture;
        public string textureAlignment;
        public string textureQuality;
        public int textureSeed;
        
        public override string ToString()
        {
            return $"VastImageToModelRequest(autoSize={autoSize}, compress={compress}, faceLimit={faceLimit}, generateParts={generateParts}, " +
                   $"imageBase64={imageBase64?.Length}, imageName={imageName}, modelSeed={modelSeed}, " +
                   $"modelVersion={modelVersion}, orientation={orientation}, pbr={pbr}, quad={quad}, " +
                   $"smartLowPoly={smartLowPoly}, style={style}, texture={texture}, " +
                   $"textureAlignment={textureAlignment}, textureQuality={textureQuality}, textureSeed={textureSeed})";
        }
    }
    
    public struct TexturePrompt
    {
        public string image;
        public string text;
        public string styleImage;

        public override string ToString()
        {
            return $"TexturePrompt(image={image?.Length}, styleImage={styleImage?.Length}, text={text})";
        }
    }

    public struct VastTextureModelRequest
    {
        public bool bake;
        public string compress;
        public string modelVersion;
        public string originalModelTaskId;
        public List<string> partNames;
        public bool pbr;
        public bool texture;
        public string textureAlignment;
        public TexturePrompt texturePrompt;
        public string textureQuality;
        public int textureSeed;
        
        public override string ToString()
        {
            return $"VastTextureModelRequest(bake={bake}, compress={compress}, modelVersion={modelVersion}, " +
                   $"originalModelTaskId={originalModelTaskId}, partNames=[{string.Join(", ", partNames)}], " +
                   $"pbr={pbr}, texture={texture}, textureAlignment={textureAlignment}, " +
                   $"texturePrompt={texturePrompt}, textureQuality={textureQuality}, textureSeed={textureSeed})";
        }
    }

    [Serializable]
    public struct VastTaskID
    {
        public string id;
        public override string ToString()
        {
            return $"VastTaskID(id={id})";
        }
    }

    internal struct VastPreRigCheckRequest
    {
        public string originalModelTaskId;
        public override string ToString()
        {
            return $"VastPreRigCheckRequest(originalModelTaskId={originalModelTaskId})";
        }
    }

    internal struct VastRigRequest
    {
        public string modelVersion;
        public string originalModelTaskId;
        public string outFormat;
        public string rigType;
        public string spec;

        public override string ToString()
        {
            return $"VastRigRequest(modelVersion={modelVersion}, originalModelTaskId={originalModelTaskId}, " +
                   $"outFormat={outFormat}, rigType={rigType}, spec={spec})";
        }
    }

    internal struct VastRetargetRequest
    {
        public string animation;
        public List<string> animations;
        public bool bakeAnimation;
        public bool exportWithGeometry;
        public string originalModelTaskId;
        public string outFormat;

        public override string ToString()
        {
            return $"VastRetargetRequest(animation={animation}, animations=[{string.Join(",", animations)}], " +
                   $"bakeAnimation={bakeAnimation}, exportWithGeometry={exportWithGeometry}, " +
                   $"originalModelTaskId={originalModelTaskId}, outFormat={outFormat})";
        }
    }

    internal struct VastStylizeModelRequest
    {
        public int blockSize;
        public string originalModelTaskId;
        public string style;

        public override string ToString()
        {
            return $"VastStylizeModelRequest(blockSize={blockSize}, originalModelTaskId={originalModelTaskId}, " +
                   $"style={style})";
        }
    }

    internal struct VastMultiviewToModelRequest
    {
        internal struct VastMultiviewToModelFileData
        {
            public string contentType;
            public string imageBase64;
            public string imageName;
            public override string ToString()
            {
                return $"VastMultiviewToModelFileData(contentType={contentType}, " +
                       $"imageBase64={imageBase64?.Length}, imageName={imageName})";
            }
        }
        
        public bool autoSize;
        public string compress;
        public int faceLimit;
        public List<VastMultiviewToModelFileData> files;
        public bool generateParts;
        public string modelVersion;
        public string orientation;
        public bool pbr;
        public bool quad;
        public bool smartLowPoly;
        public bool texture;
        public string textureAlignment;
        public string textureQuality;
        public int textureSeed;

        public override string ToString()
        {
            return $"VastMultiviewToModelRequest(autoSize={autoSize}, compress={compress}, " +
                   $"faceLimit={faceLimit}, files=[{string.Join(", ", files)}], generateParts={generateParts}, " +
                   $"modelVersion={modelVersion}, orientation={orientation}, pbr={pbr}, " +
                   $"quad={quad}, smartLowPoly={smartLowPoly}, texture={texture}, " +
                   $"textureAlignment={textureAlignment}, textureQuality={textureQuality}, " +
                   $"textureSeed={textureSeed})";
        }
    }

    internal struct VastMeshSegmentationRequest
    {
        public string modelVersion;
        public string originalModelTaskId;

        public override string ToString()
        {
            return $"VastMeshSegmentationRequest(modelVersion={modelVersion}, " +
                   $"originalModelTaskId={originalModelTaskId})";
        }
    }
    
    internal struct VastMeshCompletionRequest
    {
        public string modelVersion;
        public string originalModelTaskId;
        public List<string> partNames;

        public override string ToString()
        {
            return $"VastMeshCompletionRequest(modelVersion={modelVersion}," +
                   $"originalModelTaskId={originalModelTaskId}, partNames=[{string.Join(",", partNames)}])";
        }
    }

    internal struct VastLowpolyRequest
    {
        public bool bake;
        public int faceLimit;
        public string modelVersion;
        public string originalModelTaskId;
        public List<string> partNames;
        public bool quad;

        public override string ToString()
        {
            return $"VastLowpolyRequest(bake={bake}, faceLimit={faceLimit}, modelVersion={modelVersion}," +
                   $"originalModelTaskId={originalModelTaskId}, partNames=[{string.Join(",", partNames)}]," +
                   $"quad={quad})";
        }
    }

    public struct VastTextToModelOutput
    {
        [CanBeNull] public string generated_image;
        [CanBeNull] public string model;
        [CanBeNull] public string pbr_model;
        [CanBeNull] public string base_model;
        [CanBeNull] public string rendered_image;

        public override string ToString()
        {
            return $"VastTextToModelOutput(generated_image={generated_image}, model={model}," +
                   $" pbr_model={pbr_model}, base_model={base_model}, rendered_image={rendered_image})";
        }
    }

    internal struct VastPreRigCheckOutput
    {
        public string rig_type;
        public bool riggable;
        public string topology;

        public override string ToString()
        {
            return $"VastPreRigCheckOutput(rig_type={rig_type}, riggable={riggable}, topology={topology})";
        }
    }

    internal class VastTextToModelRestCall : TJAIRestCall<VastTextToModelRequest, TaskSubmitResponse>
    {
        public VastTextToModelRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/tripo-text-to-model";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastImageToModelRestCall : TJAIRestCall<VastImageToModelRequest, TaskSubmitResponse>
    {
        public VastImageToModelRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/tripo-image-to-model";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastTextureModelRestCall : TJAIRestCall<VastTextureModelRequest, TaskSubmitResponse>
    {
        public VastTextureModelRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/tripo-texture-model";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class VastPreRigCheckRestCall : TJAIRestCall<VastPreRigCheckRequest, TaskSubmitResponse>
    {
        public VastPreRigCheckRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-prerigcheck";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class VastRigRestCall : TJAIRestCall<VastRigRequest, TaskSubmitResponse>
    {
        public VastRigRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-rig";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class VastRetargetRestCall : TJAIRestCall<VastRetargetRequest, TaskSubmitResponse>
    {
        public VastRetargetRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-retarget";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class VastStylizeModelRestCall : TJAIRestCall<VastStylizeModelRequest, TaskSubmitResponse>
    {
        public VastStylizeModelRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-stylize-model";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastMultiviewToModelRestCall : TJAIRestCall<VastMultiviewToModelRequest, TaskSubmitResponse>
    {
        public VastMultiviewToModelRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-multiview-to-model";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastMeshSegmentationRestCall : TJAIRestCall<VastMeshSegmentationRequest, TaskSubmitResponse>
    {
        public VastMeshSegmentationRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-mesh-segmentation";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastMeshCompletionRestCall : TJAIRestCall<VastMeshCompletionRequest, TaskSubmitResponse>
    {
        public VastMeshCompletionRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-mesh-completion";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastLowpolyRestCall : TJAIRestCall<VastLowpolyRequest, TaskSubmitResponse>
    {
        public VastLowpolyRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/tripo-highpoly-to-lowpoly";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class VastDownloadRestCall : TJAIRestCall<string, byte[]>
    {
        public VastDownloadRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;
        protected override void PrepareRequest(string request, ref UnityWebRequest uRequest)
        {
            uRequest.SetRequestHeader("accept", "application/json");
        }

        protected override byte[] ParseResponse(UnityWebRequest response)
            => response.downloadHandler.data;
    }
}