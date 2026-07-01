using System;

namespace UnityEngine.AIGraph.Backend
{
    //[CreateAssetMenu(fileName = "ServerConfig.asset", menuName = "TJAI/Sprite/ServerConfig")]
    public class ServerConfig : ScriptableObject
    {
        [Flags]
        public enum EDebugMode
        {
            ArtifactDebugInfo = 1,
            SessionDebug = 1 << 2,
            OperatorDebug = 1 << 3,
            ForceUseSecretKey = 1 << 4
        }

        public string[] serverList;
        public int serverIndex;
        [SerializeField]
        string secretToken;
        public float webRequestPollRate = 1.0f;
        public int maxRetries = 3;
        public string server => serverList[serverIndex];

        [HideInInspector]
        public int model;
        [HideInInspector]
        public bool simulate;

        [SerializeField]
        EDebugMode m_DebugMode;
        public EDebugMode debugMode =>
#if UNITY_EDITOR
            UnityEditor.Unsupported.IsDeveloperMode() ? m_DebugMode : 0;
#else
            0;
#endif

        public static ServerConfig serverConfig
        {
            get
            {
                if (_serverConfig == null)
                    Initialize();
                return _serverConfig;
            }
        }
        private static ServerConfig _serverConfig;
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        private static void Initialize()
        {
            _serverConfig = Resources.Load<ServerConfig>("ServerConfig");
        }
    }
}