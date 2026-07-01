using System.Linq;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(MaterialReplaceNode))]
public class MaterialReplaceNodeView : SDNodeView
{
    private MaterialReplaceNode node;
    private DropdownField dropdownResult;

    public override void Enable()
    {
        node = nodeTarget as MaterialReplaceNode;
        if (node == null) return;

        // var ussPath = "uss/VastNodeStyle";
        // var styleSheet = Resources.Load<StyleSheet>(ussPath);
        // styleSheets.Add(styleSheet);

        base.Enable();
        
        var resultResource = node.m_Sources ?? new Material[0];
        dropdownResult = new DropdownField(resultResource.Select(a => a.name).ToList(), 0)
        {
            label = "Preview Source",
            name = "resultDropdown"
        };
        if (node.m_Sources == null || node.m_Sources.Length == 0)
        {
            dropdownResult.style.display = DisplayStyle.None;
        }
        else
        {
            dropdownResult.value = dropdownResult.choices[0];
        }

        dropdownResult.RegisterValueChangedCallback(OnResultDropdownChanged);
        controlsContainer.Add(dropdownResult);

        node.onReplace -= OnResultsChange;
        node.onReplace += OnResultsChange;

        RefreshExpandedState();
    }

    public override void Disable()
    {
        base.Disable();
        dropdownResult.UnregisterValueChangedCallback(OnResultDropdownChanged);
    }

    private void OnResultsChange()
    {
        var dropdownResult = controlsContainer.Q<DropdownField>("resultDropdown");
        var resultResource = node.m_Sources ?? new Material[0];
        dropdownResult.choices = resultResource.Select(a => a.name).ToList();

        if (node.m_Sources == null || node.m_Sources.Length == 0)
        {
            dropdownResult.style.display = DisplayStyle.None;
        }
        else
        {
            dropdownResult.style.display = DisplayStyle.Flex;
            dropdownResult.value = dropdownResult.choices[0];
        }

    }

    private void OnResultDropdownChanged(ChangeEvent<string> evt)
    {
        foreach (var mtl in node.m_Sources)
        {
            if (mtl.name == evt.newValue)
            {
                node.source = mtl;
            }
        }
    }
}