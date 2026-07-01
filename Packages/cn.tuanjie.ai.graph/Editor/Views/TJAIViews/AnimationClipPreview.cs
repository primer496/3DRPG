using UnityEditor.AIGraph.InternalBridge;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    public class AnimationClipPreviewRenderer : BasePreviewRenderer<AnimationClip>
    {
        private AvatarPreviewer m_Previewer;
        private AnimatorController m_Controller = null;
        private AnimatorStateMachine m_StateMachine;
        private AnimatorState m_State;
        bool m_FirstInitialization = true;

        private AvatarMask m_Mask = null;
        public AvatarMask mask
        {
            get { return m_Mask; }
            set { m_Mask = value; }
        }

        public override void Initialize(Object target, SDNode node)
        {
            base.Initialize(target, node);
        }

        public override void Cleanup()
        {
            m_Previewer?.OnDisable();
            m_Controller = null;
            m_StateMachine = null;
            m_State = null;
            m_Previewer = null;
        }

        public override void Update(Object target)
        {
            base.Update(target);
            Cleanup();
        }

        public override bool HasPreviewGUI()
        {
            InitPreview();
            return m_Previewer != null;
        }

        public override string GetPreviewTitle()
        {
            return GetPreviewTitleStatic(target).text;
        }

        public override void OnPreviewSettings()
        {
            m_Previewer.DoPreviewSettings();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (target == null || m_Previewer == null)
                return;

            bool isRepaint = (Event.current.type == EventType.Repaint);

            InitController();

            if (isRepaint)
                m_Previewer.UpdateTimeControl();

            // Set to full take range
            AnimationClip clip = target;
            AnimationClipSettings previewInfo = AnimationUtility.GetAnimationClipSettings(clip);

            // Set settings
            m_Previewer.isTimeControlLoop = true; // always looping, waiting for UI ctrl...

            // Sample Animation
            if (isRepaint && m_Previewer.PreviewObject != null)
            {
                if (clip.legacy == false && m_Previewer.Animator != null)
                {
                    if (m_State != null)
                        m_State.iKOnFeet = m_Previewer.IKOnFeet;

                    float normalizedTime = previewInfo.stopTime - previewInfo.startTime != 0 ? (m_Previewer.timeControlCurrentTime - previewInfo.startTime) / (previewInfo.stopTime - previewInfo.startTime) : 0.0f;
                    m_Previewer.Animator.Play(0, 0, normalizedTime);
                    m_Previewer.Animator.Update(m_Previewer.timeControlDeltaTime);
                }
                else
                {
                    clip.SampleAnimation(m_Previewer.PreviewObject, m_Previewer.timeControlCurrentTime);
                }
            }

            m_Previewer.DoAvatarPreview(rect, background);
        }

        void InitPreview()
        {
            if (target == null)
                return;

            if (m_Previewer == null)
            {
                m_Previewer = new AvatarPreviewer(null, target as Motion);
                m_Previewer.OnAvatarChangeFunc = SetPreviewAvatar;
                m_Previewer.fps = Mathf.RoundToInt((target as AnimationClip).frameRate);
                m_Previewer.ShowIKOnFeetButton = (target as Motion).isHumanMotion;
                m_Previewer.ResetPreviewFocus();
            }

            // force an update on timeControl if AvatarPreviewer is closed when creating/editing animation curves
            // prevent from having a nomralizedTime == -inf
            if (m_Previewer.timeControlCurrentTime == Mathf.NegativeInfinity)
                m_Previewer.UpdateTimeControl();

            m_Previewer.SetStopTime(target.length);
        }

        private void SetPreviewAvatar()
        {
            DestroyController();
            InitController();
        }

        private void DestroyController()
        {
            if (m_Previewer != null && m_Previewer.Animator != null)
            {
                AnimatorController.SetAnimatorController(m_Previewer.Animator, null);
            }

            Object.DestroyImmediate(m_Controller);
            Object.DestroyImmediate(m_State);
            m_Controller = null;
            m_StateMachine = null;
            m_State = null;
        }

        private void InitController()
        {
            if (target == null || target.legacy)
                return;

            if (m_Previewer != null && m_Previewer.Animator != null)
            {
                bool wasInitialized = true;
                if (m_Controller == null)
                {
                    m_Controller = new AnimatorController();
                    InternalAPI.Internal_AnimatorController_SetPushUndo(m_Controller, false);
                    m_Controller.hideFlags = HideFlags.HideAndDontSave;
                    m_Controller.AddLayer("preview");

                    m_StateMachine = m_Controller.layers[0].stateMachine;
                    InternalAPI.Internal_AnimatorStateMachine_SetPushUndo(m_StateMachine, false);
                    m_StateMachine.hideFlags = HideFlags.HideAndDontSave;

                    if (mask != null)
                    {
                        AnimatorControllerLayer[] layers = m_Controller.layers;
                        layers[0].avatarMask = mask;
                        m_Controller.layers = layers;
                    }
                    wasInitialized = false;
                }

                if (m_State == null)
                {
                    m_State = m_StateMachine.AddState("preview");
                    InternalAPI.Internal_AnimatorState_SetPushUndo(m_State, false);
                    AnimatorControllerLayer[] layers = m_Controller.layers;
                    m_State.motion = target;
                    m_Controller.layers = layers;

                    m_State.iKOnFeet = m_Previewer.IKOnFeet;
                    m_State.hideFlags = HideFlags.HideAndDontSave;
                    wasInitialized = false;
                }


                AnimatorController.SetAnimatorController(m_Previewer.Animator, m_Controller);
                if (InternalAPI.Internal_AnimatorController_GetEffectiveAnimatorController(m_Previewer.Animator) != m_Controller)
                {
                    AnimatorController.SetAnimatorController(m_Previewer.Animator, m_Controller);
                }
                if (!wasInitialized)
                {
                    m_Previewer.Animator.Play(0, 0, 0);
                    m_Previewer.Animator.Update(0);

                    if (m_FirstInitialization)
                    {
                        m_Previewer.ResetPreviewFocus();
                        m_FirstInitialization = false;
                    }
                }
            }
        }
    }
}