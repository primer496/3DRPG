namespace UnityEngine.AIGraph
{
    /// <summary>
    /// a wrapper to start/stop coroutine in runtime using Monobehaviour
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager _instance;
        private static readonly object _lock = new object();

        public static CoroutineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            GameObject obj = new GameObject("CoroutineManager");
                            _instance = obj.AddComponent<CoroutineManager>();
                            DontDestroyOnLoad(obj);
                        }
                    }
                }
                return _instance;
            }
        }

        //public Coroutine StartCoroutineWrapper(IEnumerator coroutine)
        //{
        //    return StartCoroutine(coroutine);
        //}

        //public void StopCoroutineWrapper(Coroutine coroutine)
        //{
        //    StopCoroutine(coroutine);
        //}
    }
}