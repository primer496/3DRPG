using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HotUpdate.Editor
{
    /// <summary>
    /// 首次导入时自动创建并配置 HybridCLRSettings.asset。
    /// </summary>
    [InitializeOnLoad]
    public static class HybridCLRSettingsCreator
    {
        static HybridCLRSettingsCreator()
        {
            EditorApplication.delayCall += Configure;
        }

        private static void Configure()
        {
            var settings = HybridCLRSettings.LoadOrCreate();

            // Configure hot update assembly definitions
            var hotUpdateAsmdef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(
                "Assets/HotUpdate/HotUpdate.asmdef");

            if (hotUpdateAsmdef != null)
            {
                settings.hotUpdateAssemblyDefinitions = new[] { hotUpdateAsmdef };
                // Only use hotUpdateAssemblyDefinitions, NOT hotUpdateAssemblies (avoids duplicate)
                Debug.Log("[HotUpdateSetup] HotUpdate.asmdef assigned to HybridCLR settings.");
            }
            else
            {
                Debug.LogWarning("[HotUpdateSetup] HotUpdate.asmdef not found, skipping asmdef config.");
            }

            HybridCLRSettings.Save();
            Debug.Log("[HotUpdateSetup] HybridCLRSettings.asset configured and saved.");
        }
    }
}
