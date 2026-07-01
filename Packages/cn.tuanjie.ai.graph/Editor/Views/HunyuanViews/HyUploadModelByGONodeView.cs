using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(UploadModelByGONode))]
public class UploadModelByGONodeView : SDNodeView
{
    private UploadModelByGONode node;
    private ObjectField objField;
    private VisualElement objContainer;

    public override void Enable()
    {
        node = nodeTarget as UploadModelByGONode;
        if (node == null) return;

        objField = new ObjectField("GameObject")
        {
            name = "objField", objectType = typeof(GameObject), allowSceneObjects = true,
            value = node.obj
        };
        controlsContainer.Add(objField);
        BindProperty<ObjectField, Object, UploadModelByGONode>(objField.name, "m_Obj", OnObjChanged);
        
        base.Enable();
        RefreshExpandedState();
    }

    void OnObjChanged(ChangeEvent<Object> evt)
    {
        if (node.obj == evt.newValue) return;
        node.obj = evt.newValue as GameObject;
        node.uploaded = false;
        NotifyNodeChanging();
    }
}