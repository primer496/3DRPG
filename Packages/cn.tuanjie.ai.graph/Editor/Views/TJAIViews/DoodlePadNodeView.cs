using GraphProcessor;
using UnityEditor.AIGraphs;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(DoodlePadNode))]
    public class DoodlePadNodeView : SDNodeView
    {
        private DoodlePadNode node;
        private DoodlePadView m_DoodlePadView;

        protected void DestroyDoodlePadView()
        {
            m_DoodlePadView = null;
            foreach (var previewComp in smartPreviews)
                previewComp?.UpdatePreview();
            nodeTarget.UpdatePortsForField(nameof(node.m_maskImage));
            RefreshPorts();
            NotifyNodeChanging();
        }

        protected void OnPreviewImageDoubleClick(MouseDownEvent evt)
        {
            if (evt.clickCount != 2) return;
            GenDoodlePadView();
        }

        protected void GenDoodlePadView()
        {
            if (m_DoodlePadView != null)
            {
                m_DoodlePadView.OnImageUpdated();
                return;
            }
            var inputImage = node.GetTexture();
            if (inputImage == null) return;

            m_DoodlePadView = new DoodlePadView(node);
            m_DoodlePadView.onDestroyDoodlePadView += DestroyDoodlePadView;
            m_DoodlePadView.RemoveFromHierarchy();
            owner.contentContainer.Add(m_DoodlePadView);
        }

        public override void Enable()
        {
            base.Enable();
            
            node = nodeTarget as DoodlePadNode;
            if (node == null) return;
            
            // for doodle
            // SDEditorUtils.SetCursor(previewContainer, MouseCursor.Link);
            var preview = previewContainer.Q<SmartPreviewComponent>();
            preview.tooltip = "Double click to paint mask";
            registerCallbackAction += () =>
            {
                previewContainer.RegisterCallback<MouseDownEvent>(OnPreviewImageDoubleClick);
            };
            unregisterCallbackAction += () =>
            {
                previewContainer.UnregisterCallback<MouseDownEvent>(OnPreviewImageDoubleClick);
            };
            var genMaskBtn = new Button(GenDoodlePadView)
            {
                name = "genMaskBtn", text = "Draw Mask"
            };
            previewContainer.Insert(previewContainer.childCount - 1, genMaskBtn);
            nodeTarget.onFieldValueChangedHandlers[nameof(node.m_maskImage)] += () =>
            {
                genMaskBtn.style.display = node.maskImage == null ? DisplayStyle.None : DisplayStyle.Flex;
            };
        }

        public override void Disable()
        {
            // clear doodle
            if (m_DoodlePadView != null)
            {
                m_DoodlePadView.parent.Remove(m_DoodlePadView);
                m_DoodlePadView.onDestroyDoodlePadView -= DestroyDoodlePadView;
                DestroyDoodlePadView();
            }
            base.Disable();
        }
    }
}