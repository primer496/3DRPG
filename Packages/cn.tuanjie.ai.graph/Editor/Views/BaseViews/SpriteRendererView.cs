using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(SpriteRendererNode))]
public class SpriteRendererNodeView : SDNodeView
{
    private SpriteRendererNode node;
    private ObjectField textureField;
    public override void Enable()
    {
        node = nodeTarget as SpriteRendererNode;
        if (node == null) return;

        textureField = new ObjectField("SpriteRenderer")
        {
            name = "spriteRendererField",
            objectType = typeof(SpriteRenderer),
            allowSceneObjects = true,
            value = node.m_Renderer
        };
        controlsContainer.Add(textureField);
        BindProperty<ObjectField, Object, SpriteRendererNode>(textureField.name, "m_Renderer", OnTextureChanged);


        controlsContainer.Add(textureField);

        base.Enable();
        RefreshExpandedState();
    }

    void OnTextureChanged(ChangeEvent<Object> evt)
    {
        if (node.renderer == evt.newValue) return;
        node.renderer = evt.newValue as SpriteRenderer;
        NotifyNodeChanging();
    }
}