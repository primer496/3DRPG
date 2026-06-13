using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using DialogueSystem.Presenter;
using DialogueSystem.Model;
//锟斤拷锟斤拷图锟姐，锟斤拷锟斤拷锟斤拷锟経I锟斤拷锟斤拷锟斤拷锟绞撅拷徒锟斤拷锟斤拷锟斤拷峁╋拷涌诠锟絇resenter锟斤拷锟斤拷锟皆革拷锟斤拷UI状态锟斤拷同时通锟斤拷锟铰硷拷锟斤拷锟矫伙拷锟斤拷锟诫传锟捷革拷Presenter锟斤拷锟叫达拷锟斤拷
public class DialogueUIController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Events")]
    public UnityEvent<int> OnOptionClicked = new UnityEvent<int>();
    public UnityEvent OnContinueClicked = new UnityEvent();
    public UnityEvent OnDialogueTextClicked = new UnityEvent();

    private VisualElement root;
    private VisualElement optionsContainer;
    private Button option1;
    private Button option2;
    private Button option3;
    private VisualElement dialogueBox;
    private Label characterName;
    private Label dialogueText;
    private VisualElement continueIndicator;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        root = uiDocument.rootVisualElement;
        BindComponents();
        RegisterEvents();
    }

    private void Start()
    {
        // 鍒濆闅愯棌锛歎I Toolkit 鐢� display:none 浠ｆ浛 SetActive(false)
        // 鏀惧湪 Start 鑰岄潪 Awake锛岀‘淇濇墍鏈夌粍浠跺垵濮嬪寲瀹屾瘯鍚庡啀璁剧疆
        ShowDialogue(false);
    }

    public void BindPresenter(DialoguePresenter presenter)
    {
        OnOptionClicked.AddListener(presenter.SelectOption);
        OnContinueClicked.AddListener(presenter.ContinueDialogue);
        OnDialogueTextClicked.AddListener(presenter.ContinueDialogue);
    }

    public void UnbindPresenter(DialoguePresenter presenter)
    {
        OnOptionClicked.RemoveListener(presenter.SelectOption);
        OnContinueClicked.RemoveListener(presenter.ContinueDialogue);
        OnDialogueTextClicked.RemoveListener(presenter.ContinueDialogue);
    }

    public void SetOptionState(int index, bool show, string text)
    {
        Button btn = GetOptionButton(index);
        if (btn != null)
        {
            btn.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                btn.text = text;
            }
        }
    }
    //锟斤拷UI锟斤拷锟斤拷锟绞癸拷锟経Query锟斤拷询锟斤拷锟斤拷锟芥常锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟�
    private void BindComponents()
    {
        optionsContainer = root.Q<VisualElement>("OptionsContainer");
        option1 = root.Q<Button>("Option1");
        option2 = root.Q<Button>("Option2");
        option3 = root.Q<Button>("Option3");
        dialogueBox = root.Q<VisualElement>("DialogueBox");
        characterName = root.Q<Label>("CharacterName");
        dialogueText = root.Q<Label>("DialogueText");
        continueIndicator = root.Q<VisualElement>("ContinueIndicator");
    }
    //注锟斤拷UI锟铰硷拷锟斤拷锟斤拷锟矫伙拷锟侥碉拷锟斤拷锟斤拷锟阶拷锟轿拷录锟斤拷锟斤拷锟斤拷莞锟絇resenter锟斤拷锟叫达拷锟斤拷
    private void RegisterEvents()
    {
        option1?.RegisterCallback<ClickEvent>(evt => OnOptionClicked.Invoke(0));
        option2?.RegisterCallback<ClickEvent>(evt => OnOptionClicked.Invoke(1));
        option3?.RegisterCallback<ClickEvent>(evt => OnOptionClicked.Invoke(2));

        continueIndicator?.RegisterCallback<ClickEvent>(evt => OnContinueClicked.Invoke());

        dialogueText?.RegisterCallback<ClickEvent>(evt => OnDialogueTextClicked.Invoke());
    }

    public void SetCharacterName(string name)
    {
        characterName.text = name;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void SetOptionText(int index, string text)
    {
        GetOptionButton(index).text = text;
    }

    public void ShowOptions(bool show)
    {
        optionsContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ShowContinueIndicator(bool show)
    {
        continueIndicator.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ShowDialogueBox(bool show)
    {
        dialogueBox.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ShowDialogue(bool show)
    {
        root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private Button GetOptionButton(int index)
    {
        return index switch
        {
            0 => option1,
            1 => option2,
            2 => option3,
            _ => null
        };
    }
}
