#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.AIGraph
{
    /// <summary>
    /// adaptable with urp and hdrp
    /// </summary>
    public class PipelineDefinesSetter : AssetPostprocessor
    {
        private static HashSet<string> sInstalled = new HashSet<string>();
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            UpdatePipelineDefines();
        }

        public static void UpdatePipelineDefines()
        {
            var urpInstalled = IsPackageInstalled("com.unity.render-pipelines.universal");
            var hdrpInstalled = IsPackageInstalled("com.unity.render-pipelines.high-definition");

            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var definesList = defines.Split(';').ToList();

            UpdateDefineSymbol("HAS_UNITY_URP", urpInstalled, definesList);
            UpdateDefineSymbol("HAS_UNITY_HDRP", hdrpInstalled, definesList);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup,
                string.Join(";", definesList.Distinct().ToArray()));
        }

        private static bool IsPackageInstalled(string packageName)
        {
            if (sInstalled.Contains(packageName))
                return true;
            try
            {
                var packageRequest = PackageManager.Client.List(true);
                while (!packageRequest.IsCompleted) System.Threading.Thread.Sleep(100);

                if (packageRequest.Status == PackageManager.StatusCode.Success)
                {
                    var installed =  packageRequest.Result.Any(package => package.name == packageName);
                    if (installed)
                        sInstalled.Add(packageName);
                    return installed;
                }
            }
            catch
            {
                // 忽略错误，返回false
            }

            return false;
        }

        private static void UpdateDefineSymbol(string symbol, bool enabled, List<string> definesList)
        {
            if (enabled && !definesList.Contains(symbol))
            {
                definesList.Add(symbol);
            }
            else if (!enabled && definesList.Contains(symbol))
            {
                definesList.Remove(symbol);
            }
        }
    }
}
#endif