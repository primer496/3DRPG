using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AIGraph.InternalBridge;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.AIGraph
{
    public class GameObjectPreviewRenderer : BasePreviewRenderer<GameObject>
    {
        internal class PreviewData : IDisposable
        {
            bool m_Disposed;

            public PreviewRenderUtility renderUtility;
            public GameObject gameObject { get; private set; }

            public string prefabAssetPath { get; private set; }

            public Bounds renderableBounds { get; private set; }

            public bool useStaticAssetPreview { get; set; }

            public PreviewData(UnityObject targetObject)
            {
                renderUtility = new PreviewRenderUtility();
                renderUtility.camera.fieldOfView = 30.0f;
                useStaticAssetPreview = IsPrefabFileTooLargeForInteractivePreview(targetObject);
                if (!useStaticAssetPreview)
                    UpdateGameObject(targetObject);
            }

            public void UpdateGameObject(UnityObject targetObject)
            {
                UnityObject.DestroyImmediate(gameObject);
                gameObject = InternalAPI.Internal_InstantiateForAnimatorPreview(targetObject);
                renderUtility.AddSingleGO(gameObject);
                renderableBounds = GetRenderableBounds(gameObject);
            }

            // Very large prefabs takes too long to instantiate for the interactive preview so we
            // fall back to the static preview for such prefabs
            bool IsPrefabFileTooLargeForInteractivePreview(UnityObject prefabObject)
            {
                string prefabAssetPath = AssetDatabase.GetAssetPath(prefabObject);
                if (string.IsNullOrEmpty(prefabAssetPath))
                    return false;

                string guidString = AssetDatabase.AssetPathToGUID(prefabAssetPath);
                if (string.IsNullOrEmpty(guidString))
                    return false;

                var artifactKey = new ArtifactKey(new GUID(guidString));
                var artifactID = AssetDatabaseExperimental.LookupArtifact(artifactKey);
                // The artifactID can be invalid if we are in the middle of an AssetDatabase.Refresh.
                if (!artifactID.isValid)
                    return false;
                AssetDatabaseExperimental.GetArtifactPaths(artifactID, out var paths);
                if (paths.Length != 1)
                {
                    Debug.LogError("Prefabs should just have one artifact");
                    return false;
                }

                string importedPrefabPath = Path.GetFullPath(paths[0]);
                if (!System.IO.File.Exists(importedPrefabPath))
                {
                    Debug.LogError("Could not find prefab artifact on disk");
                    return false;
                }

                long length = new System.IO.FileInfo(importedPrefabPath).Length;
                long fileSizeInKB = length / 1024;

                return fileSizeInKB > kMaxPreviewFileSizeInKB;
            }

            public void Dispose()
            {
                if (m_Disposed)
                    return;
                renderUtility.Cleanup();
                renderUtility = null;
                UnityObject.DestroyImmediate(gameObject);
                gameObject = null;
                m_Disposed = true;
            }
        }

        float zoomFactor = 3.8f;
        Vector2 m_PreviewDir;
        Rect m_PreviewRect;
        Vector2 m_StaticPreviewLabelSize = new Vector2(0, 0);
        const long kMaxPreviewFileSizeInKB = 32000;
        internal PreviewData m_PreviewInstances;
        Texture m_PreviewCache;

        public Texture2D previewCache
        {
            get
            {
                Texture tmpCache = m_PreviewCache;
                if (tmpCache == null)
                {
                    var previewData = GetPreviewData(); 
                    var previewUtility = previewData.renderUtility;

                    if (previewData.useStaticAssetPreview || !ShaderUtil.hardwareSupportsRectRenderTexture)
                    {
                        if (target == null)
                            return null;

                        Texture2D icon = AssetPreview.GetAssetPreview(target);
                        if (!icon)
                        {
                            // We have a static preview it just hasn't been loaded yet. Repaint until we have it loaded.
                            if (AssetPreview.IsLoadingAssetPreview(target.GetInstanceID()))
                            {
                                while (!icon)
                                {
                                    icon = AssetPreview.GetAssetPreview(target);
                                }

                                return icon;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        return icon;
                    }
                    
                    previewUtility.BeginPreview(m_PreviewRect, GUIStyle.none);
                    DoRenderPreview(previewData);
                    var previewTex = previewUtility.EndPreview();

                    var copy = new RenderTexture(InternalAPI.Internal_PreviewUtility_RenderTexture(previewUtility));
                    var previous = RenderTexture.active;
                    Graphics.Blit(InternalAPI.Internal_PreviewUtility_RenderTexture(previewUtility), copy);
                    RenderTexture.active = previous;
                    tmpCache = copy;

                    previewData.Dispose();
                }

                int w = tmpCache.width, h = tmpCache.height;
                var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);

                Graphics.Blit(tmpCache, rt);
 
                var prev = RenderTexture.active;
                RenderTexture.active = rt;

                var tex = new Texture2D(w, h);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();

                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                return tex;
            }
        }

        public static GUIContent staticPreviewContent = EditorGUIUtility.TrTextContent("Static Preview", "This asset is greater than 8MB so, by default, the Asset Preview displays a static preview.\nTo view the asset interactively, click the Asset Preview.");

        internal PreviewData GetPreviewData()
        {
            if (m_PreviewInstances == null)
            {
                m_PreviewInstances = new PreviewData(target);
            }

            return m_PreviewInstances;
        }

        static readonly List<Renderer> s_RendererComponentsList = new List<Renderer>();

        static bool HasRenderableParts(GameObject go)
        {
            if (!go)
                return false;
            go.GetComponentsInChildren(s_RendererComponentsList);
            return s_RendererComponentsList.Where(IsRendererUsableForPreview).ToList().Count > 0;
        }

        void InitPreviewDir(GameObject target)
        {
            if (target == null) return;

            if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D)
                m_PreviewDir = new Vector2(0, 0);
            else
            {
                m_PreviewDir = new Vector2(120, -20);

                UnityObject importedObject = PrefabUtility.IsPartOfVariantPrefab(target)
                    ? PrefabUtility.GetCorrespondingObjectFromSource(target) as GameObject
                    : target;

                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(importedObject)) as ModelImporter;
                if (importer && importer.bakeAxisConversion)
                {
                    m_PreviewDir += new Vector2(180, 0);
                }
            }
        }

        public override void Initialize(UnityObject target, SDNode node)
        {
            base.Initialize(target, node);

            InitPreviewDir(target as GameObject);
        }

        public override void Cleanup()
        {
            m_PreviewInstances?.Dispose();
            ClearPreviewCache();
            m_PreviewInstances = null;
        }

        public override void Update(UnityObject target)
        {
            base.Update(target);
            InitPreviewDir(target as GameObject);

            Cleanup();
        }

        public override bool HasPreviewGUI()
        {
            if (target != null)
            {
                return HasRenderableParts(target as GameObject);
            }

            return false;
        }

        public override string GetPreviewTitle()
        {
            return GetPreviewTitleStatic(target).text;
        }

        public override void OnPreviewSettings()
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
                return;
            GUI.enabled = true;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var previewData = GetPreviewData();

            if (previewData.useStaticAssetPreview && GUI.Button(rect, GUIContent.none))
            {
                previewData.useStaticAssetPreview = false;
                previewData.UpdateGameObject(target);
            }

            if (previewData.useStaticAssetPreview || !ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                DrawAssetPreviewTexture(rect);
                return;
            }

            var direction = InternalAPI.Internal_PreviewGUI_Drag2D(m_PreviewDir, rect);
            if (direction != m_PreviewDir)
            {
                // None of the preview are valid since the camera position has changed.
                ClearPreviewCache();
                m_PreviewDir = direction;
            }


            if (Event.current.type == EventType.ScrollWheel && PreviewZoom(rect, Event.current))
            {
                ClearPreviewCache();
            }

            if (Event.current.type != EventType.Repaint)
                return;

            if (m_PreviewRect != rect)
            {
                ClearPreviewCache();
                m_PreviewRect = rect;
            }

            var previewUtility = GetPreviewData().renderUtility;
            if (previewUtility == null)
                return;

            if (m_PreviewCache != null)
            {
                GUI.DrawTexture(rect, m_PreviewCache, ScaleMode.StretchToFill, alphaBlend: false);
            }
            else
            {
                previewUtility.BeginPreview(rect, background);
                DoRenderPreview(previewData);
                previewUtility.EndAndDrawPreview(rect);

                var copy = new RenderTexture(InternalAPI.Internal_PreviewUtility_RenderTexture(previewUtility));
                var previous = RenderTexture.active;
                Graphics.Blit(InternalAPI.Internal_PreviewUtility_RenderTexture(previewUtility), copy);
                RenderTexture.active = previous;
                m_PreviewCache = copy;
            }
        }

        private bool PreviewZoom(Rect rect, Event evt)
        {

            float num = (0f - HandleUtility.niceMouseDeltaZoom * 0.5f) * 0.05f;
            var newZoomFactor = zoomFactor + zoomFactor * num;
            if (newZoomFactor < 1.3f)
                return false;
            zoomFactor = newZoomFactor;
            evt.Use();
            return true;
        }

        private void DoRenderPreview(PreviewData previewData)
        {
            var bounds = previewData.renderableBounds;
            float halfSize = Mathf.Max(bounds.extents.magnitude, 0.0001f);
            float distance = halfSize * zoomFactor;

            Quaternion rot = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0);
            Vector3 pos = bounds.center - rot * (Vector3.forward * distance);

            previewData.renderUtility.camera.transform.position = pos;
            previewData.renderUtility.camera.transform.rotation = rot;
            previewData.renderUtility.camera.nearClipPlane = distance - halfSize * 1.1f;
            previewData.renderUtility.camera.farClipPlane = distance + halfSize * 1.1f;

            previewData.renderUtility.lights[0].intensity = .7f;
            previewData.renderUtility.lights[0].transform.rotation = rot * Quaternion.Euler(40f, 40f, 0);
            previewData.renderUtility.lights[1].intensity = .7f;
            previewData.renderUtility.lights[1].transform.rotation = rot * Quaternion.Euler(340, 218, 177);

            previewData.renderUtility.ambientColor = new Color(.1f, .1f, .1f, 0);

            previewData.renderUtility.Render(true);
        }

        static bool IsRendererUsableForPreview(Renderer r)
        {
            switch (r)
            {
                case MeshRenderer mr:
                    mr.gameObject.TryGetComponent<MeshFilter>(out var mf);
                    if (mf == null || mf.sharedMesh == null)
                        return false;
                    break;
                case SkinnedMeshRenderer skin:
                    if (skin.sharedMesh == null)
                        return false;
                    break;
                case SpriteRenderer sprite:
                    if (sprite.sprite == null)
                        return false;
                    break;
                case BillboardRenderer billboard:
                    if (billboard.billboard == null || billboard.sharedMaterial == null)
                        return false;
                    break;
            }
            return true;
        }

        static Bounds GetRenderableBounds(GameObject go)
        {
            var b = new Bounds();
            if (!go)
                return b;
            go.GetComponentsInChildren(s_RendererComponentsList);
            foreach (var r in s_RendererComponentsList)
            {
                if (!IsRendererUsableForPreview(r))
                    continue;
                if (b.extents == Vector3.zero)
                    b = r.bounds;
                else
                    b.Encapsulate(r.bounds);
            }

            return b;
        }

        void ClearPreviewCache()
        {
            if (m_PreviewCache != null)
            {
                UnityObject.DestroyImmediate(m_PreviewCache);
                m_PreviewCache = null;
            }
        }
        void DrawAssetPreviewTexture(Rect rect)
        {
            Texture2D icon = AssetPreview.GetAssetPreview(target);
            if (!icon)
            {
                // We have a static preview it just hasn't been loaded yet. Repaint until we have it loaded.
                if (AssetPreview.IsLoadingAssetPreview(target.GetInstanceID()))
                {
                    while (!icon)
                    {
                        icon = AssetPreview.GetAssetPreview(target);
                    }
                }
                else
                {
                    return;
                }
            }

            var scaleMode = ScaleMode.ScaleToFit;
            GUI.DrawTexture(rect, icon, scaleMode);

            if (m_StaticPreviewLabelSize.x == 0.0f && m_StaticPreviewLabelSize.y == 0.0f)
                m_StaticPreviewLabelSize = GUI.skin.label.CalcSize(staticPreviewContent);

            // Only render overlay text if there is space enough
            if (rect.width >= m_StaticPreviewLabelSize.x && rect.height >= m_StaticPreviewLabelSize.y + GUI.skin.label.padding.vertical)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Label(new Rect(rect.x, rect.yMax - (m_StaticPreviewLabelSize.y + GUI.skin.label.padding.vertical), rect.width, m_StaticPreviewLabelSize.y), staticPreviewContent, EditorStyles.centeredGreyMiniLabel);
                }
            }
        }
    }
}