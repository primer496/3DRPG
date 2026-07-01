using UnityEditorInternal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.AIGraph.InternalBridge
{
    internal static class InternalAPI
    {
        public static GameObject Internal_InstantiateForAnimatorPreview(UnityObject target) =>
            EditorUtility.InstantiateForAnimatorPreview(target);

        public static Vector2 Internal_PreviewGUI_Drag2D(Vector2 scrollPosition, Rect position) =>
             PreviewGUI.Drag2D(scrollPosition, position);

        public static RenderTexture Internal_PreviewUtility_RenderTexture(PreviewRenderUtility previewRenderUtility) =>
            previewRenderUtility.renderTexture;

        public static int Interal_TextureUtil_GetMipmapCount(Texture t) =>
            TextureUtil.GetMipmapCount(t);

        public static bool Interal_TextureUtil_IsUsageModeDefault(Texture t) =>
            TextureUtil.GetUsageMode(t) == TextureUsageMode.Default;

        public static bool Interal_TextureUtil_IsUsageModeRGB(Texture t) =>
            TextureUtil.IsRGBMUsageMode(TextureUtil.GetUsageMode(t));

        public static bool Interal_TextureUtil_IsUsageModeDoubleLDR(Texture t) =>
            TextureUtil.IsDoubleLDRUsageMode(TextureUtil.GetUsageMode(t));

        public static bool Interal_TextureUtil_IsUsageModeNormalMap(Texture t) =>
            TextureUtil.IsNormalMapUsageMode(TextureUtil.GetUsageMode(t));

        public static void Internal_TextureUtil_SetFilterModeNoDirty(Texture tex, FilterMode mode) =>
            TextureUtil.SetFilterModeNoDirty(tex, mode);

        public static void Internal_PreviewGUI_BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar) =>
            PreviewGUI.BeginScrollView(position, scrollPosition, viewRect, horizontalScrollbar, verticalScrollbar);

        public static Vector2 Internal_PreviewGUI_EndScrollView() =>
            PreviewGUI.EndScrollView();

        public static void Internal_HandleUtility_ApplyWireMaterial() =>
             HandleUtility.ApplyWireMaterial();

        public static SpriteMetaData[] Internal_TextureImporter_GetSpriteMetaDatas(TextureImporter textureImporter) =>
            textureImporter.GetSpriteMetaDatas();

        public static void Internal_TextureImporter_GetWidthAndHeight(TextureImporter textureImporter, ref int width, ref int height) =>
            textureImporter.GetWidthAndHeight(ref width, ref height);

        public static float Internal_EditorGUIInternal_ExposureSlider(float value, ref float maxValue, GUIStyle style) =>
            EditorGUIInternal.ExposureSlider(value, ref maxValue, style);

        public static Rect Internal_EditorGUILayout_BeginHorizontal(GUIContent content, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUILayout.BeginHorizontal(content, style, options);

        public static bool Internal_NativeClassExtensionUtilities_ExtendsANativeType(UnityObject obj) =>
            NativeClassExtensionUtilities.ExtendsANativeType(obj);

        public static MonoScript Internal_MonoScript_FromScriptedObject(UnityObject target) =>
            MonoScript.FromScriptedObject(target);

        public static bool Internal_AnimatorController_GetEffectiveAnimatorController(Animator animator) =>
            Animations.AnimatorController.GetEffectiveAnimatorController(animator);

        public static bool Internal_AnimatorController_SetPushUndo(Animations.AnimatorController controller, bool pushUndo) =>
            controller.pushUndo = pushUndo;

        public static bool Internal_AnimatorStateMachine_SetPushUndo(Animations.AnimatorStateMachine stateMachine, bool pushUndo) =>
            stateMachine.pushUndo = pushUndo;

        public static bool Internal_AnimatorState_SetPushUndo(Animations.AnimatorState state, bool pushUndo) =>
            state.pushUndo = pushUndo;

        public static GUIContent Internal_EditorGUIUtility_TextContent(string textAndTooltip) => EditorGUIUtility.TextContent(textAndTooltip);

        public static int Internal_PreviewGUI_CycleButton(int selected, GUIContent[] options) => PreviewGUI.CycleButton(selected, options);

        public static GUIStyle Internal_EditorStyles_ToolbarDropDownRight() => EditorStyles.toolbarDropDownRight;

        public static void Internal_InternalEditorUtility_DrawSkyboxMaterial(Material mat, Camera cam) => InternalEditorUtility.DrawSkyboxMaterial(mat, cam);
    }

    internal class AvatarPreviewer
    {
        private AvatarPreview m_Previewer;

        public bool isTimeControlLoop
        {
            get => m_Previewer.timeControl.loop;
            set => m_Previewer.timeControl.loop = value;
        }

        public bool IKOnFeet
        {
            get => m_Previewer.IKOnFeet;
        }

        public float timeControlDeltaTime
        {
            get => m_Previewer.timeControl.deltaTime;
        }

        public float timeControlCurrentTime
        {
            get => m_Previewer.timeControl.currentTime;
        }

        public int fps
        {
            get => m_Previewer.fps;
            set => m_Previewer.fps = value;
        }

        public bool ShowIKOnFeetButton
        {
            get => m_Previewer.ShowIKOnFeetButton;
            set => m_Previewer.ShowIKOnFeetButton = value;
        }

        public GameObject PreviewObject
        {
            get => m_Previewer.PreviewObject;
        }

        public Animator Animator
        {
            get => m_Previewer.Animator;
        }

        public AvatarPreviewer(Animator previewObjectInScene, Motion objectOnSameAsset)
        {
            m_Previewer = new AvatarPreview(previewObjectInScene, objectOnSameAsset);
        }

        public AvatarPreview.OnAvatarChange OnAvatarChangeFunc
        {
            set => m_Previewer.OnAvatarChangeFunc = value;
        }

        public void DoAvatarPreview(Rect rect, GUIStyle background) => m_Previewer.DoAvatarPreview(rect, background);

        public void OnDisable() => m_Previewer?.OnDisable();

        public void DoPreviewSettings() => m_Previewer.DoPreviewSettings();

        public void UpdateTimeControl() => m_Previewer.timeControl.Update();
    
        public void ResetPreviewFocus() => m_Previewer.ResetPreviewFocus();

        public void SetStopTime(float stoptime) => m_Previewer.timeControl.stopTime = stoptime;
    }
}