using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/AnimationClipNode")]
    public class AnimationClipNode : SDNode, ICreateNodeFrom<AnimationClip>
    {
        [Preview, HideInInspector, SerializeField]
        [Output("Animation Clip")] private AnimationClip m_AnimationClip;

        public AnimationClip animationClip
        {
            get => m_AnimationClip;
            set
            {
                if (m_AnimationClip != value)
                {
                    m_AnimationClip = value;
                    this?.NotifyFieldChanged("m_AnimationClip");
                }
            }
        }

        public override bool isRenamable => true;

        protected override void Enable()
        {
            hasSettings = true;
            base.Enable();
        }

        public bool InitializeNodeFromObject(AnimationClip value)
        {
            animationClip = value;
            return true;
        }
    }
}
