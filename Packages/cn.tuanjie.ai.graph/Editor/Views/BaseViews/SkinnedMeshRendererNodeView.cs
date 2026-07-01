using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(SkinnedMeshRendererNode))]
public class SkinnedMeshRendererNodeView : SDNodeView
{
    private SkinnedMeshRendererNode node;
    private ObjectField mRField;
    public override void Enable()
    {
        node = nodeTarget as SkinnedMeshRendererNode;
        if (node == null) return;

        mRField = new ObjectField("SkinnedMeshRenderer")
        {
            name = "SkinnedMeshRendererField",
            objectType = typeof(SkinnedMeshRenderer),
            allowSceneObjects = true,
            value = node.m_Renderer
        };
        controlsContainer.Add(mRField);
        BindProperty<ObjectField, Object, SkinnedMeshRendererNode>(mRField.name, "m_Renderer", OnChanged);


        controlsContainer.Add(mRField);

        base.Enable();
        RefreshExpandedState();
    }

    void OnChanged(ChangeEvent<Object> evt)
    {
        if (node.renderer == evt.newValue) return;
        node.renderer = evt.newValue as SkinnedMeshRenderer;
        NotifyNodeChanging();
    }
}