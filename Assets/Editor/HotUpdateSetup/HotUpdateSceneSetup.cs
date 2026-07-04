using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace HotUpdate.Editor
{
    /// <summary>
    /// 一键创建热更新验证所需的场景 GameObject。
    /// 菜单: Tools → HotUpdate → Setup Scene
    /// </summary>
    public static class HotUpdateSceneSetup
    {
        private const string PanelSettingsPath = "Assets/UI Toolkit/PanelSettings.asset";
        private const string UxmlPath = "Assets/HotUpdateAssets/HotUpdateTestUI.uxml";
        private const string UssPath = "Assets/HotUpdateAssets/HotUpdateTestUI.uss";

        [MenuItem("Tools/HotUpdate/Setup Scene")]
        public static void Setup()
        {
            var scene = EditorSceneManager.GetActiveScene();

            // 1. Create HotUpdateTestUI (UIDocument for verification label)
            var uiGo = GameObject.Find("HotUpdateTestUI");
            if (uiGo == null)
            {
                uiGo = new GameObject("HotUpdateTestUI");
                var uiDoc = uiGo.AddComponent<UIDocument>();
                uiDoc.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
                uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
                uiDoc.sortingOrder = 100; // On top of other UI
                Undo.RegisterCreatedObjectUndo(uiGo, "Create HotUpdateTestUI");
                Debug.Log("[HotUpdateSetup] Created HotUpdateTestUI GameObject.");
            }
            else
            {
                Debug.Log("[HotUpdateSetup] HotUpdateTestUI already exists, skipped.");
            }

            // 2. Create HotUpdateTestLoader (Loader MonoBehaviour)
            var loaderGo = GameObject.Find("HotUpdateTestLoader");
            if (loaderGo == null)
            {
                loaderGo = new GameObject("HotUpdateTestLoader");
                loaderGo.AddComponent<HotUpdateTestLoader>();
                Undo.RegisterCreatedObjectUndo(loaderGo, "Create HotUpdateTestLoader");
                Debug.Log("[HotUpdateSetup] Created HotUpdateTestLoader GameObject.");
            }
            else
            {
                // Ensure it has the component
                if (!loaderGo.TryGetComponent<HotUpdateTestLoader>(out _))
                {
                    loaderGo.AddComponent<HotUpdateTestLoader>();
                    Debug.Log("[HotUpdateSetup] Added HotUpdateTestLoader component to existing GO.");
                }
                else
                {
                    Debug.Log("[HotUpdateSetup] HotUpdateTestLoader already exists, skipped.");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[HotUpdateSetup] Scene setup complete.");
        }

        [MenuItem("Tools/HotUpdate/Create Prefab")]
        public static void CreatePrefab()
        {
            const string prefabPath = "Assets/HotUpdateAssets/TestHotUpdatePrefab.prefab";

            // Check if already exists
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log("[HotUpdateSetup] TestHotUpdatePrefab already exists. Delete it first if you want to recreate.");
                return;
            }

            // Create the prefab from a temporary GameObject
            var tempGo = new GameObject("TestHotUpdatePrefab");
            var changer = tempGo.AddComponent<HotUpdate.HotUpdateLabelChanger>();
            changer.Message = "热更新成功！";
            changer.LabelColor = Color.green;

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);
            Object.DestroyImmediate(tempGo);

            Debug.Log($"[HotUpdateSetup] Created prefab at {prefabPath}");

            // Also register as Addressable
            RegisterAsAddressable(prefab);
        }

        [MenuItem("Tools/HotUpdate/Register As Addressable")]
        public static void RegisterExistingPrefabAsAddressable()
        {
            const string prefabPath = "Assets/HotUpdateAssets/TestHotUpdatePrefab.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("[HotUpdateSetup] Prefab not found. Run 'Create Prefab' first.");
                return;
            }
            RegisterAsAddressable(prefab);
        }

        private static void RegisterAsAddressable(Object asset)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[HotUpdateSetup] AddressableAssetSettings not found.");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(asset);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.FindAssetEntry(guid);

            if (entry != null)
            {
                Debug.Log($"[HotUpdateSetup] Asset already registered as Addressable: {assetPath} (group: {entry.parentGroup.Name})");
                return;
            }

            // Add to "Packed Assets" group (remote), fallback to Default Local Group
            var group = settings.FindGroup("Packed Assets") ?? settings.DefaultGroup;
            entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = "TestHotUpdatePrefab";
            entry.SetLabel("hotupdate", true);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            Debug.Log($"[HotUpdateSetup] Registered '{assetPath}' as Addressable with address='TestHotUpdatePrefab' in group='{group.Name}'.");
        }

        // Also allow removing the test objects
        [MenuItem("Tools/HotUpdate/Cleanup Scene")]
        public static void Cleanup()
        {
            var scene = EditorSceneManager.GetActiveScene();
            bool changed = false;

            var uiGo = GameObject.Find("HotUpdateTestUI");
            if (uiGo != null) { Undo.DestroyObjectImmediate(uiGo); changed = true; }

            var loaderGo = GameObject.Find("HotUpdateTestLoader");
            if (loaderGo != null) { Undo.DestroyObjectImmediate(loaderGo); changed = true; }

            if (changed) EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[HotUpdateSetup] Cleanup complete.");
        }

        [MenuItem("Tools/HotUpdate/Build Addressables Only")]
        public static void BuildAddressablesOnly()
        {
            var projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            var buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();

            Debug.Log("[HotUpdateSetup] === Build Addressables ===");
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                AddressableAssetSettings.BuildPlayerContent(out var buildResult);
                Debug.Log($"[HotUpdateSetup] Addressables build completed: {buildResult}");
            }

            // Copy bundles & catalog to ServerData for HTTP serving
            var serverDir = System.IO.Path.Combine(projectRoot, "ServerData", buildTarget);
            System.IO.Directory.CreateDirectory(serverDir);

            // Find and copy bundles
            var tempAddrDir = System.IO.Path.Combine(projectRoot, "Temp", "com.unity.addressables");
            var libAddrDir = System.IO.Path.Combine(projectRoot, "Library", "com.unity.addressables", "aa", "Windows");

            // Copy bundles
            if (System.IO.Directory.Exists(tempAddrDir))
            {
                foreach (var f in System.IO.Directory.GetFiles(tempAddrDir, "*.bundle"))
                    System.IO.File.Copy(f, System.IO.Path.Combine(serverDir, System.IO.Path.GetFileName(f)), true);
            }

            // Copy catalog & settings
            if (System.IO.Directory.Exists(libAddrDir))
            {
                foreach (var f in System.IO.Directory.GetFiles(libAddrDir, "*.json"))
                    System.IO.File.Copy(f, System.IO.Path.Combine(serverDir, System.IO.Path.GetFileName(f)), true);
            }

            Debug.Log($"[HotUpdateSetup] Files copied to {serverDir}");
            Debug.Log($"[HotUpdateSetup] Start HTTP server: python -m http.server 8080 -d {serverDir}/../..");
        }

        [MenuItem("Tools/HotUpdate/Build All (GenerateAll + Addressables + Copy DLL)")]
        public static void BuildAll()
        {
            // Step 1: HybridCLR GenerateAll
            Debug.Log("[HotUpdateSetup] === Step 1: HybridCLR GenerateAll ===");
            HybridCLR.Editor.Commands.CompileDllCommand.CompileDllActiveBuildTarget();
            HybridCLR.Editor.Commands.Il2CppDefGeneratorCommand.GenerateIl2CppDef();
            // Copy stripped AOT assemblies (required before MethodBridge)
            HybridCLR.Editor.Commands.StripAOTDllCommand.GenerateStripedAOTDlls(EditorUserBuildSettings.activeBuildTarget);
            HybridCLR.Editor.Commands.MethodBridgeGeneratorCommand.GenerateMethodBridgeAndReversePInvokeWrapper();
            HybridCLR.Editor.Commands.AOTReferenceGeneratorCommand.GenerateAOTGenericReference(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("[HotUpdateSetup] HybridCLR GenerateAll completed.");

            // Step 2: Build Addressables
            Debug.Log("[HotUpdateSetup] === Step 2: Build Addressables ===");
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                AddressableAssetSettings.BuildPlayerContent(out var buildResult);
                Debug.Log($"[HotUpdateSetup] Addressables build completed. Result: {buildResult}");
            }

            // Step 3: Copy HotUpdate.dll to server directory
            Debug.Log("[HotUpdateSetup] === Step 3: Copy HotUpdate.dll ===");
            var projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            var buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            var srcDll = System.IO.Path.Combine(projectRoot, "HybridCLRData", "HotUpdateDlls", buildTarget, "HotUpdate.dll");
            var destDir = System.IO.Path.Combine(projectRoot, "ServerData", buildTarget);
            var destDll = System.IO.Path.Combine(destDir, "HotUpdate.dll");

            if (System.IO.File.Exists(srcDll))
            {
                System.IO.Directory.CreateDirectory(destDir);
                System.IO.File.Copy(srcDll, destDll, true);
                Debug.Log($"[HotUpdateSetup] Copied HotUpdate.dll to {destDll}");
            }
            else
            {
                Debug.LogError($"[HotUpdateSetup] HotUpdate.dll not found at: {srcDll}");
                // Try alternate path
                var altSrc = System.IO.Path.Combine(projectRoot, "HybridCLRData", "HotUpdateDlls", "HotUpdate.dll");
                if (System.IO.File.Exists(altSrc))
                {
                    System.IO.Directory.CreateDirectory(destDir);
                    System.IO.File.Copy(altSrc, destDll, true);
                    Debug.Log($"[HotUpdateSetup] Copied HotUpdate.dll (alt path) to {destDll}");
                }
            }

            Debug.Log("[HotUpdateSetup] === Build All Complete ===");
            Debug.Log($"[HotUpdateSetup] Next: Start HTTP server: python -m http.server 8080 -d ServerData");
        }

        [MenuItem("Tools/HotUpdate/Configure Remote Profile")]
        public static void ConfigureRemoteProfile()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[HotUpdateSetup] AddressableAssetSettings not found.");
                return;
            }

            var profileSettings = settings.profileSettings;
            var profileId = settings.activeProfileId;

            // Set Remote.BuildPath = ServerData/[BuildTarget]
            profileSettings.SetValue(profileId, "Remote.BuildPath",
                $"[UnityEditor.EditorUserBuildSettings.activeBuildTarget]");
            // Set Remote.LoadPath = http://localhost:8080/[BuildTarget]
            profileSettings.SetValue(profileId, "Remote.LoadPath",
                $"http://localhost:8080/[UnityEditor.EditorUserBuildSettings.activeBuildTarget]");

            // Ensure "Packed Assets" group uses remote paths
            var packedGroup = settings.FindGroup("Packed Assets");
            if (packedGroup != null)
            {
                // Set build path to RemoteBuildPath
                var buildSchema = packedGroup.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
                if (buildSchema != null)
                {
                    buildSchema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
                    buildSchema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.ProfileModified, settings, true);
            Debug.Log("[HotUpdateSetup] Remote profile configured: LoadPath=http://localhost:8080/[BuildTarget]");
        }
    }
}
