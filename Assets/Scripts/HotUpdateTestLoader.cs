using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

/// <summary>
/// 热更新验证加载器（AOT 侧，Assembly-CSharp）。
/// 
/// 运行时流程：
/// 1. 从本地 HTTP 服务器下载 HotUpdate.dll
/// 2. Assembly.Load 加载热更程序集
/// 3. Addressables 加载预制体
/// 4. 实例化预制体 → HotUpdateLabelChanger.Start() 执行
/// </summary>
public class HotUpdateTestLoader : MonoBehaviour
{
    [Header("Remote Config")]
    [Tooltip("热更 DLL 的远程 URL")]
    public string hotUpdateDllUrl = "http://localhost:8000/StandaloneWindows64/HotUpdate.dll";

    [Tooltip("Addressables 中预制体的 Key")]
    public string prefabKey = "TestHotUpdatePrefab";

    [Header("Status Label")]
    [Tooltip("显示状态的 Label 名称")]
    public string statusLabelName = "hot-update-label";

    private IEnumerator Start()
    {
        SetStatusLabel("初始化中...");

        // Step 1: Download HotUpdate.dll from remote
        SetStatusLabel("正在下载热更 DLL...");
        Debug.Log("[HotUpdateTestLoader] Downloading HotUpdate.dll...");

        byte[] dllBytes = null;
        using (var req = UnityWebRequest.Get(hotUpdateDllUrl))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                SetStatusLabel($"失败: DLL下载 - {req.error}");
                Debug.LogError($"[HotUpdateTestLoader] DLL download failed: {req.error}");
                yield break;
            }
            dllBytes = req.downloadHandler.data;
        }
        Debug.Log($"[HotUpdateTestLoader] Downloaded {dllBytes.Length} bytes.");

        // Step 2: Load the assembly
        SetStatusLabel("正在加载热更程序集...");
        Assembly hotUpdateAssembly;
        try
        {
            hotUpdateAssembly = Assembly.Load(dllBytes);
            Debug.Log($"[HotUpdateTestLoader] Assembly loaded: {hotUpdateAssembly.FullName}");
        }
        catch (Exception ex)
        {
            SetStatusLabel($"失败: 程序集加载 - {ex.GetType().Name}");
            Debug.LogError($"[HotUpdateTestLoader] Assembly load failed: {ex}");
            yield break;
        }

        // Step 3: Initialize Addressables
        SetStatusLabel("正在初始化 Addressables...");
        var initOp = Addressables.InitializeAsync();
        yield return new WaitUntil(() => initOp.IsDone);
        Debug.Log("[HotUpdateTestLoader] Addressables initialized.");

        // Step 4: Load prefab from Addressables
        SetStatusLabel("正在加载热更预制体...");
        var loadOp = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        yield return new WaitUntil(() => loadOp.IsDone);

        if (loadOp.Status != AsyncOperationStatus.Succeeded)
        {
            SetStatusLabel("失败: 预制体加载失败");
            Debug.LogError($"[HotUpdateTestLoader] Failed to load prefab '{prefabKey}': {loadOp.OperationException}");
            yield break;
        }

        // Step 5: Instantiate
        SetStatusLabel("正在实例化预制体...");
        Instantiate(loadOp.Result, Vector3.zero, Quaternion.identity);
        Debug.Log("[HotUpdateTestLoader] Prefab instantiated. HotUpdateLabelChanger should now run.");

        SetStatusLabel("热更流程完成！");
    }

    private void SetStatusLabel(string text)
    {
        var uiDocs = FindObjectsOfType<UIDocument>();
        foreach (var doc in uiDocs)
        {
            var label = doc.rootVisualElement?.Q<Label>(statusLabelName);
            if (label != null)
            {
                label.text = text;
                return;
            }
        }
    }
}
