# 手机中文字体显示 — 最终解决

> 2026-07-01 | 3 个文件改动

## 问题

移动端（Android）UI Toolkit 不显示中文，仅英文/数字/符号正常。所有面板（背包/任务/对话/MobileHUD）均受影响。

## 根因

UI Toolkit 在 Android 上不支持从 TTF 动态渲染 CJK 字符，也不支持 TMP SDF（`TMP_FontAsset`）。必须使用 UI Toolkit 原生 SDF 字体（`Window→Text→FontAssetCreator` 创建），通过 `PanelSettings→Text Settings→DefaultFontAsset` 配置。

## 踩坑记录

| 尝试 | 方案 | 结果 |
|---|---|---|
| 1 | USS `resource("xxx.ttf")` | ❌ 移动端不渲染 |
| 2 | `Resources.Load<Font>("xxx.ttf")` + `style.unityFont` | ❌ 移动端不渲染 |
| 3 | TMP Font Asset Creator SDF | ❌ UI Toolkit 不支持 TMP_FontAsset |
| 4 | `Resources.Load<FontAsset>` + `FontDefinition.FromSDFFont` | ❌ 运行时无法加载 |
| 5 | `Font.CreateDynamicFontFromOSFont("sans-serif")` | ❌ PC 报错/移动端不生效 |
| 6 | Custom Characters + PanelTextSettings | **✅ 成功** |

## 最终方案

1. **创建字体**：`Window → Text → Font Asset Creator`
   - Source Font: `NotoSansSC-Regular.ttf`
   - Character Set: `Custom Characters`（精确字集，约 600 字）
   - Atlas: `512×512`、Padding `5`
   - 保存到 `Assets/Resources/Fonts/NotoSansSC-Regular SDF.asset`

2. **配置 PanelSettings**：
   - 选中 `Assets/UI Toolkit/PanelSettings.asset` → Inspector → Text Settings → 拖入 `UITK Text Settings.asset`
   - `UITK Text Settings` → Default Font Asset → `NotoSansSC-Regular SDF`

## 关键经验

- **不要用 `Resources.Load` 加载 FontAsset**：运行时无法反序列化，PanelSettings 是唯一正确通道
- **不要用超大 Unicode Range**：`0020-9FFF` 会撑爆图集导致全部空白
- **Padding 不宜大于 5**：8 会裁碎字形
- **中文字集提取**：从 `.uxml` + `.yarn` 用 Python 脚本 `[regex: \u4e00-\u9fff]` 精确去重

## 涉及文件

| 文件 | 改动 |
|---|---|
| `Assets/Resources/Fonts/NotoSansSC-Regular SDF.asset` | 新建 SDF 字体 |
| `Assets/Resources/Fonts/UITK Text Settings.asset` | 新建文本设置 |
| `Assets/UI Toolkit/PanelSettings.asset` | Text Settings 字段绑定 |
