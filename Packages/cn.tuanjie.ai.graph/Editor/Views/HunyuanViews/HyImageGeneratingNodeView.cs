using System.Collections.Generic;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(HyImageGeneratingNode))]
public class HyImageGeneratingNodeView : TJAIBaseAssetNodeView
{
    private HyImageGeneratingNode node;
    private DropdownField sizeDropdown;
    private DropdownField styleDropdown;
    private TextField revisedPromptField;

    private readonly List<string> sizeChoices = new()
    {
        "1024x1024", "1024x768", "1152x864", "768x1024", "768x1280", "1280x768"
    };

    private readonly List<string> styleChoices = new()
    {
        "未定义", "宫崎骏风格", "新海诚风格", "去旅行风格", "水彩风格", "像素风格", "童话世界风格",
        "奇趣卡通风格", "赛博朋克风格", "极简风格", "复古风格", "暗黑系风格", "波普风风格",
        "糖果色风格", "胶片电影风格", "素描风格", "水墨画风格", "油画风格", "粉笔风格",
        "粘土风格", "毛毡风格", "刺绣风格", "彩铅风格", "莫奈风格", "毕加索风格",
        "穆夏风格", "古风二次元风格", "都市二次元风格", "悬疑风格", "校园风格", "都市异能风格"
    };

    public override void Enable()
    {
        node = nodeTarget as HyImageGeneratingNode;
        if (node == null) return;

        sizeDropdown = new DropdownField(sizeChoices, 0)
        {
            label = "Size", name = "sizeDropdown"
        };
        sizeDropdown.AddToClassList("vast-dropdown");
        styleDropdown = new DropdownField(styleChoices, 0)
        {
            label = "Style", name = "styleDropdown"
        };
        styleDropdown.AddToClassList("vast-dropdown");
        controlsContainer.Add(sizeDropdown);
        controlsContainer.Add(styleDropdown);

        BindProperty<DropdownField, string, HyImageGeneratingNode>(sizeDropdown.name, nameof(node.size));
        BindProperty<DropdownField, string, HyImageGeneratingNode>(styleDropdown.name, nameof(node.style),
            OnStyleChanged);
        
        // show revised prompt
        var divider = new VisualElement();
        divider.AddToClassList("control-container-divider");
        var foldout = new Foldout()
        {
            text = "Output Revised Prompt", name = "revisedPromptFoldout"
        };
        revisedPromptField = new TextField(-1, true, false, '*')
        {
            name = "revisedPromptField", isReadOnly = true, value = node.revisedPrompt
        };
        foldout.Add(revisedPromptField);
        controlsContainer.Add(divider);
        controlsContainer.Add(foldout);
        node.onProcessed += OnRevisedPromptChanged;

        base.Enable();
        RefreshExpandedState();
    }

    void OnStyleChanged(ChangeEvent<string> evt)
    {
        node.style = evt.newValue == styleChoices[0] ? string.Empty : evt.newValue;
        NotifyNodeChanging();
    }

    void OnRevisedPromptChanged()
    {
        revisedPromptField.value = node.revisedPrompt;
    }
}