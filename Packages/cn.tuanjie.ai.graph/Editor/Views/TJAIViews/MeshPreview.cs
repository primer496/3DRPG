using UnityEngine;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    public class MeshPreviewRenderer : BasePreviewRenderer<Mesh>
    {
        private MeshPreview m_Previewer;

        public override void Initialize(UnityEngine.Object target, SDNode node)
        {
            base.Initialize(target, node);

            if (m_Previewer == null && target != null)
            {
                m_Previewer = new MeshPreview(target as Mesh);
            }
        }

        public override void Cleanup()
        {
            m_Previewer?.Dispose();
            m_Previewer = null;
        }

        public override void Update(UnityEngine.Object target)
        {
            base.Update(target);

            if (target == null)
            {
                m_Previewer?.Dispose();
                m_Previewer = null;
                return;
            }

            if (m_Previewer != null)
            {
                m_Previewer.mesh = target as Mesh;
            }
            else
            {
                m_Previewer = new MeshPreview(target as Mesh);
            }
        } 

        public override bool HasPreviewGUI()
        {
            return m_Previewer != null;
        }

        public override string GetPreviewTitle()
        {
            return GetPreviewTitleStatic(this.target).text;
        }

        public override void OnPreviewSettings()
        {
            m_Previewer.OnPreviewSettings();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            m_Previewer.OnPreviewGUI(rect, background);
        }
    }
}