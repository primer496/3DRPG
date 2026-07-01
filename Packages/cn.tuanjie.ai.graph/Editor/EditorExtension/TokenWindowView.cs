using GraphProcessor;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class TokenWindowView : GraphElement
{
    private readonly TokenDataModel _dataModel;
    private Label _tokenLabel;
    private Button _payButton;

    public TokenWindowView(BaseGraphView graphView, TokenDataModel dataModel)
    {
        _dataModel = dataModel;
        InitializeUI();

        // 订阅数据变化事件
        _dataModel.TokenCountChanged += UpdateTokenDisplay;

        // 启动数据获取
        EditorCoroutineUtility.StartCoroutine(
            _dataModel.FetchTokenCountAsync(),
            graphView
        );
    }

    private void InitializeUI()
    {
        //_payButton = new Button { name = "PayButton", text = "充值" };
        //_payButton.RegisterCallback<ClickEvent>(OnPayButtonClicked);
        //Add(_payButton);
        _tokenLabel = new Label { name = "TokenLabel" };
        _tokenLabel.RegisterCallback<ClickEvent>(OnTokenLabelClicked);
        Add(_tokenLabel);
        var legalLabel = new Label("内容由AI生成，请您仔细甄别！您也可以分享反馈，帮助我们学习与改进。") { name = "LegalTitle" };
        Add(legalLabel);
        UpdateTokenDisplay(_dataModel.TokenRemaining);
    }

    private void UpdateTokenDisplay(int tokenCount)
    {
        if (tokenCount >= 0)
            _tokenLabel.text = $"Tokens: {tokenCount}";
        else
            _tokenLabel.text = "Requesting tokens...";
    }

    public void OnTokenUsed(int amount)
    {
        // 通过数据模型修改数据
        _dataModel.UseToken(amount);
    }

    ~TokenWindowView()
    {
        // 取消事件订阅
        if (_dataModel != null)
            _dataModel.TokenCountChanged -= UpdateTokenDisplay;
    }

    private void OnPayButtonClicked(ClickEvent evt)
    {
        if (!EditorUtility.DisplayDialog("免费试用",
        "免费试用阶段暂无充值入口", "OK"))
        {
            return;
        }
        // Application.OpenURL(UnityEngine.AIGraph.GlobalConstants.payUrl);
    }
    private void OnTokenLabelClicked(ClickEvent evt)
    {
        if (!EditorUtility.DisplayDialog("免费试用",
        "免费试用阶段暂无充值入口", "OK"))
        {
            return;
        }
        // Application.OpenURL(UnityEngine.AIGraph.GlobalConstants.tokenInfoUrl);
    }
}

//public class TokenWindow : GraphElement
//{
//    protected BaseGraphView graphView;
//    private Label tokenLabel;
//    private int tokenRemaining = -1; // 初始值为 -1，表示未查询过  


//    public TokenWindow(BaseGraphView graphView)
//    {
//        this.graphView = graphView;
//        InitializeUI();
//        EditorCoroutineUtility.StartCoroutine(RequestTokenCountFromBackendAsync(), graphView);
//        RequestTokenCountFromBackendAsync();
//    }

//    private void InitializeUI()
//    {
//        // 设置标签样式  
//        tokenLabel = new Label();
//        tokenLabel.style.position = Position.Absolute;
//        tokenLabel.style.top = 10;
//        tokenLabel.style.right = 10;
//        tokenLabel.style.backgroundColor = new Color(0, 0, 0, 0.5f);
//        tokenLabel.style.color = Color.white;

//        // 将标签添加到当前元素  
//        this.Add(tokenLabel);

//        // 初始化显示  
//        UpdateTokenDisplay();
//    }

//    private IEnumerator RequestTokenCountFromBackendAsync()
//    {
//        yield return GetTokenCountFromBackendAsync();
//        UpdateTokenDisplay();
//    }

//    private IEnumerator GetTokenCountFromBackendAsync()
//    {
//        var getArtifactRestCall = new GetTokenRestCall(ServerConfig.serverConfig, 3);
//        yield return getArtifactRestCall.MakeServerRequest(null);
//        tokenRemaining = getArtifactRestCall.Result.credits.currentCredits;
//    }

//    private void UpdateTokenDisplay()
//    {
//        if (tokenRemaining >= 0)
//        {
//            tokenLabel.text = $"Tokens: {tokenRemaining}";
//        }
//        else
//        {
//            tokenLabel.text = "Requesting tokens...";
//        }
//    }

//    public void OnTokenUsed(int amount)
//    {
//        // 减少 token 数量并更新显示  
//        tokenRemaining -= amount;
//        UpdateTokenDisplay();
//    }
//}