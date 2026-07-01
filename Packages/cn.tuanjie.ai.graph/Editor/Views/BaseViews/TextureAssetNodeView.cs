using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(TextureAssetNode))]
public class TextureAssetNodeView : SDNodeView
{
    private TextureAssetNode node;
    private ObjectField textureField;
    public override void Enable()
    {
        node = nodeTarget as TextureAssetNode;
        if (node == null) return;

        textureField = new ObjectField("Texture")
        {
            name = "textureField", objectType = typeof(Texture2D), allowSceneObjects = false,
            value = node.outputTexture
        };
        controlsContainer.Add(textureField);
        BindProperty<ObjectField, Object, TextureAssetNode>(textureField.name, "m_OutputTexture", OnTextureChanged);


        controlsContainer.Add(textureField);
        
        base.Enable();
        RefreshExpandedState();
    }
    
    void OnTextureChanged(ChangeEvent<Object> evt)
    {
        if (node.outputTexture == evt.newValue) return;
        node.outputTexture = evt.newValue as Texture2D;
        NotifyNodeChanging();
        nodeTarget.outputPorts.PushDatas();
    }
}