using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(VastRetargetNode))]
public class VastRetargetNodeView : TJAIBaseAssetNodeView
{
    private VastRetargetNode node;
    private ListView animationListView;
    private DropdownField dropdownResult;

    public override void Enable()
    {
        node = nodeTarget as VastRetargetNode;
        if (node == null) return;

//        var ussPath = "uss/VastNodeStyle";
//        var styleSheet = Resources.Load<StyleSheet>(ussPath);
//        styleSheets.Add(styleSheet);

        if (node.animations.Count == 0)
            node.animations.Add(node.animationChoices[0]);
        animationListView = new ListView(node.animations, -1, CreateItem, BindItem)
        {
            name = "Animations", showAddRemoveFooter = true, 
            showFoldoutHeader = true, headerTitle = "Animations",
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            unbindItem = UnbindItem
        };

        
        controlsContainer.Add(animationListView);

        
        base.Enable();

        var resultResource = node.results??new List<UnityEngine.AnimationClip>();
        dropdownResult = new DropdownField(resultResource.Select(a => a.name).ToList(), 0)
        {
            label = "Preview Animation Clip",
            name = "resultDropdown"
        };
        if (node.results == null || node.results.Count == 0)
        {
            dropdownResult.style.display = DisplayStyle.None;
        }
        else
        {
            dropdownResult.value = dropdownResult.choices[0];
        }
        
        dropdownResult.RegisterValueChangedCallback(OnResultDropdownChanged);
        controlsContainer.Add(dropdownResult);

        node.OnResultsChange -= OnResultsChange;
        node.OnResultsChange += OnResultsChange;

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
        var resultResource = node.results ?? new List<UnityEngine.AnimationClip>();
        dropdownResult.choices = resultResource.Select(a => a.name).ToList();

        if (node.results == null || node.results.Count == 0)
        {
            dropdownResult.style.display = DisplayStyle.None;
        }
        else
        {
            dropdownResult.style.display = DisplayStyle.Flex;
            dropdownResult.value = dropdownResult.choices[0];
        }

    }

    private VisualElement CreateItem()
    {
        var dropdown = new DropdownField(node.animationChoices, 0);
        
        // dropdown.AddToClassList("vast-dropdown");
        return dropdown;
    }

    private void BindItem(VisualElement item, int index)
    {
        if (item is not DropdownField dropdown) return;
        if (string.IsNullOrEmpty(node.animations[index]))
            node.animations[index] = dropdown.value;
        dropdown.RegisterValueChangedCallback(OnAnimationDropdownChanged(index));
    }

    private void UnbindItem(VisualElement item, int index)
    {
        var dropdown = item as DropdownField;
        dropdown?.UnregisterValueChangedCallback(OnAnimationDropdownChanged(index));
    }

    private EventCallback<ChangeEvent<string>> OnAnimationDropdownChanged(int index)
    {
        return evt =>
        {
            node.animations[index] = evt.newValue;
            NotifyNodeChanging();
        };
    }

    private void OnResultDropdownChanged(ChangeEvent<string> evt)
    {
        foreach(var animaclip in node.results)
        {
            if (animaclip.name == evt.newValue)
            {
                node.clip = animaclip;
            }
        }
    }
}