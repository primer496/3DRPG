using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace UnityEditor.AIGraph
{
    public class VideoClipPreviewRenderer : BasePreviewRenderer<VideoClip>
    {
        private VideoClip m_PlayingClip;
        private GUID m_PreviewID;
        private bool m_UseAssetPreview = true; 
        static readonly GUID kEmptyGUID;
        private Texture m_Texture;
        Vector2 m_Position = Vector2.zero;

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
            return GetPreviewTitleStatic(target).text + " (Press Preview to Play Clip)";
        }
        private void PlayPreview()
        {
            m_PreviewID = VideoUtil.StartPreview(m_PlayingClip);
            VideoUtil.PlayPreview(m_PreviewID, true);
        }

        private void StopPreview()
        {
            m_UseAssetPreview = true;
            if (!m_PreviewID.Empty())
                VideoUtil.StopPreview(m_PreviewID);
            m_PlayingClip = null;
            m_PreviewID = kEmptyGUID;
        }

        Texture GetAssetPreviewTexture()
        {
            Texture tex = null;
            bool isLoadingAssetPreview = AssetPreview.IsLoadingAssetPreview(target.GetInstanceID());
            tex = AssetPreview.GetAssetPreview(target);
            if (!tex)
            {
                tex = AssetPreview.GetMiniThumbnail(target);
            }
            return tex;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            VideoClip clip = target as VideoClip;

            Event evt = Event.current;
            if (evt.type != EventType.Repaint &&
                evt.type != EventType.Layout &&
                evt.type != EventType.Used)
            {
                switch (evt.type)
                {
                    case EventType.MouseDown:
                        {
                            if (rect.Contains(evt.mousePosition))
                            {
                                if (m_PlayingClip != null)
                                {
                                    if (m_PreviewID.Empty() || !VideoUtil.IsPreviewPlaying(m_PreviewID))
                                    {
                                        PlayPreview();
                                    }
                                    else
                                    {
                                        StopPreview();
                                    }
                                }
                                evt.Use();
                            }
                        }
                        break;
                }
                return;
            }

            if (clip != m_PlayingClip)
            {
                StopPreview();
                m_PlayingClip = clip;
            }

            Texture image = null;

            if (!m_PreviewID.Empty() && VideoUtil.IsPreviewPlaying(m_PreviewID))
            {
                image = VideoUtil.GetPreviewTexture(m_PreviewID);
                if (image != null && m_UseAssetPreview)
                    m_UseAssetPreview = false;
            }
            else
                image = GetAssetPreviewTexture();

            if (image != null && image.width != 0 && image.height != 0)
                m_Texture = image;

            if (!m_Texture)
                return;

            if (Event.current.type == EventType.Repaint)
                background.Draw(rect, false, false, false, false);

            float previewWidth = m_Texture.width;
            float previewHeight = m_Texture.height;

            if (m_PlayingClip.pixelAspectRatioDenominator > 0)
            {
                float pixelAspectRatio = (float)m_PlayingClip.pixelAspectRatioNumerator /
                    (float)m_PlayingClip.pixelAspectRatioDenominator;

                if (pixelAspectRatio > 1.0F)
                    previewWidth *= pixelAspectRatio;
                else
                    previewHeight /= pixelAspectRatio;
            }

            float zoomLevel = 1.0f;

            if ((rect.width / previewWidth * previewHeight) > rect.height)
                zoomLevel = rect.height / previewHeight;
            else
                zoomLevel = rect.width / previewWidth;

            zoomLevel = Mathf.Clamp01(zoomLevel);

            Rect wantedRect = !m_UseAssetPreview ? new Rect(rect.x, rect.y, previewWidth * zoomLevel, m_Texture.height * zoomLevel) : rect;
            PreviewGUI.BeginScrollView(
                rect, m_Position, wantedRect, "PreHorizontalScrollbar", "PreHorizontalScrollbarThumb");

            if (!m_UseAssetPreview)
                EditorGUI.DrawTextureTransparent(wantedRect, m_Texture, ScaleMode.StretchToFill);
            else
                GUI.DrawTexture(wantedRect, m_Texture, ScaleMode.ScaleToFit);

            m_Position = PreviewGUI.EndScrollView();

            if (!m_PreviewID.Empty() &&
                VideoUtil.IsPreviewPlaying(m_PreviewID) &&
                Event.current.type == EventType.Repaint && GUIView.current != null)
                GUIView.current.Repaint();
        }
        public override void OnPreviewSettings()
        {
        }
    }
}
