using System.Collections;
using UnityEngine.AIGraph.Backend;

public class TokenDataModel
{
    public event System.Action<int> TokenCountChanged;

    private int _tokenRemaining = -1;

    public int TokenRemaining
    {
        get => _tokenRemaining;
        private set
        {
            if (_tokenRemaining != value)
            {
                _tokenRemaining = value;
                TokenCountChanged?.Invoke(_tokenRemaining);
            }
        }
    }

    public IEnumerator FetchTokenCountAsync()
    {
        yield return GetTokenCountFromBackendAsync();
    }

    public void UseToken(int amount)
    {
        TokenRemaining -= amount;
    }

    public void UpdateToken(int tokenRemain)
    {
        TokenRemaining = tokenRemain;
    }

    private IEnumerator GetTokenCountFromBackendAsync()
    {
        var getArtifactRestCall = new GetTokenRestCall(ServerConfig.serverConfig, 3);
        yield return getArtifactRestCall.MakeServerRequest(null);
        TokenRemaining = getArtifactRestCall.Result.credits.currentCredits;
    }
}