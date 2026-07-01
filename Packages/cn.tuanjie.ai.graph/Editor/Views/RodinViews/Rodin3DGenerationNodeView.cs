using System.Collections.Generic;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(Rodin3DGenerationNode))]
public class Rodin3DGenerationRegularNodeView : TJAIBaseAssetNodeView
{
    private Rodin3DGenerationNode node;
    private SliderRangeIntegerField qualityField;

    private static readonly List<string> conditionModeChoices = new()
    {
        "concat", "fuse"
    };

    private static readonly List<string> geometryFormatChoices = new()
    {
        "glb", "fbx", "obj"
    };

    private static readonly List<string> materialChoices = new()
    {
        "PBR", "Shaded", "All"
    };

    private static readonly List<string> qualityChoices = new()
    {
        "low", "medium", "high", "extra-low"
    };

    private static readonly List<string> meshModeChoices = new()
    {
        "Quad", "Raw"
    };

    public override void Enable()
    {
        node = nodeTarget as Rodin3DGenerationNode;
        if (node == null) return;

        var conditionModeField = new DropdownField("Condition Mode", conditionModeChoices, 0)
        {
            name = "conditionModeField",
            tooltip = "Mode for multi-image generation: fuse for multiple objects, concat for multi-view single object."
        };
        
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
        
        qualityField = new SliderRangeIntegerField("Polygon Count", node.minQuadPolygonCount, node.maxQuadPolygonCount)
        {
            name = "qualityField",
            tooltip = "Generation quality level (face count)."
        };
        
        var meshModeField = new DropdownField("Mesh Mode", meshModeChoices, 0)
        {
            name = "meshModeField",
            tooltip = "Type of faces for generated models."
        };
        
        controlsContainer.Add(conditionModeField);
        controlsContainer.Add(geometryFormatField);
        controlsContainer.Add(materialField);
        controlsContainer.Add(qualityField);
        controlsContainer.Add(meshModeField);
        
        BindProperty<DropdownField, string, Rodin3DGenerationNode>(conditionModeField.name, nameof(node.conditionMode));
        BindProperty<DropdownField, string, Rodin3DGenerationNode>(geometryFormatField.name, nameof(node.geometryFileFormat));
        BindProperty<DropdownField, string, Rodin3DGenerationNode>(materialField.name, nameof(node.material));
        BindProperty<SliderRangeIntegerField, int, Rodin3DGenerationNode>(qualityField.name, nameof(node.qualityOverride));
        BindProperty<DropdownField, string, Rodin3DGenerationNode>(meshModeField.name, nameof(node.meshMode), OnMeshModeChanged);

        base.Enable();
        RefreshExpandedState();
    }

    void OnMeshModeChanged(ChangeEvent<string> evt)
    {
        if (evt.newValue == node.meshMode) return;
        node.meshMode = evt.newValue;
        if (evt.newValue == "Quad")
        {
            qualityField.MinValue = node.minQuadPolygonCount;
            qualityField.MaxValue = node.maxQuadPolygonCount;
        }
        else
        {
            qualityField.MinValue = node.minTriPolygonCount;
            qualityField.MaxValue = node.maxTriPolygonCount;
        }
    }
}