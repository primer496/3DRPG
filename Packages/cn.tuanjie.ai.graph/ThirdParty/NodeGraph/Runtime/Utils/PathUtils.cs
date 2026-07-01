using System.IO;
using UnityEngine;

namespace GraphProcessor
{
    public static class PathUtils
    {
        public static string GRAPH_OUT_PATH = 
#if UNITY_EDITOR
            "Assets/Resources/TJAIGraph";
#else
            Application.persistentDataPath;
#endif
        /// <summary>
        /// create directory recursively
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) || Directory.Exists(path))
                return;
            CreateDirectory(Path.GetDirectoryName(path));
            Directory.CreateDirectory(path);
        }
    }
}