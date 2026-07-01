using System;
using UnityEditor.AIGraph.InternalBridge;
using UnityEngine;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    public class MaterialPreviewRenderer : BasePreviewRenderer<Material>
    {
        private static class Styles
        {
            public static readonly GUIStyle inspectorBigInner = "IN BigTitle inner";

            public static readonly GUIContent reflectionProbePickerIcon = EditorGUIUtility.TrIconContent("ReflectionProbeSelector");

            public static readonly GUIContent lightmapEmissiveLabelRealtimeGISupport = EditorGUIUtility.TrTextContent("Global Illumination", "Controls if the emission is Baked or Realtime.\n\nBaked only has effect in scenes where Baked Global Illumination is enabled.\n\nRealtime uses Realtime Global Illumination if enabled in the scene. Otherwise the emission won't light up other objects.");

            public static readonly GUIContent lightmapEmissiveLabel = EditorGUIUtility.TrTextContent("Global Illumination", "Controls if the emission is Baked or Realtime.\n\nBaked only has effect in scenes where Baked Global Illumination is enabled.\n\nRealtime won't light up other objects since Realtime Global Illumination is not supported.");

            public static GUIContent[] lightmapEmissiveStrings = new GUIContent[3]
            {
                InternalAPI.Internal_EditorGUIUtility_TextContent("Realtime"),
                EditorGUIUtility.TrTextContent("Baked"),
                EditorGUIUtility.TrTextContent("None")
            };

            public static int[] lightmapEmissiveValues = new int[3] { 1, 2, 0 };

            public static string propBlockInfo = EditorGUIUtility.TrTextContent("MaterialPropertyBlock is used to modify these values").text;

            public const int kNewShaderQueueValue = -1;

            public const int kCustomQueueIndex = 4;

            public static readonly GUIContent queueLabel = EditorGUIUtility.TrTextContent("Render Queue");

            public static readonly GUIContent[] queueNames = new GUIContent[4]
            {
                EditorGUIUtility.TrTextContent("From Shader"),
                EditorGUIUtility.TrTextContent("Geometry", "Queue 2000"),
                EditorGUIUtility.TrTextContent("AlphaTest", "Queue 2450"),
                EditorGUIUtility.TrTextContent("Transparent", "Queue 3000")
            };

            public static readonly int[] queueValues = new int[4] { -1, 2000, 2450, 3000 };

            public static GUIContent[] customQueueNames = new GUIContent[5]
            {
                queueNames[0],
                queueNames[1],
                queueNames[2],
                queueNames[3],
                InternalAPI.Internal_EditorGUIUtility_TextContent("")
            };

            public static int[] customQueueValues = new int[5]
            {
                queueValues[0],
                queueValues[1],
                queueValues[2],
                queueValues[3],
                0
            };

            public static readonly GUIContent enableInstancingLabel = EditorGUIUtility.TrTextContent("Enable GPU Instancing");

            public static readonly GUIContent doubleSidedGILabel = EditorGUIUtility.TrTextContent("Double Sided Global Illumination", "When enabled, the lightmapper accounts for both sides of the geometry when calculating Global Illumination. Backfaces are not rendered or added to lightmaps, but get treated as valid when seen from other objects. When using the Progressive Lightmapper backfaces bounce light using the same emission and albedo as frontfaces.");

            public static readonly GUIContent emissionLabel = EditorGUIUtility.TrTextContent("Emission");

            public const string undoAssignMaterial = "Assign Material";

            public const string undoAssignSkyboxMaterial = "Assign Skybox Material";

            public static readonly GUIContent parentContent = EditorGUIUtility.TrTextContent("Parent", "Specify the parent of this material.");

            public static readonly GUIContent hierarchyIcon = EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow", "|Open Material Hierarchy Popup.");

            public static readonly GUIContent convertIcon = EditorGUIUtility.IconContent("d_RotateTool", "|This material is in a conversion process.");

            public const int kPadding = 3;

            public const int kHierarchyIconWidth = 44;

            public const float kSpaceForFoldoutArrow = 10f;
        }

        internal class ReflectionProbePicker : PopupWindowContent
        {
            private ReflectionProbe m_SelectedReflectionProbe;

            public Transform Target => (m_SelectedReflectionProbe != null) ? m_SelectedReflectionProbe.transform : null;

            public override Vector2 GetWindowSize()
            {
                return new Vector2(170f, 56f);
            }

            public void OnEnable()
            {
                m_SelectedReflectionProbe = EditorUtility.InstanceIDToObject(SessionState.GetInt("PreviewReflectionProbe", 0)) as ReflectionProbe;
            }

            public void OnDisable()
            {
                SessionState.SetInt("PreviewReflectionProbe", m_SelectedReflectionProbe ? m_SelectedReflectionProbe.GetInstanceID() : 0);
            }

            public override void OnGUI(Rect rc)
            {
                EditorGUILayout.LabelField("Select Reflection Probe", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                m_SelectedReflectionProbe = EditorGUILayout.ObjectField("", m_SelectedReflectionProbe, typeof(ReflectionProbe), true) as ReflectionProbe;
            }
        }

        private Vector2 m_PreviewDir = new Vector2(0f, -20f);

        private enum PreviewType
        {
            Mesh,
            Plane,
            Skybox
        }
        private int m_SelectedMesh;
        private static readonly Mesh[] s_Meshes = new Mesh[5];
        private static readonly GUIContent[] s_MeshIcons = new GUIContent[5];
        private static readonly GUIContent[] s_LightIcons = new GUIContent[2];
        private static readonly GUIContent[] s_TimeIcons = new GUIContent[2];
        private static Mesh s_PlaneMesh;
        private int m_TimeUpdate;
        private int m_LightMode = 1;
        private static PreviewRenderUtility s_PreviewRenderUtility;
        private ReflectionProbePicker m_ReflectionProbePicker = new ReflectionProbePicker();

        public override void Initialize(UnityEngine.Object target, SDNode node)
        {
            base.Initialize(target, node);

            m_SelectedMesh = EditorPrefs.GetInt("DefaultMaterialPreviewMesh");

            if (GetPreviewType(base.target as Material) == PreviewType.Skybox)
            {
                m_PreviewDir = new Vector2(0f, 50f);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            s_PreviewRenderUtility?.Cleanup();
            s_PreviewRenderUtility = null;
        }

        public override void Update(UnityEngine.Object target)
        {
            base.Update(target);
        }

        public override bool HasPreviewGUI()
        {
            return target != null;
        }

        public override string GetPreviewTitle()
        {
            return GetPreviewTitleStatic(target).text;
        }

        public override void OnPreviewSettings()
        {
            // Doesn't Support Custom Shader GUI Currently
            DefaultPreviewSettingsGUI();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            // Doesn't Support Custom Shader GUI Currently
            DefaultPreviewGUI(rect, background);
        }

        private void Init()
        {
            if (!(s_Meshes[0] == null))
            {
                return;
            }

            GameObject gameObject = (GameObject)EditorGUIUtility.LoadRequired("Previews/PreviewMaterials.fbx");
            gameObject.SetActive(value: false);
            foreach (Transform item in gameObject.transform)
            {
                MeshFilter component = item.GetComponent<MeshFilter>();
                switch (item.name)
                {
                    case "sphere":
                        s_Meshes[0] = component.sharedMesh;
                        break;
                    case "cube":
                        s_Meshes[1] = component.sharedMesh;
                        break;
                    case "cylinder":
                        s_Meshes[2] = component.sharedMesh;
                        break;
                    case "torus":
                        s_Meshes[3] = component.sharedMesh;
                        break;
                    default:
                        Debug.Log("Something is wrong, weird object found: " + item.name);
                        break;
                }
            }

            s_MeshIcons[0] = EditorGUIUtility.TrIconContent("PreMatSphere");
            s_MeshIcons[1] = EditorGUIUtility.TrIconContent("PreMatCube");
            s_MeshIcons[2] = EditorGUIUtility.TrIconContent("PreMatCylinder");
            s_MeshIcons[3] = EditorGUIUtility.TrIconContent("PreMatTorus");
            s_MeshIcons[4] = EditorGUIUtility.TrIconContent("PreMatQuad");
            s_LightIcons[0] = EditorGUIUtility.TrIconContent("PreMatLight0");
            s_LightIcons[1] = EditorGUIUtility.TrIconContent("PreMatLight1");
            s_TimeIcons[0] = EditorGUIUtility.TrIconContent("PlayButton");
            s_TimeIcons[1] = EditorGUIUtility.TrIconContent("PauseButton");
            Mesh mesh = Resources.GetBuiltinResource(typeof(Mesh), "Quad.fbx") as Mesh;
            s_Meshes[4] = mesh;
            s_PlaneMesh = mesh;
        }

        private static PreviewType GetPreviewType(Material mat)
        {
            if (mat == null)
            {
                return PreviewType.Mesh;
            }

            string text = mat.GetTag("PreviewType", searchFallbacks: false, string.Empty).ToLower();
            if (text == "plane")
            {
                return PreviewType.Plane;
            }

            if (text == "skybox")
            {
                return PreviewType.Skybox;
            }

            if (mat.shader != null && mat.shader.name.Contains("Skybox"))
            {
                return PreviewType.Skybox;
            }

            return PreviewType.Mesh;
        }


        public void DefaultPreviewSettingsGUI()
        {
            Material material = target;
            if (!SupportRenderingPreview(material))
            {
                return;
            }

            Init();
            PreviewType previewType = GetPreviewType(material);
            if (target != null || previewType == PreviewType.Mesh)
            {
                int selectedMesh = m_SelectedMesh;
                m_TimeUpdate = InternalAPI.Internal_PreviewGUI_CycleButton(m_TimeUpdate, s_TimeIcons);
                m_SelectedMesh = InternalAPI.Internal_PreviewGUI_CycleButton(m_SelectedMesh, s_MeshIcons);
                if (selectedMesh != m_SelectedMesh)
                {
                    EditorPrefs.SetInt("DefaultMaterialPreviewMesh", m_SelectedMesh);
                }

                m_LightMode = InternalAPI.Internal_PreviewGUI_CycleButton(m_LightMode, s_LightIcons);
                if (DoReflectionProbePicker(out var buttonRect))
                {
                    PopupWindow.Show(buttonRect, m_ReflectionProbePicker);
                }
            }
        }

        private bool DoReflectionProbePicker(out Rect buttonRect)
        {
            buttonRect = GUILayoutUtility.GetRect(Styles.reflectionProbePickerIcon, InternalAPI.Internal_EditorStyles_ToolbarDropDownRight());
            if (EditorGUI.DropdownButton(buttonRect, Styles.reflectionProbePickerIcon, FocusType.Passive, InternalAPI.Internal_EditorStyles_ToolbarDropDownRight()))
            {
                return true;
            }

            return false;
        }

        private static bool SupportRenderingPreview(Material material)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                return false;
            }

            if (material == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(material);
            if (assetPath.EndsWith(".vfx", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public void DefaultPreviewGUI(Rect r, GUIStyle background)
        {
            Material material = base.target as Material;
            if (!SupportRenderingPreview(material))
            {
                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), "Material preview \nnot available");
                }

                return;
            }

            Init();
            PreviewType previewType = GetPreviewType(material);
            if (DoesPreviewAllowRotation(previewType))
            {
                m_PreviewDir = InternalAPI.Internal_PreviewGUI_Drag2D(m_PreviewDir, r);
            }

            if (Event.current.type == EventType.Repaint)
            {
                PreviewRenderUtility previewRendererUtility = GetPreviewRendererUtility();
                previewRendererUtility.BeginPreview(r, background);
                DoRenderPreview(previewRendererUtility, false);
                previewRendererUtility.EndAndDrawPreview(r);
            }
        }

        private static PreviewRenderUtility GetPreviewRendererUtility()
        {
            if (s_PreviewRenderUtility == null)
            {
                s_PreviewRenderUtility = new PreviewRenderUtility();
                EditorUtility.SetCameraAnimateMaterials(s_PreviewRenderUtility.camera, animate: true);
            }

            return s_PreviewRenderUtility;
        }

        private static bool DoesPreviewAllowRotation(PreviewType type)
        {
            return type != PreviewType.Plane;
        }

        private void DoRenderPreview(PreviewRenderUtility previewRenderUtility, bool overridePreviewMesh = false)
        {
            var previewRenderTexture = InternalAPI.Internal_PreviewUtility_RenderTexture(previewRenderUtility);
            if (previewRenderTexture.width > 0 && previewRenderTexture.height > 0)
            {
                Material mat = base.target as Material;
                PreviewType previewType = GetPreviewType(mat);
                previewRenderUtility.camera.transform.position = -Vector3.forward * 5f;
                previewRenderUtility.camera.transform.rotation = Quaternion.identity;
                if (m_LightMode == 0)
                {
                    previewRenderUtility.lights[0].intensity = 1f;
                    previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
                    previewRenderUtility.lights[1].intensity = 0f;
                }
                else
                {
                    previewRenderUtility.lights[0].intensity = 1f;
                    previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(50f, 50f, 0f);
                    previewRenderUtility.lights[1].intensity = 1f;
                }

                previewRenderUtility.ambientColor = new Color(0.2f, 0.2f, 0.2f, 0f);
                Quaternion quaternion = Quaternion.identity;
                if (DoesPreviewAllowRotation(previewType))
                {
                    quaternion = Quaternion.Euler(m_PreviewDir.y, 0f, 0f) * Quaternion.Euler(0f, m_PreviewDir.x, 0f);
                }

                Mesh mesh = (overridePreviewMesh ? s_Meshes[0] : s_Meshes[m_SelectedMesh]);
                switch (previewType)
                {
                    case PreviewType.Plane:
                        mesh = s_PlaneMesh;
                        break;
                    case PreviewType.Mesh:
                        previewRenderUtility.camera.transform.position = Quaternion.Inverse(quaternion) * previewRenderUtility.camera.transform.position;
                        previewRenderUtility.camera.transform.LookAt(Vector3.zero);
                        quaternion = Quaternion.identity;
                        break;
                    case PreviewType.Skybox:
                        mesh = null;
                        previewRenderUtility.camera.transform.rotation = Quaternion.Inverse(quaternion);
                        previewRenderUtility.camera.fieldOfView = 120f;
                        break;
                }

                if (mesh != null)
                {
                    previewRenderUtility.DrawMesh(mesh, Vector3.zero, quaternion, mat, 0, null, m_ReflectionProbePicker.Target, useLightProbe: false);
                }

                previewRenderUtility.Render(allowScriptableRenderPipeline: true);
                if (previewType == PreviewType.Skybox)
                {
                    InternalAPI.Internal_InternalEditorUtility_DrawSkyboxMaterial(mat, previewRenderUtility.camera);
                }
            }
        }

    }
}