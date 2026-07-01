using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    public enum UploadModelType
    {
        FBX,
        GLB,
        OBJ
    }

    [System.Serializable, NodeMenuItem("Hunyuan/Upload Model By Url(Hunyuan)")]
    public class HyUploadModelByUrlNode : SDNode
    {
        [Output(name = "Model Url"), SerializeField] public HyModelOutput outputModelUrl;
        [HideInInspector] public UploadModelType uploadModelType = UploadModelType.FBX;
        // [HideInInspector] public string glbUrl;
        // [HideInInspector] public string fbxUrl;
        // [HideInInspector] public string objUrl;
        // [HideInInspector] public string mtlUrl;
        // [HideInInspector] public string textureUrl;
        // [HideInInspector] public string pbrMetallicUrl;
        // [HideInInspector] public string pbrRoughnessUrl;
        // [HideInInspector] public string pbrNormalsUrl;
        // [HideInInspector] public string pbrBaseMapUrl;
        public override string description => DescriptionConstants.HyUploadModelNode;
        public override bool needTrigger => true;
        public override bool isRenamable => true;
        public override string name => LocalizationManager.Instance.GetLocalizedText("UploadModelByUrl");


        public override void Process()
        {
            DebugUtils.ConditionLog($"Model Url: {outputModelUrl}");
            // if (uploadModelType == UploadModelType.FBX)
            // {
            //     outputModelUrl.fbx_url = fbxUrl;
            // }
            // else if (uploadModelType == UploadModelType.GLB)
            // {
            //     outputModelUrl = new HyGlbModelUrl { glbUrl = glbUrl };
            // }
            // else if (uploadModelType == UploadModelType.OBJ)
            // {
            //     outputModelUrl = new HyObjModelUrl
            //     {
            //         objUrl = objUrl, mtlUrl = mtlUrl,
            //         textureImageUrl = textureUrl, pbrMetallicImageUrl = pbrMetallicUrl,
            //         pbrRoughnessImageUrl = pbrRoughnessUrl, pbrNormalImageUrl = pbrNormalsUrl,
            //         pbrImageUrl = pbrBaseMapUrl
            //     };
            // }
            // else
            //     throw new ArgumentException($"Invalid model type: {uploadModelType}");
        }
    }
}