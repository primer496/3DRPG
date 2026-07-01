using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(GameObjectAssetNode))]
public class GameObjectAssetNodeView : SDNodeView
{
    private GameObjectAssetNode node;
    private ObjectField objField;
    public override void Enable()
    {
        node = nodeTarget as GameObjectAssetNode;
        if (node == null) return;

        objField = new ObjectField("Game Object")
        {
            name = "goField", objectType = typeof(GameObject), allowSceneObjects = true,
            value = node.obj
        };
        controlsContainer.Add(objField);
        BindProperty<ObjectField, Object, GameObjectAssetNode>(objField.name, "m_obj", OnGOChanged);
        
        controlsContainer.Add(objField);

        node.onProcessed += UpdateField;
        
        base.Enable();
        RefreshExpandedState();
    }

    public override void Disable()
    {
        base.Disable();
        node.onProcessed -= UpdateField;
    }

    void OnGOChanged(ChangeEvent<Object> evt)
    {
        if (node.obj == evt.newValue) return;
        node.obj = evt.newValue as GameObject;
        NotifyNodeChanging();
    }

    void UpdateField()
    {
        if (node.GetInputEdges().Count == 0) return;
        objField.SetValueWithoutNotify(node.inputGO);
    }
}