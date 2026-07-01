using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(MeshFilterNode))]
public class MeshFilterNodeView : SDNodeView
{
    private MeshFilterNode node;
    private ObjectField goField;
    public override void Enable()
    {
        node = nodeTarget as MeshFilterNode;
        if (node == null) return;

        goField = new ObjectField("GameObject")
        {
            name = "gameobjectField",
            objectType = typeof(GameObject),
            allowSceneObjects = true,
            value = node.owner
        };
        controlsContainer.Add(goField);
        BindProperty<ObjectField, Object, MeshFilterNode>(goField.name, "m_Owner", OnMeshChanged);


        controlsContainer.Add(goField);

        base.Enable();
        RefreshExpandedState();
    }

    void OnMeshChanged(ChangeEvent<Object> evt)
    {
        if (node.owner == evt.newValue) return;
        node.owner = evt.newValue as GameObject;
        NotifyNodeChanging();
    }
}