using System;
using System.Collections;
using GraphProcessor;
using UnityEditor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Upload Model By GO(Hunyuan)")]
    [UseProcessAsync]
    public class HyUploadModelByGONode : UploadModelByGONode
    {
        [Output(name = "Hunyuan Model Url")] public HyModelOutput outputModelUrl;
        public override string description => DescriptionConstants.HyUploadModelNode;

        public override IEnumerator ProcessAsync()
        {
            yield return base.ProcessAsync();
#if UNITY_EDITOR
            outputModelUrl.asset_path = null;
            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                    // try to get assetPath from mesh
                var meshFilter = obj.GetComponentInChildren<MeshFilter>();
                var skinnedMeshRenderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (meshFilter == null && skinnedMeshRenderer == null)
                    throw new ArgumentNullException("Selected game object doesn't have MeshFilter or SkinnedMeshRenderer");
                assetPath = meshFilter == null ? AssetDatabase.GetAssetPath(skinnedMeshRenderer.sharedMesh) : AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                if (assetPath == null)
                    assetPath = url;
                else
                    outputModelUrl.asset_path = assetPath;
            }
                
            if (assetPath.EndsWith(".fbx"))
                outputModelUrl.fbx_url = url;
            else if (assetPath.EndsWith(".glb"))
                outputModelUrl.glb_url = url;
            else if (assetPath.EndsWith(".obj"))
                outputModelUrl.obj_url = url;
            
            DebugUtils.ConditionLog($"HyModelOutput: {outputModelUrl}");
#endif
        }

        public override void SetTarget(Object target)
        {
            if (target is GameObject obj)
            {
                this.obj = obj;
            } else if (target is Component component)
            {
                this.obj = component.gameObject;
            }
        }
    }
}