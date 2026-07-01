using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;
using UnityEngine.Video;
using static UnityEngine.GraphicsBuffer;

namespace UnityEditor.AIGraph
{
    public class AudioClipPreviewRenderer : BasePreviewRenderer<AudioClip>
    {
        static GUIStyle s_PreButton;
        static bool s_AutoPlay;
        static bool s_Loop;
        static bool s_PlayFirst;
        static GUIContent s_PlayIcon;
        static GUIContent s_AutoPlayIcon;
        static GUIContent s_LoopIcon;
        static private string s_PreviewDisabledMessage = "AudioClip preview not available when Unity Audio is disabled in Project Settings";
        static private string s_TrPreviewDisabledMessage = L10n.Tr(s_PreviewDisabledMessage);

        static Texture2D s_DefaultIcon;
        static AudioClipPreviewRenderer s_PlayingInstance;
        static Rect s_WantedRect;
        Vector2 m_Position = Vector2.zero;

        private bool playing => s_PlayingInstance == this && m_Clip != null && AudioUtil.IsPreviewClipPlaying();
        private AudioClip m_Clip;

        private static class AudioSettingsInternalCached
        {
            private static readonly FieldInfo s_field;
            private static readonly PropertyInfo s_prop;

            static AudioSettingsInternalCached()
            {
                var t = typeof(AudioSettings);
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

                s_field = t.GetField("unityAudioDisabled", flags);
                if (s_field == null)
                    s_prop = t.GetProperty("unityAudioDisabled", flags);
            }

            public static bool IsAvailable =>
                (s_field != null && s_field.FieldType == typeof(bool)) ||
                (s_prop != null && s_prop.PropertyType == typeof(bool) && s_prop.CanRead);

            public static bool TryGet(out bool disabled)
            {
                disabled = false;

                if (s_field != null && s_field.FieldType == typeof(bool))
                {
                    disabled = (bool)s_field.GetValue(null);
                    return true;
                }

                if (s_prop != null && s_prop.PropertyType == typeof(bool) && s_prop.CanRead)
                {
                    disabled = (bool)s_prop.GetValue(null);
                    return true;
                }

                return false;
            }
        }

        public override void Initialize(UnityEngine.Object target, SDNode node)
        {
            base.Initialize(target, node);
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

        void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            AudioUtil.StopAllPreviewClips();
            AudioUtil.PlayPreviewClip(clip, startSample, loop);
            m_Clip = clip;
            s_PlayingInstance = this;
        }

        // Passing in clip and importer separately as we're not completely done with the asset setup at the time we're asked to generate the preview.
        private void DoRenderPreview(bool setMaterial, AudioClip clip, AudioImporter audioImporter, Rect wantedRect, float scaleFactor)
        {
            scaleFactor *= 0.95f; // Reduce amplitude slightly to make highly compressed signals fit.
            float[] minMaxData = (audioImporter == null) ? null : AudioUtil.GetMinMaxData(audioImporter);
            int numChannels = clip.channels;
            int numSamples = (minMaxData == null) ? 0 : (minMaxData.Length / (2 * numChannels));
            float h = (float)wantedRect.height / (float)numChannels;
            for (int channel = 0; channel < numChannels; channel++)
            {
                Rect channelRect = new Rect(wantedRect.x, wantedRect.y + h * channel, wantedRect.width, h);
                Color curveColor = new Color(1.0f, 140.0f / 255.0f, 0.0f, 1.0f);

                AudioCurveRendering.AudioMinMaxCurveAndColorEvaluator dlg = delegate (float x, out Color col, out float minValue, out float maxValue)
                {
                    col = curveColor;
                    if (numSamples <= 0)
                    {
                        minValue = 0.0f;
                        maxValue = 0.0f;
                    }
                    else
                    {
                        float p = Mathf.Clamp(x * (numSamples - 2), 0.0f, numSamples - 2);
                        int i = (int)Mathf.Floor(p);
                        int offset1 = (i * numChannels + channel) * 2;
                        int offset2 = offset1 + numChannels * 2;
                        minValue = Mathf.Min(minMaxData[offset1 + 1], minMaxData[offset2 + 1]) * scaleFactor;
                        maxValue = Mathf.Max(minMaxData[offset1 + 0], minMaxData[offset2 + 0]) * scaleFactor;
                        if (minValue > maxValue) { float tmp = minValue; minValue = maxValue; maxValue = tmp; }
                    }
                };

                if (setMaterial)
                    AudioCurveRendering.DrawMinMaxFilledCurve(channelRect, dlg);
                else
                    AudioCurveRendering.DrawMinMaxFilledCurveInternal(channelRect, dlg);
            }
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (s_DefaultIcon == null) Init();

            AudioClip clip = target as AudioClip;

            Event evt = Event.current;
            if (evt.type != EventType.Repaint && evt.type != EventType.Layout && evt.type != EventType.Used)
            {
                switch (evt.type)
                {
                    case EventType.MouseDrag:
                    case EventType.MouseDown:
                        {
                            if (rect.Contains(evt.mousePosition))
                            {
                                var startSample = (int)(evt.mousePosition.x * (AudioUtil.GetSampleCount(clip) / (int)rect.width));
                                if (!AudioUtil.IsPreviewClipPlaying() || clip != m_Clip)
                                    PlayClip(clip, startSample, s_Loop);
                                else
                                    AudioUtil.SetPreviewClipSamplePosition(clip, startSample);
                                evt.Use();
                            }
                        }
                        break;
                }
                return;
            }

            if (Event.current.type == EventType.Repaint)
                background.Draw(rect, false, false, false, false);

            int c = AudioUtil.GetChannelCount(clip);
            s_WantedRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            float sec2px = ((float)s_WantedRect.width / clip.length);

            bool previewAble = AudioUtil.HasPreview(clip) || !(AudioUtil.IsTrackerFile(clip));
            if (!previewAble)
            {
                float labelY = (rect.height > 150) ? rect.y + (rect.height / 2) - 10 : rect.y + (rect.height / 2) - 25;
                if (rect.width > 64)
                {
                    if (AudioUtil.IsTrackerFile(clip))
                    {
                        EditorGUI.DropShadowLabel(new Rect(rect.x, labelY, rect.width, 20), string.Format("Module file with " + AudioUtil.GetMusicChannelCount(clip) + " channels."));
                    }
                    else
                        EditorGUI.DropShadowLabel(new Rect(rect.x, labelY, rect.width, 20), "Can not show PCM data for this file");
                }

                if (m_Clip == clip && playing)
                {
                    float t = AudioUtil.GetPreviewClipPosition();

                    System.TimeSpan ts = new System.TimeSpan(0, 0, 0, 0, (int)(t * 1000.0f));

                    EditorGUI.DropShadowLabel(new Rect(s_WantedRect.x, s_WantedRect.y, s_WantedRect.width, 20), string.Format("Playing - {0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds));
                }
            }
            else
            {
                PreviewGUI.BeginScrollView(s_WantedRect, m_Position, s_WantedRect, "PreHorizontalScrollbar", "PreHorizontalScrollbarThumb");

                if (Event.current.type == EventType.Repaint)
                {
                    DoRenderPreview(true, clip, AudioUtil.GetImporterFromClip(clip), s_WantedRect, 1.0f);
                }

                for (int i = 0; i < c; ++i)
                {
                    if (c > 1 && rect.width > 64)
                    {
                        var labelRect = new Rect(s_WantedRect.x + 5, s_WantedRect.y + (s_WantedRect.height / c) * i, 30, 20);
                        EditorGUI.DropShadowLabel(labelRect, "ch " + (i + 1));
                    }
                }

                if (m_Clip == clip && playing)
                {
                    float t = AudioUtil.GetPreviewClipPosition();

                    System.TimeSpan ts = new System.TimeSpan(0, 0, 0, 0, (int)(t * 1000.0f));

                    GUI.DrawTexture(new Rect(s_WantedRect.x + (int)(sec2px * t), s_WantedRect.y, 2, s_WantedRect.height), EditorGUIUtility.whiteTexture);
                    if (rect.width > 64)
                        EditorGUI.DropShadowLabel(new Rect(s_WantedRect.x, s_WantedRect.y, s_WantedRect.width, 20), string.Format("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds));
                    else
                        EditorGUI.DropShadowLabel(new Rect(s_WantedRect.x, s_WantedRect.y, s_WantedRect.width, 20), string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds));
                }

                PreviewGUI.EndScrollView();
            }


            if ((s_PlayFirst || (s_AutoPlay && m_Clip != clip)))
            {
                // Autoplay preview
                PlayClip(clip, 0, s_Loop);
                s_PlayFirst = false;
            }
            AudioSettingsInternalCached.TryGet(out var unityAudioDisabled);
            if (unityAudioDisabled)
            {
                EditorGUILayout.HelpBox(s_TrPreviewDisabledMessage, MessageType.Info);
            }

            // force update GUI
            if (playing && GUIView.current != null)
                GUIView.current.Repaint();
        }

        static void Init()
        {
            if (s_PreButton != null)
                return;
            s_PreButton = "preButton";

            s_AutoPlay = EditorPrefs.GetBool("AutoPlayAudio", false);
            s_Loop = false;

            AudioSettingsInternalCached.TryGet(out var unityAudioDisabled);

            s_AutoPlayIcon = EditorGUIUtility.TrIconContent("preAudioAutoPlayOff", unityAudioDisabled ? s_PreviewDisabledMessage : "Turn Auto Play on/off");
            s_PlayIcon = EditorGUIUtility.TrIconContent("PlayButton", unityAudioDisabled ? s_PreviewDisabledMessage : "Play");
            s_LoopIcon = EditorGUIUtility.TrIconContent("preAudioLoopOff", unityAudioDisabled ? s_PreviewDisabledMessage : "Loop on/off");

            s_DefaultIcon = EditorGUIUtility.LoadIcon("Profiler.Audio");
        }

        public override void OnPreviewSettings()
        {
            if (s_DefaultIcon == null) Init();

            AudioClip clip = target as AudioClip;
            AudioSettingsInternalCached.TryGet(out var unityAudioDisabled);
            using (new EditorGUI.DisabledScope(unityAudioDisabled))
            {
                bool loop = s_Loop;
                s_Loop = GUILayout.Toggle(s_Loop, s_LoopIcon, EditorStyles.toolbarButton);
                if ((loop != s_Loop) && playing)
                    AudioUtil.LoopPreviewClip(s_Loop);
            }
        }
    }
}