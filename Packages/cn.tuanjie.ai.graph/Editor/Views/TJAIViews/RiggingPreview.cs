using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AIGraph;
using UnityObject = UnityEngine.Object;
#if HAS_UNITY_URP
#endif

namespace UnityEditor.AIGraph
{
    public class RiggingPreviewRenderer : BasePreviewRenderer<GameObject>
    {
        private GameObjectPreviewRenderer m_Previewer = null;
        private List<GameObject> boneObjects;
        private GameObject rootObject;
        private SDNode m_Node;
        private static Material s_TransparentMat;

        private static Material transparentMat
        {
            get
            {
                if (!s_TransparentMat)
                {
                    s_TransparentMat = CreateTransparentMaterial(0.25f);
                    s_TransparentMat.hideFlags = HideFlags.DontSaveInEditor;
                }

                return s_TransparentMat;
            }
        }

        private static Material s_BoneMat;
        private static Material boneMaterial
        {
            get
            {
                if (!s_BoneMat)
                {
                    Shader shader = (Shader)Resources.Load<Shader>("Shaders/Bone");

                    s_BoneMat = new Material(shader);
                    s_BoneMat.hideFlags = HideFlags.DontSaveInEditor;
                    s_BoneMat.enableInstancing = true;
                }

                return s_BoneMat;
            }
        }

        private void initBonePreview()
        {
            BoneRendererSetup((rootObject as GameObject).transform);
            ExtractBones();

            boneObjects = new List<GameObject>();
            rootObject.name = rootObject.name + "_rigpreview";

            SkinnedMeshRenderer[] skinnedMeshRenderers = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in skinnedMeshRenderers)
            {
                SetMaterialsToUnlitTransparent(renderer);
            }
            rootObject.hideFlags = HideFlags.HideAndDontSave;

            GenerateBoneObjects();
        }

        public override void Initialize(UnityObject target, SDNode node)
        {
            base.Initialize(target, node);

            if (target == null)
                return;
            m_Node = node;
            m_Previewer = new GameObjectPreviewRenderer();
            m_Previewer.Initialize(target, node);
            m_Previewer.GetPreviewData();
            rootObject = m_Previewer.m_PreviewInstances.gameObject;
            initBonePreview();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            ClearBoneObjects();
            ClearBones();
            Object.DestroyImmediate(transparentMat);
            m_Previewer?.Cleanup();
        }

        public override void Update(UnityObject target)
        {
            Cleanup();
            if (target == null)
                return;

            if (m_Previewer == null)
            {
                m_Previewer = new GameObjectPreviewRenderer();
                m_Previewer.Initialize(target, m_Node);
            }
            else
                m_Previewer.Update(target);

            m_Previewer.GetPreviewData();
            rootObject = m_Previewer.m_PreviewInstances.gameObject;
            initBonePreview();
        }

        public override bool HasPreviewGUI()
        {
            return m_Previewer != null && m_Previewer.HasPreviewGUI() && rootObject != null;
        }

        public override string GetPreviewTitle()
        {
            return "Rigging Preview";
        }

        public override void OnPreviewSettings()
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
                return;
            GUI.enabled = true;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            m_Previewer?.OnPreviewGUI(rect, background);
        }

        public struct TransformPair
        {
            public Transform first;
            public Transform second;
        };

        private TransformPair[] m_Bones;
        private Transform[] m_Tips;

        private Transform[] m_Transforms;

        public void ClearBones()
        {
            m_Bones = null;
            m_Tips = null;
        }

        private void SetMaterialsToUnlitTransparent(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = transparentMat;
            }
            renderer.sharedMaterials = materials;
        }


        public void ExtractBones()
        {
            if (m_Transforms == null || m_Transforms.Length == 0)
            {
                ClearBones();
                return;
            }

            var transformsHashSet = new HashSet<Transform>(m_Transforms);

            var bonesList = new List<TransformPair>(m_Transforms.Length);
            var tipsList = new List<Transform>(m_Transforms.Length);

            for (int i = 0; i < m_Transforms.Length; ++i)
            {
                bool hasValidChildren = false;

                var transform = m_Transforms[i];
                if (transform == null)
                    continue;

                if (UnityEditor.SceneVisibilityManager.instance.IsHidden(transform.gameObject, false))
                    continue;

                var mask = UnityEditor.Tools.visibleLayers;
                if ((mask & (1 << transform.gameObject.layer)) == 0)
                    continue;

                if (transform.childCount > 0)
                {
                    for (var k = 0; k < transform.childCount; ++k)
                    {
                        var childTransform = transform.GetChild(k);

                        if (transformsHashSet.Contains(childTransform))
                        {
                            bonesList.Add(new TransformPair() { first = transform, second = childTransform });
                            hasValidChildren = true;
                        }
                    }
                }

                if (!hasValidChildren)
                {
                    tipsList.Add(transform);
                }
            }

            m_Bones = bonesList.ToArray();
            m_Tips = tipsList.ToArray();
        }

        public void BoneRendererSetup(Transform transform)
        {
            var animator = transform.GetComponent<Animator>();
            var renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            var bones = new List<Transform>();
            if (animator != null && renderers != null && renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var renderer = renderers[i];
                    for (int j = 0; j < renderer.bones.Length; ++j)
                    {
                        var bone = renderer.bones[j];
                        if (!bones.Contains(bone))
                        {
                            bones.Add(bone);

                            for (int k = 0; k < bone.childCount; k++)
                            {
                                if (!bones.Contains(bone.GetChild(k)))
                                    bones.Add(bone.GetChild(k));
                            }
                        }
                    }
                }
            }
            else
            {
                bones.AddRange(transform.GetComponentsInChildren<Transform>());
            }

            m_Transforms = bones.ToArray();
        }

        private void GenerateBoneObjects()
        {
            ClearBoneObjects();

            foreach (var bone in m_Bones)
            {
                if (bone.first == null || bone.second == null) continue;

                GameObject boneGO = CreateBone(bone.first.position, bone.second.position);
                    
                boneGO.transform.SetParent(rootObject.transform);
                boneObjects.Add(boneGO);
                boneGO.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void ClearBoneObjects()
        {
            if (boneObjects == null)
                return;
            foreach (var boneObj in boneObjects)
            {
                if (boneObj != null)
                {
                    Object.DestroyImmediate(boneObj);
                }
            }
            boneObjects.Clear();
        }

        private GameObject CreateBone(Vector3 start, Vector3 end)
        {
            GameObject pyramid = new GameObject("Pyramid");
            pyramid.hideFlags = HideFlags.HideAndDontSave;

            MeshFilter meshFilter = pyramid.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = pyramid.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            Vector3 direction = (end - start).normalized;
            float height = Mathf.Min(Vector3.Distance(start, end), 0.2f);
            float baseRadius = Mathf.Min(height * 0.1f, 0.04f);

            Vector3 baseNormal = direction;
            Vector3 tangent = Vector3.Cross(baseNormal, Vector3.up).normalized;
            if (tangent.magnitude < 0.1f)
            {
                tangent = Vector3.Cross(baseNormal, Vector3.right).normalized;
            }
            Vector3 bitangent = Vector3.Cross(baseNormal, tangent).normalized;

            Vector3[] vertices = new Vector3[4];

            Vector3 baseCenter = start + direction * baseRadius * 0.5f;

            vertices[0] = baseCenter + Quaternion.AngleAxis(0, baseNormal) * tangent * baseRadius;
            vertices[1] = baseCenter + Quaternion.AngleAxis(120, baseNormal) * tangent * baseRadius;
            vertices[2] = baseCenter + Quaternion.AngleAxis(240, baseNormal) * tangent * baseRadius;

            vertices[3] = Vector3.Distance(baseCenter, end) > Vector3.Distance(baseCenter, baseCenter + direction * 0.3f) ? baseCenter + direction * 0.3f : end;

            int[] triangles = new int[] {
                0, 2, 1,
                0, 1, 3,
                1, 2, 3,
                2, 0, 3
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshRenderer.material = boneMaterial;

            return pyramid;
        }

        public static Material CreateTransparentMaterial(float alpha)
        {
            RenderPipelineAsset currentPipeline = GraphicsSettings.renderPipelineAsset;
            Material mat = null;
            if (currentPipeline == null)
            {
                mat = SetupBuiltinTransparency(alpha);
            }
            else if (currentPipeline.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            {
                mat = SetupURPTransparency(alpha);
            }
            else if (currentPipeline.GetType().Name.Contains("HDRenderPipelineAsset"))
            {
                mat = SetupHDRPTransparency(alpha);
            }

            if (mat == null)
            {
                mat = new Material(currentPipeline.defaultMaterial);
#if HAS_UNITY_URP
                UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(mat);
#endif
            }

            return mat;
        }

        private static Material SetupBuiltinTransparency(float alpha)
        {
            var mat = new Material(Shader.Find("Standard"));

            mat.SetFloat("_Mode", 3); // 3 = Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            Color color = mat.color;
            color.a = alpha;
            mat.color = color;

            return mat;
        }

        private static Material SetupURPTransparency(float alpha)
        {
            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader != null)
            {
                
                var mat = new Material(urpLitShader);
                
                mat.SetFloat("_Surface", 1);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                if (mat.HasProperty("_BaseColor"))
                {
                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor.a = alpha;
                    mat.SetColor("_BaseColor", baseColor);
                }
                else
                {
                    SetMaterialAlpha(mat, alpha);
                }
#if HAS_UNITY_URP
                UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(mat);
#endif
                return mat;
            }
            return null;
        }

        private static Material SetupHDRPTransparency(float alpha)
        {
            Shader hdrpLitShader = Shader.Find("HDRP/Lit");
            if (hdrpLitShader != null)
            {
                var mat = new Material(hdrpLitShader);

                mat.SetFloat("_Surface", 1);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                if (mat.HasProperty("_BaseColor"))
                {
                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor.a = alpha;
                    mat.SetColor("_BaseColor", baseColor);
                }
                else
                {
                    SetMaterialAlpha(mat, alpha);
                }
#if HAS_UNITY_URP
                UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(mat);
#endif

                return mat;
            }
            return null;
        }

        private static void SetMaterialAlpha(Material mat, float alpha)
        {
            string[] alphaProperties = { "_Color", "_BaseColor", "_TintColor", "_MainColor" };

            foreach (string property in alphaProperties)
            {
                if (mat.HasProperty(property))
                {
                    Color color = mat.GetColor(property);
                    color.a = alpha;
                    mat.SetColor(property, color);
                    return;
                }
            }
        }
    }
}