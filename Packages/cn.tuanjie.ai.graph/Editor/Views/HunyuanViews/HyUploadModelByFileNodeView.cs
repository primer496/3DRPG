using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyUploadModelByFileNode))]
public class HyUploadModelByFileNodeView : SDNodeView
{
    private HyUploadModelByFileNode node;
    private FilePathField fileField;
    private VisualElement objContainer;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as HyUploadModelByFileNode;
        
        fileField = new FilePathField("Model Path")
        {
            name = "modelFilePathField", fileExtension = "fbx,glb,obj"
        };
        controlsContainer.Add(fileField);
        BindProperty<FilePathField, string, HyUploadModelByFileNode>(fileField.name, string.Empty, OnFileSelected);

        objContainer = new VisualElement() { name = "objContainer" };
        var mtlField = new FilePathField("Material Path")
        {
            name = "materialFilePathField", fileExtension = "mtl"
        };
        
        base.Enable();
        RefreshExpandedState();
    }

    void OnFileSelected(ChangeEvent<string> evt)
    {
        Debug.Log($"Call OnFileSelected: {evt.newValue}");
        node.modelPath = evt.newValue;
        NotifyNodeChanging();
    }
}