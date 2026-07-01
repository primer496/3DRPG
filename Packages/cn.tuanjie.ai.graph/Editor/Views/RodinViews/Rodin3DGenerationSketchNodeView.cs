using System.Collections.Generic;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(Rodin3DGenerationSketchNode))]
public class Rodin3DGenerationSketchNodeView : TJAIBaseAssetNodeView
{
    private Rodin3DGenerationSketchNode node;

    private static readonly List<string> geometryFormatChoices = new()
    {
        "glb", "fbx", "obj"
    };

    private static readonly List<string> materialChoices = new()
    {
        "PBR", "Shaded", "All"
    };

    public override void Enable()
    {
        node = nodeTarget as Rodin3DGenerationSketchNode;
        if (node == null) return;

        var geometryFormatField = new DropdownField("Geometry Format", geometryFormatChoices, 0)
        {
            name = "geometryFormatField",
            tooltip = "Output geometry file format."
        };

        var materialField = new DropdownField("Material", materialChoices, 0)
        {
            name = "materialField",
            tooltip = "Material type for the generated model."
        };

        controlsContainer.Add(geometryFormatField);
        controlsContainer.Add(materialField);

        BindProperty<DropdownField, string, Rodin3DGenerationSketchNode>(geometryFormatField.name,
            nameof(node.geometryFileFormat));
        BindProperty<DropdownField, string, Rodin3DGenerationSketchNode>(materialField.name, nameof(node.material));

        base.Enable();
        RefreshExpandedState();
    }
}