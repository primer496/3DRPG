using System.Collections.Generic;
using UnityEditor.AIGraph.InternalBridge;
using UnityEngine.AIGraph;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityEditor.AIGraph
{
    public class Texture2DPreviewRenderer : BasePreviewRenderer<Texture2D>
    {
        class Styles
        {
            public GUIContent smallZoom, largeZoom;
            public GUIStyle toolbarButton, previewSlider, previewSliderThumb, previewLabel, mipLevelLabel;

            public readonly GUIContent[] previewButtonContents =
            {
                EditorGUIUtility.TrIconContent("PreTexRGB"),
                EditorGUIUtility.TrIconContent("PreTexR"),
                EditorGUIUtility.TrIconContent("PreTexG"),
                EditorGUIUtility.TrIconContent("PreTexB"),
                EditorGUIUtility.TrIconContent("PreTexA")
            };

            public readonly GUIContent wrapModeLabel = EditorGUIUtility.TrTextContent("Wrap Mode");
            public readonly GUIContent wrapU = EditorGUIUtility.TrTextContent("U axis");
            public readonly GUIContent wrapV = EditorGUIUtility.TrTextContent("V axis");
            public readonly GUIContent wrapW = EditorGUIUtility.TrTextContent("W axis");

            public readonly GUIContent[] wrapModeContents =
            {
                EditorGUIUtility.TrTextContent("Repeat"),
                EditorGUIUtility.TrTextContent("Clamp"),
                EditorGUIUtility.TrTextContent("Mirror"),
                EditorGUIUtility.TrTextContent("Mirror Once"),
                EditorGUIUtility.TrTextContent("Per-axis")
            };
            public readonly int[] wrapModeValues =
            {
                (int)TextureWrapMode.Repeat,
                (int)TextureWrapMode.Clamp,
                (int)TextureWrapMode.Mirror,
                (int)TextureWrapMode.MirrorOnce,
                -1
            };

            public Styles()
            {
                smallZoom = EditorGUIUtility.IconContent("PreTextureMipMapLow");
                largeZoom = EditorGUIUtility.IconContent("PreTextureMipMapHigh");

                toolbarButton = "toolbarbutton";
                previewSlider = "preSlider";
                previewSliderThumb = "preSliderThumb";
                previewLabel = "toolbarLabel";

                mipLevelLabel = "PreOverlayLabel";
                mipLevelLabel.alignment = TextAnchor.UpperCenter;
                mipLevelLabel.padding.top = 5;
            }
        }
        static Styles s_Styles;

        internal enum PreviewMode
        {
            RGB,
            R,
            G,
            B,
            A,
        }

        internal PreviewMode m_PreviewMode = PreviewMode.RGB;
        private float m_ExposureSliderValue = 0.0f;
        private float m_ExposureSliderMax = 16f; // this value can be altered by the user
        //[SerializeField]
        //float m_MipLevel = 0;
        [SerializeField]
        protected Vector2 m_Pos;

        public override void Initialize(UnityEngine.Object target, SDNode node)
        {
            base.Initialize(target, node);
        }

        public override void Cleanup()
        {
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

        static bool IsDefaultImportedTexture(Texture t)
        {
            if (t == null) return false;
            if (!AssetDatabase.Contains(t)) return false;

            var path = AssetDatabase.GetAssetPath(t);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            return importer != null && importer.textureType == TextureImporterType.Default;
        }

        public override void OnPreviewSettings()
        {
            Texture t = target as Texture;

            bool alphaOnly = true;
            bool hasAlpha = false;
            bool needsExposureControl = false;
            int mipCount = 1;

            if (t == null) // texture might have disappeared while we're showing this in a preview popup
                return;

            mipCount = Mathf.Max(mipCount, InternalAPI.Interal_TextureUtil_GetMipmapCount(t));

            if (!GraphicsFormatUtility.IsAlphaOnlyFormat(t.graphicsFormat))
                alphaOnly = false;

            if (GraphicsFormatUtility.HasAlphaChannel(t.graphicsFormat))
            {
                if (InternalAPI.Interal_TextureUtil_IsUsageModeDefault(t)) // all other texture usage modes don't displayable alpha
                    hasAlpha = true;
            }

            // 3D texture previewer doesn't support an exposure value.
            if (t.dimension != TextureDimension.Tex3D && NeedsExposureControl(t))
                needsExposureControl = true;
            

            if (needsExposureControl)
            {
                OnExposureSlider();
            }

            if (s_Styles == null)
                s_Styles = new Styles();

            List<PreviewMode> previewCandidates = new List<PreviewMode>(5);
            previewCandidates.Add(PreviewMode.RGB);
            previewCandidates.Add(PreviewMode.R);
            previewCandidates.Add(PreviewMode.G);
            previewCandidates.Add(PreviewMode.B);
            previewCandidates.Add(PreviewMode.A);

            if (alphaOnly)
            {
                previewCandidates.Clear();
                previewCandidates.Add(PreviewMode.A);
                m_PreviewMode = PreviewMode.A;
            }
            else if (!hasAlpha)
            {
                previewCandidates.Remove(PreviewMode.A);
            }

            if (previewCandidates.Count > 1 && t != null && !IsNormalMap(t))
            {
                int selectedIndex = previewCandidates.IndexOf(m_PreviewMode);
                if (selectedIndex == -1)
                    selectedIndex = 0;

                if (previewCandidates.Contains(PreviewMode.RGB))
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.RGB, s_Styles.previewButtonContents[0], s_Styles.toolbarButton)
                        ? PreviewMode.RGB
                        : m_PreviewMode;
                if (previewCandidates.Contains(PreviewMode.R))
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.R, s_Styles.previewButtonContents[1], s_Styles.toolbarButton)
                        ? PreviewMode.R
                        : m_PreviewMode;
                if (previewCandidates.Contains(PreviewMode.G))
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.G, s_Styles.previewButtonContents[2], s_Styles.toolbarButton)
                        ? PreviewMode.G
                        : m_PreviewMode;
                if (previewCandidates.Contains(PreviewMode.B))
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.B, s_Styles.previewButtonContents[3], s_Styles.toolbarButton)
                        ? PreviewMode.B
                        : m_PreviewMode;
                if (previewCandidates.Contains(PreviewMode.A))
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.A, s_Styles.previewButtonContents[4], s_Styles.toolbarButton)
                        ? PreviewMode.A
                        : m_PreviewMode;
            }

            //if (mipCount > 1)
            //{
            //    int mipmapLimit = GetMipmapLimit(target as Texture);
            //    GUILayout.Box(s_Styles.smallZoom, s_Styles.previewLabel);
            //    GUI.changed = false;

            //    int leftValue = mipCount - mipmapLimit - 1;
            //    if (m_MipLevel > leftValue)
            //    {
            //        // Left value can change depending on the mipmap limit. Cap slider value appropriately.
            //        m_MipLevel = leftValue;
            //    }
            //    m_MipLevel = Mathf.Round(GUILayout.HorizontalSlider(m_MipLevel, leftValue, 0, s_Styles.previewSlider, s_Styles.previewSliderThumb, GUILayout.MaxWidth(64)));

            //    //For now, we don't have mipmaps smaller than the tile size when using VT.
            //    if (InternalAPI.Interal_EditorGUI_UseVTMaterial(t))
            //    {
            //        int numMipsOfTile = (int)Mathf.Log(VirtualTexturing.EditorHelpers.tileSize, 2) + 1;
            //        m_MipLevel = Mathf.Min(m_MipLevel, Mathf.Max(mipCount - numMipsOfTile, 0));
            //    }

            //    GUILayout.Box(s_Styles.largeZoom, s_Styles.previewLabel);
            //}
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (Event.current.type == EventType.Repaint)
                background.Draw(rect, false, false, false, false);

            // show texture
            Texture t = target as Texture;
            if (t == null) // texture might be gone by now, in case this code is used for floating texture preview
                return;

            GraphicsFormat format = t.graphicsFormat;
            if (!(GraphicsFormatUtility.IsIEEE754Format(format) || GraphicsFormatUtility.IsNormFormat(format)))
            {
                EditorGUI.HelpBox(rect, "This preview only supports floating point or normalized formats.", MessageType.Warning);
                return;
            }

            // Render target must be created before we can display it (case 491797)
            RenderTexture rt = t as RenderTexture;
            if (rt != null)
            {
                if (rt.Create() == false)
                {
                    return;
                }
            }

            // target can report zero sizes in some cases just after a parameter change;
            // guard against that.
            int texWidth = Mathf.Max(t.width, 1);
            int texHeight = Mathf.Max(t.height, 1);

            float mipLevel = 0;// GetMipLevelForRendering();
            float zoomLevel = Mathf.Min(Mathf.Min(rect.width / texWidth, rect.height / texHeight), 1);
            Rect wantedRect = new Rect(rect.x, rect.y, texWidth * zoomLevel, texHeight * zoomLevel);
            InternalAPI.Internal_PreviewGUI_BeginScrollView(rect, m_Pos, wantedRect, "PreHorizontalScrollbar", "PreHorizontalScrollbarThumb");
            FilterMode oldFilter = t.filterMode;
            InternalAPI.Internal_TextureUtil_SetFilterModeNoDirty(t, FilterMode.Point);
            Texture2D t2d = t as Texture2D;
            ColorWriteMask colorWriteMask = ColorWriteMask.All;

            switch (m_PreviewMode)
            {
                case PreviewMode.R:
                    colorWriteMask = ColorWriteMask.Red | ColorWriteMask.Alpha;
                    break;
                case PreviewMode.G:
                    colorWriteMask = ColorWriteMask.Green | ColorWriteMask.Alpha;
                    break;
                case PreviewMode.B:
                    colorWriteMask = ColorWriteMask.Blue | ColorWriteMask.Alpha;
                    break;
            }

            if (m_PreviewMode == PreviewMode.A)
            {
                EditorGUI.DrawTextureAlpha(wantedRect, t, ScaleMode.StretchToFill, 0, mipLevel);
            }
            else
            {
                if (t2d != null && t2d.alphaIsTransparency)
                    EditorGUI.DrawTextureTransparent(wantedRect, t, ScaleMode.StretchToFill, 0, mipLevel,
                        colorWriteMask, GetExposureValueForTexture(t));
                else
                {
                    //if (IsDefaultImportedTexture(t))
                    //{
                    EditorGUI.DrawTextureTransparent(wantedRect, t, ScaleMode.StretchToFill);
                    //}
    //                else
    //                {
    //                    EditorGUI.DrawPreviewTexture(wantedRect, t, null, ScaleMode.StretchToFill, 0, mipLevel,
    //colorWriteMask, GetExposureValueForTexture(t));
    //                }
                }

            }

            // TODO: Less hacky way to prevent sprite rects to not appear in smaller previews like icons.
            if ((wantedRect.width > 32 && wantedRect.height > 32) && Event.current.type == EventType.Repaint)
            {
                string path = AssetDatabase.GetAssetPath(t);
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                SpriteMetaData[] spritesheet = textureImporter != null ? InternalAPI.Internal_TextureImporter_GetSpriteMetaDatas(textureImporter) : null;

                if (spritesheet != null && textureImporter.spriteImportMode == SpriteImportMode.Multiple)
                {
                    Rect screenRect = new Rect();
                    Rect sourceRect = new Rect();
                    CalculateScaledTextureRects(wantedRect, ScaleMode.StretchToFill, (float)t.width / (float)t.height, ref screenRect, ref sourceRect);

                    int origWidth = t.width;
                    int origHeight = t.height;

                    InternalAPI.Internal_TextureImporter_GetWidthAndHeight(textureImporter, ref origWidth, ref origHeight);
                    float definitionScale = (float)t.width / (float)origWidth;

                    InternalAPI.Internal_HandleUtility_ApplyWireMaterial();
                    GL.PushMatrix();
                    GL.MultMatrix(Handles.matrix);
                    GL.Begin(GL.LINES);
                    GL.Color(new Color(1f, 1f, 1f, 0.5f));
                    foreach (SpriteMetaData sprite in spritesheet)
                    {
                        Rect spriteRect = sprite.rect;
                        Rect spriteScreenRect = new Rect();
                        spriteScreenRect.xMin = screenRect.xMin + screenRect.width * (spriteRect.xMin / t.width * definitionScale);
                        spriteScreenRect.xMax = screenRect.xMin + screenRect.width * (spriteRect.xMax / t.width * definitionScale);
                        spriteScreenRect.yMin = screenRect.yMin + screenRect.height * (1f - spriteRect.yMin / t.height * definitionScale);
                        spriteScreenRect.yMax = screenRect.yMin + screenRect.height * (1f - spriteRect.yMax / t.height * definitionScale);
                        DrawRect(spriteScreenRect);
                    }
                    GL.End();
                    GL.PopMatrix();
                }
            }

            InternalAPI.Internal_TextureUtil_SetFilterModeNoDirty(t, oldFilter);

            //int mipmapLimit = GetMipmapLimit(target as Texture);
            //int cpuMipLevel = Mathf.Min(TextureUtil.GetMipmapCount(target as Texture) - 1, (int)mipLevel + mipmapLimit);
            m_Pos = InternalAPI.Internal_PreviewGUI_EndScrollView();
            //if (cpuMipLevel != 0)
            //{
            //    GUIContent mipLevelTextContent = new GUIContent((cpuMipLevel != mipLevel)
            //            ? string.Format("Mip {0}\nMip {1} on GPU (Texture Limit)", cpuMipLevel, mipLevel)
            //            : string.Format("Mip {0}", mipLevel));
            //    Vector2 size = s_Styles.mipLevelLabel.CalcSize(mipLevelTextContent);
            //    if (size.x <= rect.width)
            //    {
            //        EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, size.y), mipLevelTextContent, s_Styles.mipLevelLabel);
            //    }
            //}
        }

        private bool NeedsExposureControl(Texture t)
        {
            return GraphicsFormatUtility.IsHDRFormat(t.graphicsFormat) || InternalAPI.Interal_TextureUtil_IsUsageModeRGB(t) || InternalAPI.Interal_TextureUtil_IsUsageModeDoubleLDR(t);
        }

        private void OnExposureSlider()
        {
            if (s_Styles == null)
                s_Styles = new Styles();
            m_ExposureSliderValue = InternalAPI.Internal_EditorGUIInternal_ExposureSlider(m_ExposureSliderValue, ref m_ExposureSliderMax, s_Styles.previewSlider);
        }

        private static bool IsNormalMap(Texture t)
        {
            return InternalAPI.Interal_TextureUtil_IsUsageModeNormalMap(t);
        }

        private float GetExposureValueForTexture(Texture t)
        {
            if (NeedsExposureControl(t))
            {
                return m_ExposureSliderValue;
            }
            return 0.0f;
        }

        private void DrawRect(Rect rect)
        {
            GL.Vertex(new Vector3(rect.xMin, rect.yMin, 0f));
            GL.Vertex(new Vector3(rect.xMax, rect.yMin, 0f));
            GL.Vertex(new Vector3(rect.xMax, rect.yMin, 0f));
            GL.Vertex(new Vector3(rect.xMax, rect.yMax, 0f));
            GL.Vertex(new Vector3(rect.xMax, rect.yMax, 0f));
            GL.Vertex(new Vector3(rect.xMin, rect.yMax, 0f));
            GL.Vertex(new Vector3(rect.xMin, rect.yMax, 0f));
            GL.Vertex(new Vector3(rect.xMin, rect.yMin, 0f));
        }

        // Calculate screenrect and sourcerect for different scalemodes
        private static bool CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, ref Rect outScreenRect, ref Rect outSourceRect)
        {
            float destAspect = position.width / position.height;
            bool ret = false;

            switch (scaleMode)
            {
                case ScaleMode.StretchToFill:
                    outScreenRect = position;
                    outSourceRect = new Rect(0, 0, 1, 1);
                    ret = true;
                    break;
                case ScaleMode.ScaleAndCrop:
                    if (destAspect > imageAspect)
                    {
                        float stretch = imageAspect / destAspect;
                        outScreenRect = position;
                        outSourceRect = new Rect(0, (1 - stretch) * .5f, 1, stretch);
                        ret = true;
                    }
                    else
                    {
                        float stretch = destAspect / imageAspect;
                        outScreenRect = position;
                        outSourceRect = new Rect(.5f - stretch * .5f, 0, stretch, 1);
                        ret = true;
                    }
                    break;
                case ScaleMode.ScaleToFit:
                    if (destAspect > imageAspect)
                    {
                        float stretch = imageAspect / destAspect;
                        outScreenRect = new Rect(position.xMin + position.width * (1.0f - stretch) * .5f, position.yMin, stretch * position.width, position.height);
                        outSourceRect = new Rect(0, 0, 1, 1);
                        ret = true;
                    }
                    else
                    {
                        float stretch = destAspect / imageAspect;
                        outScreenRect = new Rect(position.xMin, position.yMin + position.height * (1.0f - stretch) * .5f, position.width, stretch * position.height);
                        outSourceRect = new Rect(0, 0, 1, 1);
                        ret = true;
                    }
                    break;
            }

            return ret;
        }

        //private int GetMipmapLimit(Texture t)
        //{
        //    if (t is Texture2D)
        //    {
        //        return (t as Texture2D).activeMipmapLimit;
        //    }
        //    return 0;
        //}

        //public float GetMipLevelForRendering()
        //{
        //    if (target == null)
        //        return 0.0f;

        //    return Mathf.Min(m_MipLevel, InternalAPI.Interal_TextureUtil_GetMipmapCount(target as Texture) - 1);
        //}
    }
}