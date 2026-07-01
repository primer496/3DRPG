using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/AnimationComponent"), UseProcessAsync]
    public class AnimationComponentNode : SDNode
    {
        [SerializeField]
        public Animation m_Animation;

        [SerializeField, HideInInspector]
        [Input("AnimationClip")] private AnimationClip m_Clip;

        [SerializeField, Preview, HideInInspector, HideInPreviewSelector]
        private GameObject m_GO;

        public Animation animation
        {
            get => m_Animation;
            set
            {
                if (m_Animation != value)
                {
                    m_Animation = value;
                    m_GO = m_Animation == null ? null : m_Animation.gameObject;
                    this?.NotifyFieldChanged("m_GO");
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

            animation.clip = m_Clip;
            animation.AddClip(m_Clip, m_Clip.name);
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
        public override void SetTarget(Object target)
        {
            if (target is Animation tgt)
            {
                animation = tgt;
            }
        }
    }
}
