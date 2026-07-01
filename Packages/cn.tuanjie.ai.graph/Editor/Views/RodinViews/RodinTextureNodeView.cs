using System.Collections.Generic;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(RodinTextureNode))]
public class RodinTextureNodeView : TJAIBaseAssetNodeView
{
    private RodinTextureNode node;

    private static readonly List<string> geometryFormatChoices = new()
    {
        "glb", "usdz", "fbx", "obj"
    };

    private static readonly List<string> materialChoices = new()
    {
        "PBR", "Shaded"
    };

    private static readonly List<string> resolutionChoices = new()
    {
        "Basic", "High"
    };

    public override void Enable()
    {
        node = nodeTarget as RodinTextureNode;
        if (node == null) return;

        var geometryFormatField = new DropdownField("Geometry Format", geometryFormatChoices, 0)
        {
            name = "geometryFormatField",
            tooltip = "Output geometry file format."
        };
        
        var materialField = new DropdownField("Material", materialChoices, 0)
        {
            name = "materialField",
            tooltip = "Material type for the generated texture."
        };
        
        var resolutionField = new DropdownField("Resolution", resolutionChoices, 0)
        {
            name = "resolutionField",
            tooltip = "Resolution of the output texture."
        };

        controlsContainer.Add(geometryFormatField);
        controlsContainer.Add(materialField);
        controlsContainer.Add(resolutionField);
        
        BindProperty<DropdownField, string, RodinTextureNode>(geometryFormatField.name, nameof(node.geometryFileFormat));
        BindProperty<DropdownField, string, RodinTextureNode>(materialField.name, nameof(node.material));
        BindProperty<DropdownField, string, RodinTextureNode>(resolutionField.name, nameof(node.resolution));

        base.Enable();
        RefreshExpandedState();
    }
}