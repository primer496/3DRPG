#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

namespace UnityEngine.AIGraph
{
    static class EditorUtilities
    {
        public static string OpenFile(string title, string directory, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, directory, extension);
#else
            return "";
#endif
        }

        public static string[] OpenFilesInFolder(string title, string directory, string extension)
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFolderPanel(title, directory, "");
            if (path != "" && Directory.Exists(path))
            {
                string pattern = string.Format("*.{0}", extension);
                string[] files = Directory.GetFiles(path, pattern);
                return files;
            }
            else
            {
                return new string[] { };
            }
#else
            return new string[] { };
#endif
        }
    }
}