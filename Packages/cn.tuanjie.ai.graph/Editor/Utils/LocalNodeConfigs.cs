using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    // [CreateAssetMenu(fileName = "LocalNodeConfigs.asset", menuName = "TJAI/LocalNodeConfigs")]
    internal class LocalNodeConfigs : ScriptableObject
    {
        public SerializedDictionary<string, string> nodeTags = new();
        static LocalNodeConfigs _instance;

        public static LocalNodeConfigs nodeConfigs
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<LocalNodeConfigs>("configs/LocalNodeConfigs");
                return _instance;
            }
        }
    }
}