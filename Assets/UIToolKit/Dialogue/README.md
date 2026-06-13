# 对话系统使用说明

## 系统概述
这是一套为团结引擎1.8.5的3D RPG项目设计的对话系统UIToolkit代码，包含UXML布局文件和USS样式文件。

## 文件结构
- `DialogueSystem.uxml` - 对话系统布局文件
- `DialogueSystem.uss` - 对话系统样式文件
- `arrow_left.png` - 上一条按钮图标
- `close.png` - 关闭按钮图标
- `continue.png` - 继续提示图标

## 如何挂载
1. 在Unity场景中创建一个UI Document对象
2. 将`DialogueSystem.uxml`文件拖放到UI Document的Source Asset字段
3. 创建一个C#脚本作为对话系统的控制器，挂载到UI Document对象上

## 如何监听事件
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class DialogueSystemController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    
    // 事件定义
    public UnityEvent OnPreviousButtonClicked = new UnityEvent();
    public UnityEvent OnCloseButtonClicked = new UnityEvent();
    public UnityEvent<int> OnOptionSelected = new UnityEvent<int>();
    
    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // 获取按钮元素
        Button previousButton = root.Q<Button>("PreviousButton");
        Button closeButton = root.Q<Button>("CloseButton");
        Button[] optionButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            optionButtons[i] = root.Q<Button>($"Option{i+1}");
        }
        
        // 绑定事件
        previousButton.clicked += () => OnPreviousButtonClicked.Invoke();
        closeButton.clicked += () => OnCloseButtonClicked.Invoke();
        
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].clicked += () => OnOptionSelected.Invoke(index);
        }
    }
    
    // 设置对话文本
    public void SetDialogueText(string text)
    {
        Label dialogueText = root.Q<Label>("DialogueText");
        dialogueText.text = text;
    }
    
    // 设置角色名称
    public void SetCharacterName(string name)
    {
        Label characterName = root.Q<Label>("CharacterName");
        characterName.text = name;
    }
    
    // 设置选项
    public void SetOptions(string[] options)
    {
        VisualElement optionsContainer = root.Q<VisualElement>("OptionsContainer");
        Button[] optionButtons = new Button[4];
        
        for (int i = 0; i < 4; i++)
        {
            optionButtons[i] = root.Q<Button>($"Option{i+1}");
            if (i < options.Length)
            {
                optionButtons[i].text = options[i];
                optionButtons[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                optionButtons[i].style.display = DisplayStyle.None;
            }
        }
        
        // 显示选项容器
        VisualElement dialogueBox = root.Q<VisualElement>("DialogueBox");
        dialogueBox.RemoveFromClassList("text-playing");
        dialogueBox.RemoveFromClassList("waiting-for-input");
        dialogueBox.AddToClassList("showing-options");
    }
    
    // 切换状态
    public void SetState(DialogueState state)
    {
        VisualElement dialogueBox = root.Q<VisualElement>("DialogueBox");
        
        // 移除所有状态类
        dialogueBox.RemoveFromClassList("text-playing");
        dialogueBox.RemoveFromClassList("waiting-for-input");
        dialogueBox.RemoveFromClassList("showing-options");
        
        // 添加对应状态类
        switch (state)
        {
            case DialogueState.TextPlaying:
                dialogueBox.AddToClassList("text-playing");
                break;
            case DialogueState.WaitingForInput:
                dialogueBox.AddToClassList("waiting-for-input");
                break;
            case DialogueState.ShowingOptions:
                dialogueBox.AddToClassList("showing-options");
                break;
        }
    }
}

public enum DialogueState
{
    TextPlaying,
    WaitingForInput,
    ShowingOptions
}
```

## 如何切换状态
使用`SetState`方法切换对话系统的状态：
- `DialogueState.TextPlaying` - 文本播放中，隐藏继续提示
- `DialogueState.WaitingForInput` - 等待玩家输入，显示继续提示
- `DialogueState.ShowingOptions` - 显示选项，隐藏继续提示

## 样式扩展
如果需要修改配色或尺寸，只需修改`DialogueSystem.uss`文件中的对应样式即可，无需改动UXML布局结构。

## 性能优化
- 使用了模块化分层设计，每个模块可独立修改样式
- 避免了过度特效，所有样式都可在Runtime流畅运行
- 采用了高效的布局方式，无布局抖动
