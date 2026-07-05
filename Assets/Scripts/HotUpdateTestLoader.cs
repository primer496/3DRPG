using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

/// <summary>
/// 热更新验证加载器（AOT 侧，Assembly-CSharp）。
/// 统一走 Addressables 一条管线：DLL + 预制体都从同一个远程地址加载。
/// 
/// 运行时流程：
/// 1. Addressables.InitializeAsync
/// 2. 通过 Addressables 下载 HotUpdate.dll.bytes → Assembly.Load
/// 3. 通过 Addressables 加载预制体 → Instantiate → HotUpdateLabelChanger.Start()
/// </summary>
public class HotUpdateTestLoader : MonoBehaviour
{
    [Header("Addressable Keys")]
    [Tooltip("热更 DLL 在 Addressables 中的 Key")]
    public string dllKey = "HotUpdateDll";

    [Tooltip("热更预制体在 Addressables 中的 Key")]
    public string prefabKey = "TestHotUpdatePrefab";

    [Header("Status Label")]
    [Tooltip("显示状态的 Label 名称")]
    public string statusLabelName = "hot-update-label";

    private AsyncOperationHandle<TextAsset> _dllHandle;
    private AsyncOperationHandle<GameObject> _prefabHandle;
    private GameObject _instantiated;

    private IEnumerator Start()
    {
        SetStatusLabel("初始化 Addressables...");
        Debug.Log("[HotUpdateTestLoader] Initializing Addressables...");
        var initOp = Addressables.InitializeAsync();
        yield return new WaitUntil(() => initOp.IsDone);
        Debug.Log("[HotUpdateTestLoader] Addressables initialized.");

        // Step 1: Load DLL via Addressables
        SetStatusLabel("正在下载热更 DLL...");
        Debug.Log("[HotUpdateTestLoader] Loading HotUpdateDll via Addressables...");
        _dllHandle = Addressables.LoadAssetAsync<TextAsset>(dllKey);
        yield return new WaitUntil(() => _dllHandle.IsDone);

        if (_dllHandle.Status != AsyncOperationStatus.Succeeded)
        {
            SetStatusLabel($"失败: DLL加载 - {_dllHandle.OperationException?.Message}");
            Debug.LogError($"[HotUpdateTestLoader] DLL load failed: {_dllHandle.OperationException}");
            yield break;
        }

        byte[] dllBytes = _dllHandle.Result.bytes;
        Debug.Log($"[HotUpdateTestLoader] DLL loaded: {dllBytes.Length} bytes.");

        // Step 2: Assembly.Load
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

        // Step 3: Load prefab via Addressables
        SetStatusLabel("正在加载热更预制体...");
        Debug.Log("[HotUpdateTestLoader] Loading prefab via Addressables...");
        _prefabHandle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        yield return new WaitUntil(() => _prefabHandle.IsDone);

        if (_prefabHandle.Status != AsyncOperationStatus.Succeeded)
        {
            SetStatusLabel("失败: 预制体加载失败");
            Debug.LogError($"[HotUpdateTestLoader] Prefab load failed: {_prefabHandle.OperationException}");
            yield break;
        }

        // Step 4: Instantiate
        SetStatusLabel("正在实例化预制体...");
        _instantiated = Instantiate(_prefabHandle.Result, Vector3.zero, Quaternion.identity);
        Debug.Log("[HotUpdateTestLoader] Prefab instantiated.");

        SetStatusLabel("热更流程完成！");
    }

    private void OnDestroy()
    {
        if (_instantiated != null)
            Destroy(_instantiated);
        if (_prefabHandle.IsValid())
            Addressables.Release(_prefabHandle);
        if (_dllHandle.IsValid())
            Addressables.Release(_dllHandle);
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
