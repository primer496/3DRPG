using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/SpriteRendererNode"), UseProcessAsync]
    public class SpriteRendererNode : SDNode
    {
        [SerializeField, HideInInspector]
        public SpriteRenderer m_Renderer;

        [SerializeField, HideInInspector]
        [Input("Sprite")] private Texture2D m_Sprite;

        [SerializeField, Preview, HideInInspector, HideInPreviewSelector]
        private GameObject m_GO;

        public SpriteRenderer renderer
        {
            get => m_Renderer;
            set
            {
                if (m_Renderer != value)
                {
                    m_Renderer = value;
                    m_Sprite = m_Renderer.sprite ? m_Renderer.sprite.texture : null;
                    m_GO = m_Renderer.gameObject;
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

        public override void SetTarget(Object target)
        {
            if (target is SpriteRenderer spriteRenderer)
                renderer = spriteRenderer;
        }

        public override IEnumerator ProcessAsync()
        {
            if (m_Renderer.sprite != null)
            {
                Sprite oldSprite = m_Renderer.sprite;
                Vector2 pivot = oldSprite.pivot;
                float pixelsPerUnit = oldSprite.pixelsPerUnit;
                Vector4 border = oldSprite.border;
               
                Sprite newSprite = Sprite.Create(
                    m_Sprite,
                    new Rect(0, 0, m_Sprite.width, m_Sprite.height),
                    new Vector2(pivot.x / oldSprite.rect.width, pivot.y / oldSprite.rect.height),
                    pixelsPerUnit,
                    0,
                    SpriteMeshType.Tight,
                    border
                );
                m_Renderer.sprite = newSprite;
            }
            else
            {
                Sprite newSprite = Sprite.Create(
                    m_Sprite,
                    new Rect(0, 0, m_Sprite.width, m_Sprite.height),
                    new Vector2(0f, 0f)
                );
                m_Renderer.sprite = newSprite;
            }
            this?.NotifyFieldChanged("m_GO");
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
