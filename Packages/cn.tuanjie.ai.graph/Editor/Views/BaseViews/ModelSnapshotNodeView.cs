using GraphProcessor;
using UnityEditor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(ModelSnapshotNode))]
public class ModelSnapshotNodeView : SDNodeView
{
    private ModelSnapshotNode node;

    private Texture2D snapshotCache;
    public override void Enable()
    {
        node = nodeTarget as ModelSnapshotNode;
        if (node == null) return;
        base.Enable(); 
        RefreshExpandedState();

        node.getSnapshot += () =>
        {
            var goPreview = this.previewContainer.Q<SmartPreviewComponent>().previewRenderer as GameObjectPreviewRenderer;
            return goPreview.previewCache;
        };
        node.getSnapshotAsync += () =>
        {
            return snapshotCache;
        };
        node.getSnapshotWindow += () =>
        {
            return GenSnapshotView();
        };
    }

    public EditorWindow GenSnapshotView()
    {
        return SnapshootWindow.Open(node.go, this, node);
    }


    private class SnapshootWindow : EditorWindow
    {
        internal GameObjectPreviewRenderer previewRenderer;
        private ModelSnapshotNodeView m_View;
        IMGUIContainer previewContainer;
        private Rect currentRect;
        public static EditorWindow Open(GameObject target, ModelSnapshotNodeView view, SDNode node)
        {
            var w = GetWindow<SnapshootWindow>();
            w.previewRenderer = new GameObjectPreviewRenderer();
            w.previewRenderer.Initialize(target, node);
            w.titleContent = new GUIContent("Snapshot Window");
            w.minSize = new Vector2(500, 500);
            w.m_View = view;
            w.Show();

            return w;
        }

        void CreateGUI()
        {
            previewContainer = new IMGUIContainer(() => OnDrawPreview());
            previewContainer.style.flexDirection = FlexDirection.Row;
            previewContainer.style.alignItems = Align.Center;
            previewContainer.style.paddingLeft = 6;
            previewContainer.style.paddingRight = 6;

            rootVisualElement.Add(previewContainer);
        }

        private void OnDrawPreview()
        {
            if (previewRenderer.HasPreviewGUI())
            {
                GUILayout.BeginHorizontal();
                currentRect = GUILayoutUtility.GetRect(500f, 500f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                previewRenderer.OnPreviewGUI(currentRect, GUIStyle.none);
                GUILayout.EndHorizontal();
            }
        }
        void OnDisable() => InvokeClosed(); 
        void OnDestroy() => InvokeClosed();

        public void InvokeClosed()
        {
            if (previewRenderer != null)
                m_View.snapshotCache = previewRenderer.previewCache;

            previewRenderer?.Cleanup();
            previewRenderer = null;
        }
    }
}