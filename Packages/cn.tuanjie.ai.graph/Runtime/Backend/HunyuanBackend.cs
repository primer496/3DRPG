using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace UnityEngine.AIGraph.Backend
{
    internal struct HyViewsToGeometryRequest
    {
        public string frontImage;
        public string backImage;
        public string leftImage;
        public string rightImage;
        public int? seed;
        public int? n;
        public int faceCount;
        public string polygonType;
        public bool strictMode;

        public override string ToString()
        {
            return $"HyViewsToGeometryRequest(frontImage={frontImage?.Length}, " +
                   $"backImage={backImage?.Length}, leftImage={leftImage?.Length}, " +
                   $"rightImage={rightImage?.Length}, seed={seed}, n={n}," +
                   $"faceCount={faceCount}, polygonType={polygonType}, strictMode={strictMode})";
        }
    }


    internal struct HyViewsToGeometryData
    {
        public string glbUrl;
        public string imagUrl;

        public override string ToString()
        {
            return $"HyViewsToGeometryData(glbUrl={glbUrl}, imagUrl={imagUrl})";
        }
    }
    
    internal struct HyViewsToGeometryOutput
    {
        public string id;
        public int created;
        public List<HyViewsToGeometryData> data;

        public override string ToString()
        {
            return $"HyViewsToGeometryOutput(id={id}, created={created}, " +
                   $"data=[{string.Join(",", data)}])";
        }
    }
    
    internal struct HySemanticUVRequest
    {
        public bool enableAutoSmoothing;
        public string fbxUrl;
        public string objUrl;
        public string glbUrl;
        public int? n;          // 可选，默认 1

        public override string ToString() => 
            $"HySemanticUVRequest(enableAutoSmooting={enableAutoSmoothing}, fbx={fbxUrl ?? "null"} " +
            $"obj={objUrl ?? "null"}, glb={glbUrl ?? "null"}, n={n ?? 1})";
    }
    
    internal struct Hy3DAnimationWFRequest
    {
        public string fbxUrl;
        public string objUrl;
        public string mtlUrl;
        public string textureImageUrl;
        public int motionType;

        public override string ToString()
        {
            return $"Hy3DAnimationWFRequest(fbxUrl={fbxUrl}, objUrl={objUrl}, " +
                   $"mtlUrl={mtlUrl}, textureImageUrl={textureImageUrl}, motionType={motionType})";
        }
    }

    internal struct HyMotionRetargetRequest
    {
        public string fbxUrl;
        public int motionType;
        public int n;

        public override string ToString()
        {
            return $"HyMotionRetargetRequest(fbxUrl={fbxUrl}, motionType={motionType}, n={n})";
        }
    }
    
    internal struct HyLowPolyRequest
    {
        public string glbUrl;
        public string objUrl;
        public string polygonType;
        public int n;

        public override string ToString()
        {
            return $"HyLowPolyRequest(glbUrl={glbUrl}, objUrl={objUrl}, " +
                   $"polygonType={polygonType}, n={n})";
        }
    }
    internal struct HyImageSubjectSegmentationRequest
    {
        public string image;
        public float segmentationThreshold;

        public override string ToString()
        {
            return $"HyImageSubjectSegmentationRequest(image={image.Length}, " +
                   $"segmentationThreshold={segmentationThreshold})";
        }
    }
    internal struct HyImageControlnetGrayScaleRequest
    {
        public string image;
        public string prompt;
        public int seed;

        public override string ToString()
        {
            return $"HyImageControlnetGrayScaleRequest(image={image?.Length}, " +
                   $"prompt={prompt}, seed={seed})";
        }
    }
    
    internal struct HyTexttoImageV3Request
    {
        public string prompt;
        public string size;
        public bool revise;
        public int seed;
        public bool enableThinking;

        public override string ToString()
        {
            return $"HyImageGeneratingRequest(prompt={prompt}, size={size}, " +
                   $"revise={revise}, seed={seed}, enableThinking={enableThinking})";
        }
    }

    internal struct HyImageGeneratingRequest
    {
        public string prompt;
        public string image;
        public string size;
        public string style;
        public bool revise;
        public int n;
        public int seed;
        public bool ignoreStyleForIrag;

        public override string ToString()
        {
            return $"HyImageGeneratingRequest(prompt={prompt}, image={image?.Length}, size={size}, style={style}, " +
                   $"revise={revise}, n={n}, seed={seed}, ignoreStyleForIrag={ignoreStyleForIrag})";
        }
    }

    internal struct HyImageCharacterThreeViewRequest
    {
        public string image;

        public override string ToString()
        {
            return $"HyImageCharacterThreeViewRequest(image={image.Length}";
        }
    }
    internal struct HyImageFlexibilityConsistencyRequest
    {
        public string prompt;
        public string version;
        public string image;
        public string size;
        public int n;
        public int seed;

        public override string ToString()
        {
            return $"HyImageFlexibilityConsistencyRequest(prompt={prompt}, version={version}, " +
                   $"image={image?.Length}, size={size}, n={n}, seed={seed})";
        }
    }
    internal struct HyImageStyleSwitchRequest
    {
        public string image;
        public string style;
        public int n;
        public int seed;

        public override string ToString()
        {
            return $"HyImageStyleSwitchRequest(image={image.Length}, " +
                   $"style={style}, n={n}, seed={seed})";
        }
    }


    internal struct HyImagePoseStandardizationRequest
    {
        public string image;
        public int n;

        public override string ToString()
        {
            return $"HyImagePoseStandardizationRequest(image={image.Length}, " +
                   $"n={n})";
        }
    }
    internal struct HyImageToTextureRequest
    {
        public string image; // 图生纹理时必填。
        public string glbUrl;
        public string objUrl;
        public bool keepUv;
        public int n;
        public bool enablePbr;

        public override string ToString()
        {
            return $"HyImageToTextureRequest(image={image.Length}, glbUrl={glbUrl}, objUrl={objUrl}, " +
                   $"keepUv={keepUv}, n={n}, enablePbr={enablePbr})";
        }
    }
    internal struct HyImageToGeometryRequest
    {
        public string image;
        public int n;
        public bool enablePbr;
        public int faceCount;
        public bool strictMode;
        public string polygonType;
        public string prompt;

        public override string ToString()
        {
            return $"HyImageToGeometryRequest(image={image?.Length}, n={n}," +
                   $"enablePbr={enablePbr}, faceCount={faceCount}, prompt={prompt}, polygonType={polygonType}, strictMode={strictMode})";
        }
    }
    internal struct HyAutoRiggingRequest
    {
        public string fbxUrl;
        public string objUrl;
        public string mtlUrl;
        public string glbUrl;
        public string textureImageUrl;
        public string pbrMetallicImageUrl;
        public string pbrRoughnessImageUrl;
        public string pbrNormalImageUrl;
        public string pbrImageUrl;
        public int n;

        public override string ToString()
        {
            return $"HyAutoRiggingRequest(fbxUrl={fbxUrl}, objUrl={objUrl}, mtlUrl={mtlUrl}, " +
                   $"glbUrl={glbUrl}, textureImage={textureImageUrl}, pbrMetallicImage={pbrMetallicImageUrl}, " +
                   $"pbrRoughnessImage={pbrRoughnessImageUrl}, pbrNormalImage={pbrNormalImageUrl}, " +
                   $"pbrImage={pbrImageUrl}, n={n})";
        }
    }
    internal struct HyTextToPanoramaRequest
    {
        public string prompt;
        public int n;

        public override string ToString()
        {
            return $"HyTextToPanoramaRequest(prompt={prompt}, n={n})";
        }
    }
    internal struct Hy3DFormatConversionRequest
    {
        public string fbxUrl;
        public string objZipUrl;
        public string responseFormat;

        public override string ToString()
        {
            return $"Hy3DFormatConversionRequest(fbxUrl={fbxUrl}, objZipUrl={objZipUrl}, responseFormat={responseFormat})";
        }
    }
    internal struct HyImageClarityRequest
    {
        public string version;
        public string image;
        public string imageUrl;
        public int n;

        public override string ToString()
        {
            return $"HyImageClarityRequest(version={version}, " +
                   $"image={image?.Length}, imageUrl={imageUrl}, " +
                   $"n={n})";
        }
    }
    internal struct HySketch2MeshRequest
    {
        public string prompt;
        public string sketchImage;
        public int faceCount;
        public bool strictMode;
        public bool enablePbr;

        public override string ToString()
        {
            return $"HySketch2MeshRequest(prompt={prompt}, sketchImage={sketchImage?.Length}," +
                   $"faceCount={faceCount}, strictMode={strictMode}, enablePbr={enablePbr})";
        }
    }
    internal struct HyBackgroundReplacementRequest
    {
        public string version;
        public string image;
        public string imageUrl;
        public string mask;
        public string maskUrl;
        public string prompt;
        public int n;

        public override string ToString()
        {
            return $"HyBackgroundReplacementRequest(version={version}, " +
                   $"image={image?.Length}, imageUrl={imageUrl}, " +
                   $"mask={mask?.Length}, maskUrl={maskUrl}, " +
                   $"prompt={prompt}, n={n})";
        }
    }
    internal struct HyViewsToTextureRequest
    {
        public string frontImage;
        public string backImage;
        public string leftImage;
        public string rightImage;
        public string glbUrl;
        public string objUrl;
        public bool keepUv;
        public int seed;
        public int n;
        public bool enablePbr;

        public override string ToString()
        {
            return $"HyViewsToTextureRequest(frontImage={frontImage?.Length}, backImage={backImage?.Length}, " +
                   $"leftImage={leftImage?.Length}, rightImage={rightImage?.Length}, glbUrl={glbUrl}, " +
                   $"objUrl={objUrl}, keepUv={keepUv}, seed={seed}, n={n}, enablePbr={enablePbr})";
        }
    }

    internal struct HyTextToImageOutput
    {
        [CanBeNull] public string revised_prompt;
        [CanBeNull] public string style;
        [CanBeNull] public string url;
        [CanBeNull] public List<string> image_urls;

        public override string ToString()
        {
            return $"HyTextToImageOutput(revised_prompt= {revised_prompt}, style={style}, url= {url}," +
                   $"image_urls={DebugUtils.ToString(image_urls)})";
        }
    }
    
    public struct HyModelID
    {
        public string id;
        public override string ToString()
        {
            return $"HyModelID(id={id})";
        }
    }

    [Serializable]
    public struct HyModelOutput
    {
        [CanBeNull] public string glb_url;
        [CanBeNull] public string fbx_url;
        [CanBeNull] public string obj_zip_url;
        [CanBeNull] public string obj_url;
        [CanBeNull] public string image_url;
        [CanBeNull] public string texture_image_url;
        [CanBeNull] public string pbr_metallic_image_url;
        [CanBeNull] public string pbr_roughness_image_url;
        [CanBeNull] public string pbr_normal_image_url;
        [CanBeNull] public string pbr_image_url;
        [CanBeNull] public string mtl_url;

        [CanBeNull] public string asset_path;
        public override string ToString()
        {
            return $"HyModelOutput(obj_url={obj_url}, glb_url={glb_url}, " +
                   $"image_url={image_url}, texture_image_url={texture_image_url}, " +
                   $"pbr_metallic_image_url={pbr_metallic_image_url}, " +
                   $"pbr_roughness_image_url={pbr_roughness_image_url}, " +
                   $"pbr_normal_image_url={pbr_normal_image_url}, " +
                   $"pbr_image_url={pbr_image_url}, mtl_url={mtl_url}, obj_zip_url={obj_zip_url}, " +
                   $"fbx_url={fbx_url})";
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(glb_url) && string.IsNullOrEmpty(fbx_url) &&
                   string.IsNullOrEmpty(obj_zip_url) && string.IsNullOrEmpty(obj_url);
        }
    }
    internal struct Hy3DFormatConversionOutput
    {
        public string stl_url;
        public string usdz_url;
        public string fbx_url;
        public string mp4_url;
        public string gif_url;
        public override string ToString()
        {
            return $"Hy3DFormatConversionOutput(stl_url={stl_url}, usdz_url={usdz_url}," +
                   $"fbx_url={fbx_url}, mp4_url={mp4_url}, gif_url={gif_url})";
        }
    }

    public struct HyPanoramaOutput
    {
        public string panorama_image_url;
        public string cover_image_url;
        public override string ToString()
        {
            return $"HyPanoramaOutput(panorama_image_url={panorama_image_url}, cover_image_url={cover_image_url})";
        }
    }

    public struct HyUploadRequest
    {
        public string fileName;
        public byte[] fileData;
        public override string ToString()
        {
            return $"HyUploadRequest(fileName={fileName}, fileData={fileData.Length})";
        }
    }
    
    public struct HyUploadResponse
    {
        public string objectKey;
        public string requestId;
        public string url;
        public override string ToString()
        {
            return $"HyUploadResponse(objectKey={objectKey}, requestId={requestId}, url={url})";
        }
    }
    
    internal class HyUploadModelRestCall : TJAIRestCall<HyUploadRequest, HyUploadResponse>
    {
        public HyUploadModelRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/upload/model";
    
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;

        protected override void PrepareRequest(HyUploadRequest request, ref UnityWebRequest uRequest)
        {
            // step 1: set Authorization
            uRequest.SetRequestHeader("accept", "application/json");
            uRequest.SetRequestHeader("X-VS", Application.tuanjieVersion);

            string accessToken = UnityConnectProxy.instance.GetAccessToken();
            uRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
            // step 2: set model data
            var formData = new WWWForm();
            // 添加 byte[] 数据到 "model" 字段
            formData.AddBinaryData("model", request.fileData, 
                StringUtils.CleanFileName(request.fileName), "application/octet-stream");
            // 设置上传处理器
            uRequest.uploadHandler = new UploadHandlerRaw(formData.data);
            // 设置 header
            foreach (var kvp in formData.headers)
            {
                uRequest.SetRequestHeader(kvp.Key, kvp.Value);
            }
        }
    }
    
    internal class HyUploadImageRestCall : TJAIRestCall<HyUploadRequest, HyUploadResponse>
    {
        public HyUploadImageRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/upload/image";
    
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;

        protected override void PrepareRequest(HyUploadRequest request, ref UnityWebRequest uRequest)
        {
            // step 1: set Authorization
            uRequest.SetRequestHeader("accept", "application/json");
            uRequest.SetRequestHeader("X-VS", Application.tuanjieVersion);

            string accessToken = UnityConnectProxy.instance.GetAccessToken();
            uRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
            // step 2: set image data
            var formData = new WWWForm();
            // 添加 byte[] 数据到 "image" 字段
            formData.AddBinaryData("image", request.fileData,
                StringUtils.CleanFileName(request.fileName, "image"), 
                "application/octet-stream");
            // 设置上传处理器
            uRequest.uploadHandler = new UploadHandlerRaw(formData.data);
            // 设置 header
            foreach (var kvp in formData.headers)
            {
                uRequest.SetRequestHeader(kvp.Key, kvp.Value);
            }
        }
    }
    
    internal class HyViewsToGeometryRestCall : TJAIRestCall<HyViewsToGeometryRequest, TaskSubmitResponse>
    {
        public HyViewsToGeometryRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/hunyuan-3d-views2geometry-wf";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class HySemanticUVRestCall : TJAIRestCall<HySemanticUVRequest, TaskSubmitResponse>
    {
        public HySemanticUVRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/hunyuan-3d-semantic-uv-v2";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class Hy3DAnimationWFRestCall : TJAIRestCall<Hy3DAnimationWFRequest, TaskSubmitResponse>
    {
        public Hy3DAnimationWFRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-animation-wf";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class HyMotionRetargetRestCall : TJAIRestCall<HyMotionRetargetRequest, TaskSubmitResponse>
    {
        public HyMotionRetargetRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/hunyuan-3d-motion-retarget";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    
    internal class HyLowPolyRestCall : TJAIRestCall<HyLowPolyRequest, TaskSubmitResponse>
    {
        public HyLowPolyRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-low-poly";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageSubjectSegmentationRestCall : TJAIRestCall<HyImageSubjectSegmentationRequest, TaskSubmitResponse>
    {
        public HyImageSubjectSegmentationRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-images-subject-segmentation";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyTexttoImageV3RestCall : TJAIRestCall<HyTexttoImageV3Request, TaskSubmitResponse>
    {
        public HyTexttoImageV3RestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-text2image-v3";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageGeneratingRestCall : TJAIRestCall<HyImageGeneratingRequest, TaskSubmitResponse>
    {
        public HyImageGeneratingRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-all-in-one-irag";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageControlnetGrayScaleRestCall : TJAIRestCall<HyImageControlnetGrayScaleRequest, TaskSubmitResponse>
    {
        public HyImageControlnetGrayScaleRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-taurus-controlnet-gray-scale";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageCharacterThreeViewRestCall : TJAIRestCall<HyImageCharacterThreeViewRequest, TaskSubmitResponse>
    {
        public HyImageCharacterThreeViewRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-taurus-character-three-view";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageFlexibilityConsistencyRestCall : TJAIRestCall<HyImageFlexibilityConsistencyRequest, TaskSubmitResponse>
    {
        public HyImageFlexibilityConsistencyRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-flexibility-consistency";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageStyleSwitchRestCall : TJAIRestCall<HyImageStyleSwitchRequest, TaskSubmitResponse>
    {
        public HyImageStyleSwitchRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-style-switches-pro";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImagePoseStandardizationRestCall : TJAIRestCall<HyImagePoseStandardizationRequest, TaskSubmitResponse>
    {
        public HyImagePoseStandardizationRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-images-pose-standardization";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageToTextureRestCall : TJAIRestCall<HyImageToTextureRequest, TaskSubmitResponse>
    {
        public HyImageToTextureRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => "/api/editor/task/hunyuan-3d-texture-to-image-v3";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageToTexturePBRRestCall : TJAIRestCall<HyImageToTextureRequest, TaskSubmitResponse>
    {
        public HyImageToTexturePBRRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => "/api/editor/task/hunyuan-3d-texture-to-image-v3";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageToGeometryRestCall : TJAIRestCall<HyImageToGeometryRequest, TaskSubmitResponse>
    {
        public HyImageToGeometryRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-image2gen-wf";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyTextToGeometryRestCall : TJAIRestCall<HyImageToGeometryRequest, TaskSubmitResponse>
    {
        public HyTextToGeometryRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-text2gen-wf";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyAutoRiggingRestCall : TJAIRestCall<HyAutoRiggingRequest, TaskSubmitResponse>
    {
        public HyAutoRiggingRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-auto-rigging";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyTextToPanoramaRestCall : TJAIRestCall<HyTextToPanoramaRequest, TaskSubmitResponse>
    {
        public HyTextToPanoramaRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-world-text-to-panorama";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class Hy3DFormatConversionRestCall : TJAIRestCall<Hy3DFormatConversionRequest, TaskSubmitResponse>
    {
        public Hy3DFormatConversionRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-3d-format-conversions";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyImageClarityRestCall : TJAIRestCall<HyImageClarityRequest, TaskSubmitResponse>
    {
        public HyImageClarityRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-clarity";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HySketch2MeshRestCall : TJAIRestCall<HySketch2MeshRequest, TaskSubmitResponse>
    {
        public HySketch2MeshRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => "/api/editor/task/hunyuan-3d-sketch2gen-wf";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyBackgroundReplacementRestCall : TJAIRestCall<HyBackgroundReplacementRequest, TaskSubmitResponse>
    {
        public HyBackgroundReplacementRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => "/api/editor/task/hunyuan-image-background-replacements";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyViewsToTextureRestCall : TJAIRestCall<HyViewsToTextureRequest, TaskSubmitResponse>
    {
        public HyViewsToTextureRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/hunyuan-3d-views2texture-v3";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class HyViewsToTexturePBRRestCall : TJAIRestCall<HyViewsToTextureRequest, TaskSubmitResponse>
    {
        public HyViewsToTexturePBRRestCall(ServerConfig asset, int mode) : base(asset, mode) { }
        public override string endPoint => $"/api/editor/task/hunyuan-3d-views2texture-v3";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
}