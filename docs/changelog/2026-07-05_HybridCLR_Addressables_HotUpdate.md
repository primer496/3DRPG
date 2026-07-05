# HybridCLR + Addressables 热更新验证报告

> 日期: 2026-07-05  
> 目标: 验证 Unity 中资源热更新(Addressables) + 代码热更新(HybridCLR) 的完整流程  
> 结果: ✅ 成功

---

## 一、架构概览

```
┌─────────────────────────────────────────────────────┐
│                    HTTP 服务器                        │
│  catalog.json / settings.json / *.bundle / DLL.bytes │
└──────────────────────┬──────────────────────────────┘
                       │ Addressables 统一管线
┌──────────────────────▼──────────────────────────────┐
│                   Player (IL2CPP)                     │
│  HotUpdateTestLoader (AOT)                            │
│    1. Addressables.InitializeAsync                    │
│    2. LoadAsset<TextAsset>("HotUpdateDll") → Assembly.Load │
│    3. LoadAsset<GameObject>("TestHotUpdatePrefab") → Instantiate │
└─────────────────────────────────────────────────────┘
```

- **HybridCLR**: 负责代码热更——将 C# 编译为 IL DLL，运行时 `Assembly.Load(byte[])` 解释执行
- **Addressables**: 负责资源热更——统一管理 DLL(.bytes) 和 prefab(.bundle) 的远程分发

---

## 二、项目结构

```
Assets/
├── HotUpdate/                          # 热更程序集
│   ├── HotUpdate.asmdef                #   程序集定义
│   ├── HotUpdateLabelChanger.cs        #   热更脚本：修改 UI Label
│   └── HotUpdate.dll.bytes             #   编译后的 DLL(由 HybridCLR 生成后覆盖)
├── HotUpdateAssets/
│   ├── TestHotUpdatePrefab.prefab      #   Addressable 预制体(挂载 HotUpdateLabelChanger)
│   ├── HotUpdateTestUI.uxml            #   验证用 UI(Label: "热更新前")
│   └── HotUpdateTestUI.uss             #   UI 样式
├── Scripts/
│   └── HotUpdateTestLoader.cs          #   AOT 侧加载器(coroutine 驱动)
├── link.xml                            #   IL2CPP Linker 安全兜底
ProjectSettings/
└── HybridCLRSettings.asset             #   HybridCLR 配置(指向 HotUpdate.asmdef)
```

---

## 三、关键设计决策

### 3.1 DLL 走 Addressables 统一管线

最初 DLL 通过 `UnityWebRequest` 单独下载，prefab 走 Addressables——两条下载通道。优化后**全部纳入 Addressables**，只需维护一套部署管线：

```csharp
// HotUpdateTestLoader.Start() — 协程驱动，不在 Update 里 await
IEnumerator Start() {
    // 1. 初始化
    var initOp = Addressables.InitializeAsync();
    yield return new WaitUntil(() => initOp.IsDone);

    // 2. 下载 DLL → 加载代码
    var dllOp = Addressables.LoadAssetAsync<TextAsset>("HotUpdateDll");
    yield return new WaitUntil(() => dllOp.IsDone);
    Assembly.Load(dllOp.Result.bytes);

    // 3. 加载预制体 → 实例化
    var prefabOp = Addressables.LoadAssetAsync<GameObject>("TestHotUpdatePrefab");
    yield return new WaitUntil(() => prefabOp.IsDone);
    Instantiate(prefabOp.Result);
}
```

### 3.2 加载顺序

DLL 必须在 prefab 之前加载，否则 Unity 反序列化 `HotUpdateLabelChanger` 组件时 `TypeNotFound`。

---

## 四、完整操作流程

### 首次搭建（一次性）

| 步骤 | 操作 | 位置 |
|---|---|---|
| 1 | 创建 HotUpdate 程序集(.asmdef) | `Assets/HotUpdate/` |
| 2 | 编写热更脚本 | `HotUpdateLabelChanger.cs` |
| 3 | 创建预制体，挂载热更脚本 | `TestHotUpdatePrefab.prefab` |
| 4 | 拖入 Addressables Group | Addressables Groups 窗口 → Packed Assets |
| 5 | 设置 Address Key | `TestHotUpdatePrefab` / `HotUpdateDll` |
| 6 | 配置 HybridCLR Settings 指向该 asmdef | `ProjectSettings/HybridCLRSettings.asset` |
| 7 | 配置 Addressables Profile 远程路径 | Remote.LoadPath = `http://xxx/[BuildTarget]` |
| 8 | 撰写 AOT 侧加载器 | `HotUpdateTestLoader.cs` |
| 9 | 场景中放置加载器 + 验证 UI | `HotUpdateTestUI` / `HotUpdateTestLoader` |

### 每次热更新

```
┌─ 改了代码？ ──────────────────────────┐
│ HybridCLR → Generate → All              │  → 重编 HotUpdate.dll
│ 覆盖 Assets/HotUpdate/HotUpdate.dll.bytes │  ← 同步到 Assets
└────────────────────────────────────────┘
                    ↓
┌─ 改了资源或 DLL？ ─────────────────────┐
│ Addressables Groups → Update Previous Build │  → 只打包变化的部分
│ 复制产物到 HTTP 服务器目录               │
└────────────────────────────────────────┘
                    ↓
              重启 Player → 自动拉取
```

### 复制产物到服务器

```powershell
Copy-Item Temp\com.unity.addressables\*.bundle ServerData\StandaloneWindows64\ -Force
Copy-Item Library\com.unity.addressables\aa\Windows\catalog.json ServerData\StandaloneWindows64\ -Force
Copy-Item Library\com.unity.addressables\aa\Windows\settings.json ServerData\StandaloneWindows64\ -Force
```

### 启动 HTTP 服务器

```powershell
python -m http.server 8000 -d ServerData
```

---

## 五、踩坑记录

### 5.1 IL2CPP 构建失败

| 问题 | 修复 |
|---|---|
| `Shader error: undeclared identifier LODFadeCrossFade` | `SimpleURPToonLitOutlineExample_Shared.hlsl` 添加 `#include LODCrossFade.hlsl` |
| `UnityLinker: Failed to resolve Mono.Posix` | `System.Windows.Forms.dll` / `Ookii.Dialogs.dll` 的 `.meta` 排除 Standalone 平台 |
| `CS0234: Forms does not exist` | `StandaloneFileBrowserWindows.cs` 包裹 `#if UNITY_EDITOR` |
| `CS0246: StandaloneFileBrowserWindows not found` | `StandaloneFileBrowser.cs` 静态构造器包裹 `#if UNITY_EDITOR` |
| `TJAIMaterial.cs: TextureImporter/AssetImporter` | 包裹 `#if UNITY_EDITOR` |
| `HotUpdate is duplicated` | `HybridCLRSettings` 中 `hotUpdateAssemblyDefinitions` 和 `hotUpdateAssemblies` 只保留一个 |

### 5.2 Addressables Key 不匹配

`Addressables.LoadAssetAsync("HotUpdateDll")` 查 catalog 找不到 key → `InvalidKeyException`。

**原因**: Group 里的 Address 和代码里的 key 不一致。  
**修复**: 确保 Address 列的值完全等于代码中的 key。

### 5.3 GUID 理解

Addressables / Unity 内部用 GUID 标识文件，不看路径。所以文件放哪个文件夹无所谓，关键是 Group 里登记了正确的 GUID。`.meta` 文件损坏会导致 GUID 丢失，需要 Reimport 重新生成。

### 5.4 代码默认值 vs 序列化值

```csharp
public string Message = "新值";  // 代码默认值
```

如果 prefab 已序列化了旧值，热更 DLL 后**不会**覆盖——Unity 优先用序列化值。纯代码热更应该改非序列化的逻辑（颜色、字号、追加文本等）。

### 5.5 Addressables 句柄泄漏

`Instantiate` 出来的对象和 `LoadAssetAsync` 的句柄必须在 `OnDestroy` 中释放：

```csharp
void OnDestroy() {
    if (_instantiated) Destroy(_instantiated);
    if (_prefabHandle.IsValid())  Addressables.Release(_prefabHandle);
    if (_dllHandle.IsValid())     Addressables.Release(_dllHandle);
}
```

---

## 六、关键文件清单

| 文件 | 作用 |
|---|---|
| `Assets/HotUpdate/HotUpdate.asmdef` | 热更程序集定义 |
| `Assets/HotUpdate/HotUpdateLabelChanger.cs` | 热更脚本 |
| `Assets/HotUpdate/HotUpdate.dll.bytes` | 编译后的 DLL(Addressable) |
| `Assets/HotUpdateAssets/TestHotUpdatePrefab.prefab` | Addressable 预制体 |
| `Assets/HotUpdateAssets/HotUpdateTestUI.uxml` | 验证 UI |
| `Assets/Scripts/HotUpdateTestLoader.cs` | AOT 加载器 |
| `Assets/link.xml` | IL2CPP Linker 配置 |
| `ProjectSettings/HybridCLRSettings.asset` | HybridCLR 配置 |
