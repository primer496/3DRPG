using System.Collections.Generic;
using JetBrains.Annotations;

namespace UnityEngine.AIGraph.Backend
{
    internal struct Rodin3DGenerationRequest
    {
        public List<string> addons;
        public List<float> bboxCondition;
        public string conditionMode;
        public string geometryFormat;
        public List<string> imageBase64List;
        public List<string> imageUrls;
        public string material;
        public string meshMode;
        public bool meshSimplify;
        public bool meshSmooth;
        public string prompt;
        public string quality;
        public int qualityOverride;
        public ushort seed;
        public bool taPose;
        public string tier;
        public bool useOriginalAlpha;

        public override string ToString()
        {
            return $"Rodin3DGenerationRequest(addons={addons}, bboxCondition={DebugUtils.ToString(bboxCondition)}, " +
                   $"conditionMode={conditionMode}, geometryFormat={geometryFormat}, " +
                   $"imageUrls={DebugUtils.ToString(imageUrls)}, " +
                   $"material={material}, meshMode={meshMode}, " +
                   $"meshSimplify={meshSimplify}, meshSmooth={meshSmooth}, prompt={prompt}, " +
                   $"quality={quality}, qualityOverride={qualityOverride}, seed={seed}, " +
                   $"taPose={taPose}, tier={tier}, useOriginalAlpha={useOriginalAlpha}," +
                   $"imageBase64List=[{DebugUtils.ToString(imageBase64List)}])";
        }
    }
    internal struct RodinTextureRequest
    {
        public string imageUrl;
        public string imageBase64;
        public string prompt;
        public string modelUrl;
        public ushort seed;
        public float referenceScale;
        public string geometryFileFormat;
        public string material;
        public string resolution;
        public float escore;

        public override string ToString()
        {
            return $"RodinTextureRequest(image={imageBase64?.Length}, imageUrl={imageUrl}" +
                   $"prompt={prompt}, model={modelUrl}, " +
                   $"seed={seed}, referenceScale={referenceScale}, geometryFileFormat={geometryFileFormat}, " +
                   $"material={material}, resolution={resolution}, escore={escore})";
        }
    }

    internal struct RodinSkyboxRequest
    {
        public string prompt;
        public List<string> imageBase64s;
        public bool high_res;
        public override string ToString()
        {
            return $"RodinSkyboxRequest(prompt={prompt}, images={DebugUtils.ToString(imageBase64s)}, " +
                   $"highRes={high_res})";
        }
    }

    public struct RodinModelOutput
    {
        [CanBeNull] public string base_basic_pbr;
        public override string ToString()
        {
            return $"RodinModelOutput(base_basic_pbr={base_basic_pbr})";
        }
    }

    public struct RodinImageOutput
    {
        [CanBeNull] public string hdr;
        [CanBeNull] public string skybox_basic;
        public override string ToString()
        {
            return $"RodinImageOutput(hdr={hdr}, skybox_basic={skybox_basic})";
        }
    }

    internal class Rodin3DGenerationRestCall : TJAIRestCall<Rodin3DGenerationRequest, TaskSubmitResponse>
    {
        public Rodin3DGenerationRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/api/editor/task/rodin-generation";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class RodinTextureRestCall : TJAIRestCall<RodinTextureRequest, TaskSubmitResponse>
    {
        public RodinTextureRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/rodin-texture";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
    internal class RodinSkyboxRestCall : TJAIRestCall<RodinSkyboxRequest, TaskSubmitResponse>
    {
        public RodinSkyboxRestCall(ServerConfig asset, int mode) : base(asset, mode) { }

        public override string endPoint => $"/api/editor/task/rodin-skybox";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }
}