using System.Collections;
using GraphProcessor;
#if UNITY_EDITOR
using UnityEditor.Animations;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/AnimatorState"), UseProcessAsync]
    public class AnimatorStateNode : SDNode
    {
        [SerializeField]
        public AnimatorState m_Animation;

        [SerializeField, HideInInspector]
        [Input("AnimationClip")] private AnimationClip m_Clip;

        public AnimatorState animation
        {
            get => m_Animation;
            set
            {
                if (m_Animation != value)
                {
                    m_Animation = value;
                }
            }
        }

        protected override void Enable()
        {
            base.Enable();
        }

        public override bool isRenamable => true;

        public override bool needTrigger => true;

        protected override void Destroy()
        {
            base.Destroy();
        }

        public override void CollectSubAssets()
        {
            base.CollectSubAssets();
        }

        public override IEnumerator ProcessAsync()
        {
            if (animation == null || m_Clip == null)
                yield break;

            animation.motion = m_Clip;

            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
        public override void SetTarget(Object target)
        {
            if (target is AnimatorState state)
            {
                animation = state;
            }
        }
    }
}
#endif