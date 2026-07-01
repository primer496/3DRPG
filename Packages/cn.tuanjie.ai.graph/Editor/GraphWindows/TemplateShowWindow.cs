using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using static GraphProcessor.ProviderGroup;
using UnityEditor.MemoryProfiler;

namespace UnityEditor.AIGraph
{ 
    internal class TemplateWindow : EditorWindow
    {
        [Flags]
        public enum Category
        {
            None = 0,
            All = 1,
            TwoD = 2,
            ThreeD = 4,
            Material = 8,
            Animation = 16,
        }
        private Category m_ActiveCategories = Category.All;
        public Category activeCategories
        {
            set
            {
                if (value != m_ActiveCategories)
                {
                    SetCategory(value);
                    Repaint();
                }
            }
        }


        private UnityEngine.Object[] m_Targets;
        public UnityEngine.Object[] targets
        {
            get => m_Targets ?? Array.Empty<Object>();
            set
            {
                if (m_Targets != value)
                    m_Targets = value;
            }
        }

        private List<TemplateItem> allItems;
        private ScrollView contentScrollView;
        private Button allButton, twoDButton, threeDButton, textureButton, characterButton;

        private float s_DefaultHeight = 150f;
        private float s_DefaultWidth = 200f;

        private void CreateGUI()
        {
            var root = rootVisualElement;

            var styleSheet = Resources.Load<StyleSheet>("uss/TemplateWindow");
            root.styleSheets.Add(styleSheet);

            root.AddToClassList("root");
            var a = Selection.gameObjects;
            CreateHeader(root);
            CreateCategoryBar(root);
            CreateContentArea(root);
            InitializeData();
            RefreshContent();
        }

        private void CreateHeader(VisualElement root)
        {
            var header = new VisualElement();
            header.AddToClassList("header");

            var title = new Label("Tuanjie AI Graph Template");
            title.AddToClassList("title");

            header.Add(title);
            root.Add(header);
        }

        private void CreateCategoryBar(VisualElement root)
        {
            var categoryBar = new VisualElement();
            categoryBar.AddToClassList("category-bar");

            allButton = new Button(() => SetCategory(Category.All)) { text = "All" };
            twoDButton = new Button(() => SetCategory(Category.TwoD)) { text = "2D" };
            threeDButton = new Button(() => SetCategory(Category.ThreeD)) { text = "3D" };
            textureButton = new Button(() => SetCategory(Category.Material)) { text = "Material" };
            characterButton = new Button(() => SetCategory(Category.Animation)) { text = "Animation" };

            foreach (var btn in new[] { allButton, twoDButton, threeDButton, textureButton, characterButton })
            {
                btn.AddToClassList("button");
            }

            allButton.AddToClassList("button-active");

            categoryBar.Add(allButton);
            categoryBar.Add(twoDButton);
            categoryBar.Add(threeDButton);
            categoryBar.Add(textureButton);
            categoryBar.Add(characterButton);

            root.Add(categoryBar);

            UpdateButtonStyles();
        }


        private void CreateContentArea(VisualElement root)
        {
            contentScrollView = new ScrollView();
            contentScrollView.AddToClassList("content-scroll-view");

            var gridContainer = new VisualElement();
            gridContainer.AddToClassList("grid-container");

            contentScrollView.Add(gridContainer);
            root.Add(contentScrollView);
        }

        private void InitializeData()
        {
            allItems = new List<TemplateItem>
            {
                new TemplateItem(
                    "2D文生图（腾讯混元）", 
                    "根据Prompt提示生成图片", 
                    Category.TwoD, 
                    LoadTexture("2DTextToImageTmp"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanTextToImage(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Image Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图生图（腾讯混元）", 
                    "根据参考图生成图片", 
                    Category.TwoD, 
                    LoadTexture("2DImageToImage"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanImageToImage(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Image Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D文生Sprite（腾讯混元）",
                    "根据Prompt提示生成Sprite，在文生图管线基础上增加了背景去除和降采样",
                    Category.TwoD,
                    LoadTexture("2DTextToSprite"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanTextToSprite(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Sprite Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图生Sprite（腾讯混元）",
                    "根据参考图生成Sprite，在图生图管线基础上增加了背景去除和降采样",
                    Category.TwoD,
                    LoadTexture("2DImageToSprite"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanImageToSprite(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(1600f, s_DefaultHeight - 50f), "2D Sprite Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图片上色（腾讯混元）",
                    "输入灰度图和prompt描述生成彩色图片",
                    Category.TwoD,
                    LoadTexture("2DImageColoring"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanImageColoring(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Image Coloring Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图片风格转换（腾讯混元）",
                    "根据输入图片生成特殊风格图片，风格包含旅行、像素、日漫、动漫和水彩风格",
                    Category.TwoD,
                    LoadTexture("2DStyleSwitching"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanImageStyleSwitch(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Image Style Switching Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D角色图生成角色三视图（腾讯混元）",
                    "输入角色图生成角色三视图",
                    Category.TwoD,
                    LoadTexture("2DImageThreeView"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanCharacter3View(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Three View Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D角色图编辑（腾讯混元）",
                    "根据prompt描述编辑角色图片",
                    Category.TwoD,
                    LoadTexture("2DCharacterEditing"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanCharacterEditing(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Character Editing Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图片背景去除（腾讯混元）",
                    "去除输入图片的背景",
                    Category.TwoD,
                    LoadTexture("2dremovebg"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuaRemoveBG(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Remove Background Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图片背景替换（腾讯混元）",
                    "根据输入图片和Mask替换背景",
                    Category.TwoD,
                    LoadTexture("2dreplacebg"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanReplaceBG(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Replace Background Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图片超分辨率（腾讯混元）",
                    "根据输入图片输出高分辨率图片",
                    Category.TwoD,
                    LoadTexture("2dsuperresolute"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanSuperResolution(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get2DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "2D Super Resolution Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "2D图生天空盒（Rodin3D）",
                    "根据输入图片生成天空盒材质",
                    Category.TwoD | Category.Material,
                    LoadTexture("2dskybox"),
                    targets =>
                    {
                        StickyNote globalNote = new StickyNote("Rodin天空盒生成", new Vector2(s_DefaultWidth + 350f, s_DefaultWidth - 100f), 200f, 100f);
                        globalNote.content = "输入prompt描述或图片生成天空盒材质，可切换输入方式";

                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
                        {
                            (typeof(StringNode), new Vector2(s_DefaultWidth, s_DefaultWidth), null),
                            (typeof(RodinGenerateSkyboxNode), new Vector2(s_DefaultWidth + 350f, s_DefaultWidth), null),
                        };
                        var edgeList = new List<((int, string, string), (int, string, string))>()
                        {
                            ((1, "prompt", null), (0, "output", null))
                        };

                        StickyNote nodeNote = null;
                        if (targets.Length > 0 && targets[0] is Skybox)
                        {
                            nodeList.Add((typeof(SkyboxComponentNode), new Vector2(s_DefaultWidth + 700f, s_DefaultWidth), null));
                            edgeList.Add(((2, "m_Material", null), (1, "m_Material", null)));
                            nodeNote = new StickyNote("节点说明", new Vector2(s_DefaultWidth + 700f, s_DefaultWidth - 100f), 200f, 100f);
                            nodeNote.content = "运行将替换对应Skybox组件上的material";
                        }
                        else if (targets.Length > 0 && targets[0] is RenderSettings)
                        {
                            nodeList.Add((typeof(LightSettingsNode), new Vector2(s_DefaultWidth + 700f, s_DefaultWidth), null));
                            edgeList.Add(((2, "m_Material", null), (1, "m_Material", null)));
                            nodeNote = new StickyNote("节点说明", new Vector2(s_DefaultWidth + 700f, s_DefaultWidth - 100f), 200f, 100f);
                            nodeNote.content = "运行将替换Window->Rendering->Lighting中的天空盒材质";
                        }

                        StickyNote note = new StickyNote("选择输入方式", new Vector2(200f, 30f), 250f, 100f);
                        note.content = "下拉条可切换输入方式，包括Text和Image两种，切换后先前历史信息会丢失请注意保存资产! 若删除初始节点可能导致Edge无法在切换后保持连接，需手动重连！";
                        var group = new ProviderGroup("Input", new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), new List<string>(){ "Text", "Image" },
                            new List<GroupNodeEdgeInfo>()
                            {
                                new GroupNodeEdgeInfo()
                                {
                                    nodes = new List<string>() { typeof(StringNode).AssemblyQualifiedName},
                                    edges = new List<EdgeInfo>(),
                                    connectPorts = new List<GroupConnectPort>() { new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 0, portField = "output", portIdentifier = null} },
                                },
                                new GroupNodeEdgeInfo()
                                {
                                    nodes = new List<string>() { typeof(TextureAssetNode).AssemblyQualifiedName },
                                    edges = new List<EdgeInfo>(),
                                    connectPorts = new List<GroupConnectPort>() { new GroupConnectPort() { connectPortInfoIndex = 1, nodeIndex = 0, portField = "m_OutputTexture", portIdentifier = null}},
                                }
                            },
                        100f);
                        group.size = new Vector2(400f, 300f);
                        var graph = CreateGraphTemplate(nodeList, edgeList);
                        graph.AddStickyNote(note);
                        graph.AddStickyNote(globalNote);
                        if (nodeNote != null)
                            graph.AddStickyNote(nodeNote);
                        group.addNodeBeforeInit(graph.nodes[0]);
                        group.setConnectPort(graph.nodes[1], "prompt", null, true);
                        group.setConnectPort(graph.nodes[1], "images", null, true);
                        graph.AddGroup(group);
                        graph.name = "2D Skybox Template";
                        TJAIGraphWindow.Open(graph);
                    }
                ),
                new TemplateItem(
                "3D文/图生模型（腾讯混元）",
                "根据输入prompt描述或图片生成模型",
                Category.ThreeD,
                    LoadTexture("3dhunyuantextimagetomodel"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanTextToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Text/Image to Model Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetHunyuanInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1, 2 },
                                    new List<(int, string, string, bool)>() { (3, "inputModelUrl", null, true), (5, "image", null, true) }));
                            });
                    }
                ),
                new TemplateItem(
                    "3D文/图生模型（Tripo）",
                    "根据输入prompt描述或图片生成模型",
                    Category.ThreeD,
                    LoadTexture("3dvasttextimagetomodel"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastTextToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Text/Image to Model Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetVastInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1 },
                                    new List<(int, string, string, bool)>() { (2, "m_Go", null, true) }));
                            });
                    }
                ),
                new TemplateItem(
                    "3D文/图生模型（Rodin3D）",
                    "根据输入prompt描述或图片生成模型",
                    Category.ThreeD,
                    LoadTexture("3drodintextimagetomodel"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.RodinTextToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 700f), "3D Text/Image to Model Template", ref nodeList, ref edgeList, stickyNote,                           
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetRodinInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1, 2, 3, 4 },
                                    new List<(int, string, string, bool)>() { (5, "m_Go", null, true) }));
                            });
                    }
                ),
                new TemplateItem(
                    "3D多视图生模型（腾讯混元）",
                    "根据输入多视图生成模型",
                    Category.ThreeD,
                    LoadTexture("3dhunyuanmulti"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanMultiviewToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Multi-view to Model Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D多视图生模型（Tripo）",
                    "根据输入多视图生成模型",
                    Category.ThreeD,
                    LoadTexture("3dvastmulti"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastMultiviewToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Multi-view to Model Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D多视图生模型（Rodin3D）",
                    "根据输入多视图生成模型",
                    Category.ThreeD,
                    LoadTexture("3drodinmulti"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.RodinViewsToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 700f), "3D Multi-view to Model Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetRodinModelSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 5 },
                                    new List<(int, string, string, bool)>() { (6, "m_Go", null, true), (0, "m_OutputTexture", null, false), (1, "m_OutputTexture", null, false),
                                    (2, "m_OutputTexture", null, false), (3, "m_OutputTexture", null, false), (4, "m_OutputTexture", null, false)}));
                            });
                    }
                ),
                new TemplateItem(
                    "3D线稿生模型（腾讯混元）",
                    "根据输入的线稿和图片描述生成模型",
                    Category.ThreeD,
                    LoadTexture("3dsketchtomodel"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanSketchToModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Sketch to Model Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D文/图生成低面数模型（Tripo）",
                    "根据输入prompt描述或图片生成模型并简化",
                    Category.ThreeD,
                    LoadTexture("3dvasttextimagelowpoly"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastTextToLowPolyModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Text/Image to Lowpoly Model Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetVastInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1 },
                                    new List<(int, string, string, bool)>() { (2, "inputModelID", null, true) }, true));
                            });
                    }
                ),
                new TemplateItem(
                    "3D多视图生成低面数模型（Tripo）",
                    "根据输入多视图生成模型并简化",
                    Category.ThreeD,
                    LoadTexture("3dvastviewslowpoly"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastMultiViewToLowPolyModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Multi-view to Lowpoly Model Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D文/图生成风格化模型（Tripo）",
                    "根据输入prompt描述或图片生成模型并风格化",
                    Category.ThreeD,
                    LoadTexture("3Dtextimagestylize"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastTextToStyleModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Text/Image to Stylized Model Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetVastInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1 },
                                    new List<(int, string, string, bool)>() { (2, "inputModelID", null, true) }, true));
                            });
                    }
                ),
                new TemplateItem(
                    "3D多视图生成风格化模型（Tripo）",
                    "根据输入多视图生成模型并风格化",
                    Category.ThreeD,
                    LoadTexture("3Dvaststylizemulti"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastMultiViewToStyleModel(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight - 50f), "3D Multi-view to Stylized Model Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D文/图生动画角色（腾讯混元）",
                    "根据prompt描述或图片生成带动画的角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3dhunyuantextimagetocharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanTextToCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Text to Character Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetHunyuanInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1, 2 },
                                    new List<(int, string, string, bool)>() { (3, "inputModelUrl", null, true), (5, "image", null, true) }));
                            });
                    }
                ),
                new TemplateItem(
                    "3D文/图生动画角色（Tripo）",
                    "根据prompt描述或图片生成带动画的角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3dvasttextimagetocharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastTextToCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Text/Image to Character Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetVastInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1 },
                                    new List<(int, string, string, bool)>() { (2, "inputModelID", null, true) }, true));
                            });
                    }
                ),
                new TemplateItem(
                    "3D多视图生动画角色（腾讯混元）",
                    "根据输入多视图生成带动画的角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3DMultiViewToCharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanViewsToCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Multi-view to Character Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D多视图生动画角色（Tripo）",
                    "根据输入多视图生成带动画的角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3DMultiViewToCharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastMultiviewToCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Multi-view to Character Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D线稿生动画角色（腾讯混元）",
                    "根据输入线稿生成带动画的角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3dsketch"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.HunyuanSketchToCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Sketch to Character Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D文/图生带动画的低面数角色模型（Tripo）",
                    "根据prompt描述或图片生成带动画的低面数角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3DImgToLowPoly"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastTextToLowPolyCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Text/Image to Lowpoly Character Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetVastInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1 },
                                    new List<(int, string, string, bool)>() { (2, "inputModelID", null, true) }, true));
                            });
                    }
                ),
                new TemplateItem(
                    "3D多视图生带动画的低面数角色模型（Tripo）",
                    "根据输入多视图生成带动画的低面数角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3Dvastlowpolymulticharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastMultiviewToLowPolyCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Multi-view to Lowpoly Character Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),
                new TemplateItem(
                    "3D文/图生带动画的风格化角色模型（Tripo）",
                    "根据prompt描述或图片生成带动画的风格化角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3dvaststylizecharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastTextToStylizeCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Text/Image to Stylized Character Template", ref nodeList, ref edgeList, stickyNote,
                            graph =>
                            {
                                graph.AddGroup(TemplateGraph.GetVastInputSession(new Vector2(s_DefaultWidth - 50f, s_DefaultHeight - 50f), graph, new List<int>() { 0, 1 },
                                    new List<(int, string, string, bool)>() { (2, "inputModelID", null, true) }, true));
                            });
                    }
                ),
                new TemplateItem(
                    "3D多视图生带动画的风格化角色模型（Tripo）",
                    "根据输入多视图生成带动画的风格化角色模型",
                    Category.ThreeD | Category.Animation,
                    LoadTexture("3Dvaststylizemulticharacter"),
                    target =>
                    {
                        var nextWidth = TemplateGraph.VastMultiviewToStylizeCharacter(s_DefaultWidth, s_DefaultHeight, out var nodeList, out var edgeList, out var stickyNote);
                        Get3DAnimTemplateWithTarget(new Vector2(nextWidth, s_DefaultHeight + 400f), "3D Multi-view to Stylized Character Template", ref nodeList, ref edgeList, stickyNote);
                    }
                ),

                new TemplateItem(
                    "3D模型简化（腾讯混元）",
                    "上传3D模型生成低面数模型",
                    Category.ThreeD,
                    LoadTexture("3dmeshsimp"),
                    targets =>
                    {
                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>();
                        var edgeList = new List<((int, string, string), (int, string, string))>();
                        List<StickyNote> notes = new List<StickyNote>();
                        List<(int, int)> pairNode = new List<(int, int)>();
                        float shift = 0f;
                        bool atLeastOneTmpExist = false;

                        var globalnote = new StickyNote("腾讯混元模型简化", new Vector2(550f, s_DefaultHeight + shift), 200f, 100f);
                        globalnote.content = "上传模型生成重新布线低模";
                        notes.Add(globalnote);
                        bool needUploadStickyNote = true;

                        bool needMRNote = true;
                        bool needSkinnedMRNote = true;
                        bool needPSRNote = true;

                        foreach (var target in targets)
                        {
                            if (GetUploadSession(new Vector2(200f, s_DefaultHeight + shift),
                                typeof(HyUploadModelByGONode), target, ref nodeList,
                                ref notes, new List<Type>(){ typeof(ParticleSystemRenderer) }, needUploadStickyNote))
                                needUploadStickyNote = false;
                            else
                                continue;

                            atLeastOneTmpExist = true;
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyLowpolyNode), new Vector2(550f, s_DefaultHeight + shift + 100f), null),
                                (typeof(HySemanticUVNode), new Vector2(900f, s_DefaultHeight + shift), null),
                                (typeof(MeshAndMaterialNode),  new Vector2(1250f, s_DefaultHeight + shift), null)
                            });
                            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
                            {
                                ((nodeList.Count - 3, "inputModelUrl", null), (nodeList.Count - 4, "outputModelUrl", null)),
                                ((nodeList.Count - 2, "inputModelUrl", null), (nodeList.Count - 3, "outputModelUrl", null)),
                                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),
                            });

                            var lastNodeIndex = nodeList.Count - 1;
                            if (target is GameObject)
                            {
                                var go = target as GameObject;

                                if (go.GetComponent<MeshFilter>() != null)
                                {
                                    Action<BaseNode> mfAction = node => ((MeshFilterNode)node).owner = (target as GameObject);
                                    nodeList.Add((typeof(MeshFilterNode), new Vector2(1500f, s_DefaultHeight + shift), mfAction));
                                    edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (lastNodeIndex, "m_Mesh", null)));
                                }
                                if (go.GetComponent<MeshRenderer>() != null)
                                {
                                    Action<BaseNode> mrAction = node => ((MeshRendererNode)node).renderer = (target as GameObject).GetComponent<MeshRenderer>();
                                    nodeList.Add((typeof(MeshRendererNode),  new Vector2(1850f, s_DefaultHeight + shift), mrAction));
                                    edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                                }

                                if ((go.GetComponent<MeshFilter>() != null || go.GetComponent<MeshRenderer>() != null) && needMRNote)
                                {
                                    var note = new StickyNote("节点说明", new Vector2(1500f, s_DefaultHeight + shift - 100f), 200f, 100f);
                                    note.content = "运行MeshRendererNode和MeshFilterNode，对应的material和Mesh将被替换。";
                                    notes.Add(note);
                                    needMRNote = false;
                                }

                                if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshRenderer>() != null)
                                    pairNode.Add((nodeList.Count - 2, nodeList.Count - 1));
                            }
                            else if (target is SkinnedMeshRenderer)
                            {
                                Action<BaseNode> mrAction = node => ((SkinnedMeshRendererNode)node).renderer = (target as SkinnedMeshRenderer);
                                nodeList.Add((typeof(SkinnedMeshRendererNode), new Vector2(1600f, s_DefaultHeight + shift), mrAction));
                                edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (lastNodeIndex, "m_Mesh", null)));
                                edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                                if (needSkinnedMRNote)
                                {
                                    var note = new StickyNote("节点说明", new Vector2(1600f, s_DefaultHeight + shift - 100f), 200f, 100f);
                                    note.content = "运行SkinnedMeshRendererNode，对应的material和mesh将被替换。";
                                    notes.Add(note);
                                    needSkinnedMRNote = false;
                                }
                            }
                            else if (target is ParticleSystemRenderer)
                            {
                                Action<BaseNode> mrAction = node => ((ParticleSystemRendererNode)node).renderer = (target as ParticleSystemRenderer);
                                nodeList.Add((typeof(ParticleSystemRendererNode), new Vector2(1600f, s_DefaultHeight + shift), mrAction));
                                edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (lastNodeIndex, "m_Mesh", null)));
                                edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                                if (needPSRNote)
                                {
                                    var note = new StickyNote("节点说明", new Vector2(1600f, s_DefaultHeight + shift - 100f), 200f, 100f);
                                    note.content = "运行ParticleSystemRendererNode，对应的material和mesh将被替换。";
                                    notes.Add(note);
                                    needPSRNote = false;
                                }
                            }
                            shift += 400f;
                        }

                        if (!atLeastOneTmpExist)
                        {
                            var note = new StickyNote("上传模型", new Vector2(0f, s_DefaultHeight + shift), 200f, 100f);
                            note.content = "选择模型对应的GameObject，上传至服务器";
                            notes.Add(note);
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyUploadModelByGONode), new Vector2(200f, s_DefaultHeight), null),
                                (typeof(HyLowpolyNode), new Vector2(550f, s_DefaultHeight + shift + 100f), null),
                                (typeof(HySemanticUVNode), new Vector2(900f, s_DefaultHeight + shift), null),
                                (typeof(MeshAndMaterialNode),  new Vector2(1250f, s_DefaultHeight + shift), null)
                            });
                            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
                            {
                                ((nodeList.Count - 3, "inputModelUrl", null), (nodeList.Count - 4, "outputModelUrl", null)),
                                ((nodeList.Count - 2, "inputModelUrl", null), (nodeList.Count - 3, "outputModelUrl", null)),
                                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),
                            });
                        }

                        var graph = CreateGraphTemplate(nodeList, edgeList);

                        foreach(var pair in pairNode)
                        {
                            graph.nodes[pair.Item1].onProcessed += () => (graph.nodes[pair.Item2] as MeshRendererNode).NotifyFieldChanged("m_GO");
                        }

                        graph.name = "3D Lowpoly Template";

                        foreach (var note in notes)
                        {
                            graph.AddStickyNote(note);
                        }
                        TJAIGraphWindow.Open(graph);
                    }
                ),
                new TemplateItem(
                    "3D自动绑骨（腾讯混元）",
                    "上传3D模型生成骨骼和权重",
                    Category.ThreeD |Category.Animation,
                    LoadTexture("3DAutoRigging"),
                    targets =>
                    {
                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>();
                        var edgeList = new List<((int, string, string), (int, string, string))>();
                        List<StickyNote> notes = new List<StickyNote>();
                        float shift = 0f;
                        bool atLeastOneTmpExist = false;

                        bool needUploadStickyNote = true;

                        var globalnote = new StickyNote("腾讯混元自动绑骨", new Vector2(550f, s_DefaultHeight - 100f), 200f, 100f);
                        globalnote.content = "上传模型生成骨骼和蒙皮权重";
                        notes.Add(globalnote);

                        foreach (var target in targets)
                        {
                            if (GetUploadSession(new Vector2(200f, s_DefaultHeight + shift),
                                typeof(HyUploadModelByGONode), target, ref nodeList,
                                ref notes, new List<Type>(), needUploadStickyNote))
                                needUploadStickyNote = false;
                            else
                                continue;
                            atLeastOneTmpExist = true;

                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyAutoRiggingNode), new Vector2(550f, s_DefaultHeight + shift), null),
                                (typeof(MeshAndMaterialNode),  new Vector2(900f, s_DefaultHeight + shift), null),
                            });
                            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
                            {
                                ((nodeList.Count - 2, "inputModelUrl", null), (nodeList.Count - 3, "outputModelUrl", null)),
                                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),
                            });

                            if (target is SkinnedMeshRenderer)
                            {
                                var note = new StickyNote("节点说明", new Vector2(1250f, s_DefaultHeight - 100f), 200f, 100f);
                                note.content = "运行SkinnedMeshRendererNode，对应的material和mesh将被替换。";
                                notes.Add(note);
                                Action<BaseNode> mrAction = node => ((SkinnedMeshRendererNode)node).renderer = (target is GameObject) ?
                                (target as GameObject).GetComponent<SkinnedMeshRenderer>() : (target is SkinnedMeshRenderer ? (target as SkinnedMeshRenderer) : null);
                                nodeList.Add((typeof(SkinnedMeshRendererNode), new Vector2(1250f, s_DefaultHeight + shift), mrAction));
                                edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (nodeList.Count - 2, "m_Mesh", null)));
                                edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (nodeList.Count - 2, "m_Materials", null)));
                            }

                            shift += 400f;
                        }


                        if (!atLeastOneTmpExist)
                        {
                            var note = new StickyNote("上传模型", new Vector2(0f, s_DefaultHeight + shift), 200f, 100f);
                            note.content = "选择模型对应的GameObject，上传至服务器";
                            notes.Add(note);
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyUploadModelByGONode), new Vector2(200f, s_DefaultHeight), null),
                                (typeof(HyAutoRiggingNode), new Vector2(550f, s_DefaultHeight), null),
                                (typeof(MeshAndMaterialNode),  new Vector2(900f, s_DefaultHeight), null),
                            });
                            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
                            {
                                ((nodeList.Count - 2, "inputModelUrl", null), (nodeList.Count - 3, "outputModelUrl", null)),
                                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),
                            });
                        }

                        var graph = CreateGraphTemplate(nodeList, edgeList);
                        graph.name = "Auto Rigging Template";

                        foreach (var note in notes)
                        {
                            graph.AddStickyNote(note);
                        }
                        TJAIGraphWindow.Open(graph);
                    }
                ),
                new TemplateItem(
                    "3D动画重定向（腾讯混元）",
                    "上传3D模型生成腾讯混元预设动画",
                    Category.ThreeD |Category.Animation,
                    LoadTexture("3dretarget"),
                    targets =>
                    {
                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>();
                        var edgeList = new List<((int, string, string), (int, string, string))>();
                        List<StickyNote> notes = new List<StickyNote>();
                        float shift = 0f;
                        bool atLeastOneTmpExist = false;

                        bool needUploadStickyNote = true;

                        var globalnote = new StickyNote("腾讯混元动画重定向", new Vector2(550f, s_DefaultHeight - 100f), 200f, 100f);
                        globalnote.content = "上传带有蒙皮权重的模型，获得预设动画";
                        notes.Add(globalnote);
                        foreach (var target in targets)
                        {
                            if (GetUploadSession(new Vector2(200f, s_DefaultHeight + shift),
                                typeof(HyUploadModelByGONode), target, ref nodeList,
                                ref notes, new List<Type>(), needUploadStickyNote))
                                needUploadStickyNote = false;
                            else
                                continue;
                            atLeastOneTmpExist = true;

                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyMotionRetargetNode), new Vector2(550f, s_DefaultHeight + shift), null),
                            });
                            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
                            {
                                ((nodeList.Count - 1, "inputModelUrl", null), (nodeList.Count - 2, "outputModelUrl", null)),
                            });

                            shift += 400f;
                        }


                        if (!atLeastOneTmpExist)
                        {
                            var note = new StickyNote("上传模型", new Vector2(0f, s_DefaultHeight + shift), 200f, 100f);
                            note.content = "选择模型对应的GameObject，上传至服务器";
                            notes.Add(note);
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyUploadModelByGONode), new Vector2(200f, s_DefaultHeight), null),
                                (typeof(HyMotionRetargetNode), new Vector2(550f, s_DefaultHeight), null),
                            });
                            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
                            {
                                ((nodeList.Count - 1, "inputModelUrl", null), (nodeList.Count - 2, "outputModelUrl", null)),
                            });
                        }

                        var graph = CreateGraphTemplate(nodeList, edgeList);
                        graph.name = "Retarget Template";
                        foreach (var note in notes)
                        {
                            graph.AddStickyNote(note);
                        }

                        TJAIGraphWindow.Open(graph);
                    }
                ),
                new TemplateItem(
                    "3D图生材质（腾讯混元）",
                    "输入图片并上传模型，生成模型材质",
                    Category.ThreeD |Category.Material,
                    LoadTexture("3DImageToMTL"),
                    targets =>
                    {
                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>();
                        var edgeList = new List<((int, string, string), (int, string, string))>();
                        List<(int, int)> pairNode = new List<(int, int)>();
                        List<StickyNote> notes = new List<StickyNote>();
                        float shift = 0f;
                        bool atLeastOneTmpExist = false;

                        bool needUploadStickyNote = true;
                        bool needMRNote = true;
                        bool needSkinnedMRNote = true;
                        bool needPSRNote = true;
                        bool needMtlNote = true;
                        bool needStickyNotes = true;

                        foreach (var target in targets)
                        {
                           if (GetUploadSession(new Vector2(200f, s_DefaultHeight + shift),
                                typeof(HyUploadModelByGONode), target, ref nodeList,
                                ref notes, new List<Type>() { typeof(Material), typeof(ParticleSystemRenderer) }, needUploadStickyNote))
                                needUploadStickyNote = false;
                            else
                                continue;

                            int uploadIndex = nodeList.Count - 1;
                            atLeastOneTmpExist = true;
                            TemplateGraph.HunyuanImgToMtl(550f, s_DefaultHeight + shift, uploadIndex,  ref nodeList, ref edgeList, ref notes, ref needStickyNotes);

                            AddRenderSupport(nodeList.Count - 1, new Vector2(1600f, s_DefaultHeight + shift), target, ref  nodeList, ref edgeList, ref notes, ref needMRNote, ref needSkinnedMRNote,
                                ref needPSRNote, ref needMtlNote);
                            shift += 400f;
                        }

                        if (!atLeastOneTmpExist)
                        {
                            var note = new StickyNote("上传模型", new Vector2(0f, s_DefaultHeight + shift), 200f, 100f);
                            note.content = "选择模型对应的GameObject，上传至服务器";
                            notes.Add(note);
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyUploadModelByGONode),  new Vector2(200f, s_DefaultHeight + shift), null),
                            });
                            TemplateGraph.HunyuanImgToMtl(550f, s_DefaultHeight + shift, 0,  ref nodeList, ref edgeList, ref notes, ref needStickyNotes);
                        }
                        var graph = CreateGraphTemplate(nodeList, edgeList);
                        graph.name = "3D Image to Material Template";

                        foreach (var note in notes)
                        {
                            graph.AddStickyNote(note);
                        }
                        TJAIGraphWindow.Open(graph);
                    }
                ),
                new TemplateItem(
                    "3D图生材质（Rodin3D）",
                    "输入图片并上传模型，生成模型材质",
                    Category.ThreeD |Category.Material,
                    LoadTexture("3DImageToMTL"),
                    targets =>
                    {
                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>();
                        var edgeList = new List<((int, string, string), (int, string, string))>();
                        List<(int, int)> pairNode = new List<(int, int)>();
                        List<StickyNote> notes = new List<StickyNote>();
                        float shift = 0f;
                        bool atLeastOneTmpExist = false;

                        bool needUploadStickyNote = true;
                        bool needMRNote = true;
                        bool needSkinnedMRNote = true;
                        bool needPSRNote = true;
                        bool needMtlNote = true;
                        bool needStickyNotes = true;

                        foreach (var target in targets)
                        {
                            if (GetUploadSession(new Vector2(200f, s_DefaultHeight + shift),
                                typeof(UploadModelByGONode), target, ref nodeList,
                                ref notes, new List<Type>() { typeof(Material), typeof(ParticleSystemRenderer) }, needUploadStickyNote))
                                needUploadStickyNote = false;
                            else
                                continue;

                            int uploadIndex = nodeList.Count - 1;
                            atLeastOneTmpExist = true;

                            TemplateGraph.RodinImgToMtl(550f, s_DefaultHeight + shift, uploadIndex,  ref nodeList, ref edgeList, ref notes, ref needStickyNotes);
                            AddRenderSupport(nodeList.Count - 1, new Vector2(1600f, s_DefaultHeight + shift), target, ref  nodeList, ref edgeList, ref notes, ref needMRNote, ref needSkinnedMRNote,
                                ref needPSRNote, ref needMtlNote);
                            shift += 550f;
                        }

                        if (!atLeastOneTmpExist)
                        {
                            var note = new StickyNote("上传模型", new Vector2(0f, s_DefaultHeight + shift), 200f, 100f);
                            note.content = "选择模型对应的GameObject，上传至服务器";
                            notes.Add(note);
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(UploadModelByGONode),  new Vector2(200f, s_DefaultHeight + shift), null),
                            });
                            TemplateGraph.RodinImgToMtl(550f, s_DefaultHeight + shift, 0,  ref nodeList, ref edgeList, ref notes, ref needStickyNotes);
                        }
                        var graph = CreateGraphTemplate(nodeList, edgeList);
                        graph.name = "3D Image to Material Template";

                        foreach (var note in notes)
                        {
                            graph.AddStickyNote(note);
                        }
                        TJAIGraphWindow.Open(graph);
                    }
                ),
                new TemplateItem(
                    "3D多视图生材质（腾讯混元）",
                    "输入多视图并上传模型，生成模型材质",
                    Category.ThreeD |Category.Material,
                    LoadTexture("3dmultimtl"),
                    targets =>
                    {
                        var nodeList = new List<(Type, Vector2, Action<BaseNode>)>();
                        var edgeList = new List<((int, string, string), (int, string, string))>();
                        List<(int, int)> pairNode = new List<(int, int)>();
                        List<StickyNote> notes = new List<StickyNote>();
                        float shift = 0f;
                        bool atLeastOneTmpExist = false;

                        bool needUploadStickyNote = true;
                        bool needMRNote = true;
                        bool needSkinnedMRNote = true;
                        bool needPSRNote = true;
                        bool needMtlNote = true;
                        bool needStickyNotes = true;

                        foreach (var target in targets)
                        {
                            if (GetUploadSession(new Vector2(200f, s_DefaultHeight + shift),
                                typeof(HyUploadModelByGONode), target, ref nodeList,
                                ref  notes, new List<Type>() { typeof(Material), typeof(ParticleSystemRenderer) }, needUploadStickyNote))
                                needUploadStickyNote = false;
                            else
                                continue;

                            int uploadIndex = nodeList.Count - 1;
                            atLeastOneTmpExist = true;

                            TemplateGraph.HunyuanViewsToMtl(550f, s_DefaultHeight + shift, uploadIndex,  ref nodeList, ref edgeList, ref notes, ref needStickyNotes);
                            AddRenderSupport(nodeList.Count - 1, new Vector2(1600f, s_DefaultHeight + shift), target, ref  nodeList, ref edgeList, ref notes, ref needMRNote, ref needSkinnedMRNote,
                                ref needPSRNote, ref needMtlNote);
                            shift += 1100f;
                        }

                        if (!atLeastOneTmpExist)
                        {
                            var note = new StickyNote("上传模型", new Vector2(0f, s_DefaultHeight + shift), 200f, 100f);
                            note.content = "选择模型对应的GameObject，上传至服务器";
                            notes.Add(note);
                            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
                            {
                                (typeof(HyUploadModelByGONode),  new Vector2(200f, s_DefaultHeight + shift), null),
                            });
                            TemplateGraph.HunyuanViewsToMtl(550f, s_DefaultHeight + shift, 0,  ref nodeList, ref edgeList, ref notes, ref needStickyNotes);
                        }
                        var graph = CreateGraphTemplate(nodeList, edgeList);
                        graph.name = "3D Views to Material Template";

                        foreach (var note in notes)
                        {
                            graph.AddStickyNote(note);
                        }
                        TJAIGraphWindow.Open(graph);
                    }
                ),
            };
        }

        private bool GetUploadSession(Vector2 pos, Type uploadNodeType, UnityEngine.Object target, ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
            ref List<StickyNote> notes, List<Type> extraAllowedType, bool needStickyNote)
        {
            bool addUploadNode = false;
            Action<BaseNode> UploadNodeAction = null;
            if (target is GameObject)
            {
                UploadNodeAction = node => ((UploadModelByGONode)node).obj = target as GameObject;
                addUploadNode = true;
            }
            else if (target is SkinnedMeshRenderer)
            {
                UploadNodeAction = node => ((UploadModelByGONode)node).obj = (target as SkinnedMeshRenderer).gameObject;
                addUploadNode = true;
            }

            if (!addUploadNode)
            {
                foreach (var allowedType in extraAllowedType)
                {
                    if (target.GetType() == allowedType)
                    {
                        addUploadNode = true;
                        break;
                    }
                }
            }
            
            if (addUploadNode)
            {
                nodeList.Add((uploadNodeType, pos, UploadNodeAction));
            }
            if (needStickyNote)
            {
                var note = new StickyNote("上传模型", new Vector2(pos.x - 200f, pos.y), 200f, 100f);
                note.content = "选择模型对应的GameObject，上传至服务器";
                notes.Add(note);
            }

            return addUploadNode;
        }

        private void AddRenderSupport(int lastNodeIndex, Vector2 pos, UnityEngine.Object target, ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
            ref List<((int, string, string), (int, string, string))> edgeList, ref List<StickyNote> notes, ref bool needMRNote, ref bool needSkinnedMRNote,
            ref bool needPSRNote, ref bool needMtlNote)
        {
            if ((target is GameObject && (target as GameObject).GetComponent<MeshRenderer>() != null)
                                || target is MeshRenderer)
            {
                Action<BaseNode> mrAction = null;
                if (target is GameObject)
                    mrAction = node => ((MeshRendererNode)node).renderer = (target as GameObject).GetComponent<MeshRenderer>();
                else
                    mrAction = node => ((MeshRendererNode)node).renderer = target as MeshRenderer;
                nodeList.Add((typeof(MeshRendererNode), pos, mrAction));
                edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                if (needMRNote)
                {
                    var note = new StickyNote("节点说明", new Vector2(pos.x, pos.y - 100f), 200f, 100f);
                    note.content = "运行MeshRendererNode，对应的material将被替换。";
                    notes.Add(note);
                }
                needMRNote = false;
            }
            else if (target is SkinnedMeshRenderer)
            {
                Action<BaseNode> mrAction = node => ((SkinnedMeshRendererNode)node).renderer = target as SkinnedMeshRenderer;
                nodeList.Add((typeof(SkinnedMeshRendererNode), pos, mrAction));
                edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                if (needSkinnedMRNote)
                {
                    var note = new StickyNote("节点说明", new Vector2(pos.x, pos.y - 100f), 200f, 100f);
                    note.content = "运行SkinnedMeshRendererNode，对应的material和mesh将被替换。";
                    notes.Add(note);
                }
                needSkinnedMRNote = false;
            }
            else if (target is ParticleSystemRenderer)
            {
                Action<BaseNode> mrAction = node => ((ParticleSystemRendererNode)node).renderer = target as ParticleSystemRenderer;
                nodeList.Add((typeof(ParticleSystemRendererNode), pos, mrAction));
                edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                if (needPSRNote)
                {
                    var note = new StickyNote("节点说明", new Vector2(pos.x, pos.y - 100f), 200f, 100f);
                    note.content = "运行ParticleSystemRendererNode，对应的material和mesh将被替换。";
                    notes.Add(note);
                }
                needPSRNote = false;
            }
            else if (target is Material)
            {
                Action<BaseNode> mtlAction = node => (node as MaterialReplaceNode).m_Target = target as Material;
                nodeList.Add((typeof(MaterialReplaceNode), pos, mtlAction));
                edgeList.Add(((nodeList.Count - 1, "m_Sources", null), (lastNodeIndex, "m_Materials", null)));
                if (needMtlNote)
                {
                    var note = new StickyNote("节点说明", new Vector2(pos.x, pos.y - 100f), 200f, 100f);
                    note.content = "运行MaterialReplaceNode，对应的material将被替换，原有的Asset中Material将被删除。";
                    notes.Add(note);
                }
                needMtlNote = false;
            }
        }

        private void Get2DTemplateWithTarget(Vector2 pos, string graphName, ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
            ref List<((int, string, string), (int, string, string))> edgeList, StickyNote stickyNote)
        {
            var lastNodeIndex = nodeList.Count - 1;
            bool hasSpriteRenderer = false;
            bool hasTextureImporter = false;
            float orgPosY = pos.y;
            foreach (var target in targets)
            {
                if (target is SpriteRenderer)
                {
                    Action<BaseNode> spriteRendererAction = node => ((SpriteRendererNode)node).renderer = (SpriteRenderer)target;
                    nodeList.Add((typeof(SpriteRendererNode), pos, spriteRendererAction));
                    edgeList.Add(((nodeList.Count - 1, "m_Sprite", null), (lastNodeIndex, "m_OutputTexture", null)));
                    hasSpriteRenderer = true;
                }
                else if (target is TextureImporter)
                {
                    Action<BaseNode> textureImporterAction = node => ((TextureImporterNode)node).importer = (TextureImporter)target;
                    nodeList.Add((typeof(TextureImporterNode), pos, textureImporterAction));
                    edgeList.Add(((nodeList.Count - 1, "m_Texture", null), (lastNodeIndex, "m_OutputTexture", null)));
                    hasTextureImporter = true;
                }

                pos.y += 400f;
            }


            var graph = CreateGraphTemplate(nodeList, edgeList);
            graph.name = graphName;
            graph.AddStickyNote(stickyNote);
            if (hasSpriteRenderer || hasTextureImporter)
            {
                StickyNote extraNote = new StickyNote("节点信息", new Vector2(pos.x, orgPosY - 100f), 300f, 100f);
                extraNote.content = "";
                if (hasSpriteRenderer)
                    extraNote.content += "运行SpriteRendererNode，对应的Sprite将被替换。";
                if (hasTextureImporter)
                    extraNote.content += "运行TextureImporterNode，对应在AssetPath的图片将在确认后被替换。";
                graph.AddStickyNote(extraNote);
            }

            TJAIGraphWindow.Open(graph);
        }

        private void Get3DAnimTemplateWithTarget(Vector2 pos, string graphName, ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
        ref List<((int, string, string), (int, string, string))> edgeList, StickyNote stickyNote, Action<TJAIGraph> onGraphCreated = null)
        {
            var skinNodeIndex = nodeList.Count - 2;
            var retargetNodeIndex = nodeList.Count - 1;
            string errorMessage = "";
            bool hasSkinnedMR = false;
            bool hasAnimation= false;
            bool hasAnimatorState = false;
            bool hasAnimationClip = false;
            bool hasGOorSkinedMR = false;
            float orgPosY = pos.y;
            float skinnedPos = pos.x;
            foreach (var target in targets)
            {
                if (target is GameObject || target is Animation || target is SkinnedMeshRenderer)
                {
                    if (target is GameObject && (target as GameObject).GetComponent<SkinnedMeshRenderer>() == null)
                    {
                        errorMessage += $"Warning: No SkinnedMeshRenderer on {target.name} as target of skeleton animation generation!\n";
                        continue;
                    }

                    Action<BaseNode> mrAction = node => ((SkinnedMeshRendererNode)node).renderer = (target is GameObject) ?
                       (target as GameObject).GetComponent<SkinnedMeshRenderer>() : (target is SkinnedMeshRenderer ? (target as SkinnedMeshRenderer) : null);
                    nodeList.Add((typeof(SkinnedMeshRendererNode), new Vector2(pos.x + 200f, pos.y), mrAction));
                    edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (skinNodeIndex, "m_Mesh", null)));
                    edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (skinNodeIndex, "m_Materials", null)));

                    hasSkinnedMR = true;

                    if (target is GameObject || target is SkinnedMeshRenderer)
                    {
                        hasGOorSkinedMR = true;
                        nodeList.Add((typeof(AnimatorCreatorNode), new Vector2(pos.x + 650f, pos.y), null));
                        edgeList.Add(((nodeList.Count - 1, "m_GO", null), (nodeList.Count - 2, "m_GO", null)));
                        edgeList.Add(((nodeList.Count - 1, "m_Clip", null), (retargetNodeIndex, "m_Clip", null)));
                    }
                    else if (target is Animation)
                    {
                        hasAnimation = true;
                        Action<BaseNode> AnimationAction = node => ((AnimationComponentNode)node).animation = (target as Animation);
                        nodeList.Add((typeof(AnimationComponentNode), new Vector2(pos.x + 650f, pos.y), AnimationAction));
                        edgeList.Add(((nodeList.Count - 1, "m_Clip", null), (retargetNodeIndex, "m_Clip", null)));
                    }

                }
                else if (target is AnimatorState || target is AnimationClip)
                {
                    nodeList.Add((typeof(SkinnedMeshRendererNode), new Vector2(pos.x + 200f, pos.y), null));
                    edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (skinNodeIndex, "m_Mesh", null)));
                    edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (skinNodeIndex, "m_Materials", null)));

                    if (target is AnimatorState)
                    {
                        hasAnimatorState = true;
                        Action<BaseNode> AnimationAction = node => ((AnimatorStateNode)node).animation = (target as AnimatorState);
                        nodeList.Add((typeof(AnimatorStateNode), new Vector2(pos.x + 650f, pos.y), AnimationAction));
                        edgeList.Add(((nodeList.Count - 1, "m_Clip", null), (retargetNodeIndex, "m_Clip", null)));

                    }
                    else if (target is AnimationClip)
                    {
                        hasAnimationClip = true;
                        Action<BaseNode> AnimationAction = node => ((AnimationClipReplaceNode)node).m_Target = (target as AnimationClip);
                        nodeList.Add((typeof(AnimationClipReplaceNode), new Vector2(pos.x + 650f, pos.y), AnimationAction));
                        edgeList.Add(((nodeList.Count - 1, "m_Source", null), (retargetNodeIndex, "m_Clip", null)));
                    }
                }

                pos.y += 400f;
            }
            var graph = CreateGraphTemplate(nodeList, edgeList);

            graph.name = graphName;
            graph.AddStickyNote(stickyNote);

            if (hasSkinnedMR)
            {
                StickyNote extraNote = new StickyNote("节点信息", new Vector2(pos.x + 200f, orgPosY - 100f), 200f, 100f);
                extraNote.content = "运行SkinnedMeshRendererNode，对应的material和mesh将被替换。";
                graph.AddStickyNote(extraNote);
            }
            if (hasAnimation)
            {
                StickyNote extraNote1 = new StickyNote("节点信息", new Vector2(pos.x + 650f, orgPosY - 100f), 300f, 100f);
                extraNote1.content = "运行AnimationComponentNode，对应Animation组件上的默认AnimationClip将被替换，新的AnimationClip也将加入ClipList。";
                graph.AddStickyNote(extraNote1);
            }
            if (hasAnimatorState)
            {
                StickyNote extraNote1 = new StickyNote("节点信息", new Vector2(pos.x + 650f, orgPosY - 100f), 300f, 100f);
                extraNote1.content = "运行AnimatorStateNode，对应Controller中state上的AnimationClip将被替换。";
                graph.AddStickyNote(extraNote1);
            }
            if (hasAnimationClip)
            {
                StickyNote extraNote1 = new StickyNote("节点信息", new Vector2(pos.x + 650f, orgPosY - 100f), 300f, 100f);
                extraNote1.content = "运行AnimationClipReplaceNode，对应AssetPath下的AnimationClip资产将被替换。";
                graph.AddStickyNote(extraNote1);
            }
            if (hasGOorSkinedMR)
            {
                StickyNote extraNote1 = new StickyNote("节点信息", new Vector2(pos.x + 650f, orgPosY - 100f), 300f, 100f);
                extraNote1.content = "运行AnimatorCreatorNode，在Asset中会创建一个Controller包含AnimationClip的State，" +
                    "GameObject上的Animator组件会挂载新的Controller，如果对应的GameObject不包含Animator组件则会创建Animator组件。";
                graph.AddStickyNote(extraNote1);
            }

            onGraphCreated?.Invoke(graph);

            if (errorMessage != "")
                TJAIGraphWindow.Open(graph).ShowNotification(new GUIContent(errorMessage));
            else
                TJAIGraphWindow.Open(graph);
        }

        private void Get3DTemplateWithTarget(Vector2 pos, string graphName, ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
            ref List<((int, string, string), (int, string, string))> edgeList, StickyNote stickyNote, Action<TJAIGraph> onGraphCreated = null)
        {
            var lastNodeIndex = nodeList.Count - 1;

            List<(int, int)> pairNode = new List<(int, int)>();
            string errorMessage = "";
            bool hasMeshFilterAndMeshRenderer = false;
            bool hasSkinnedMR = false;
            bool hasPSMR = false;
            float orgPosY = pos.y;

            foreach (var target in targets)
            {
                if (target is GameObject)
                {
                    var go = target as GameObject;
                    if (go.GetComponent<MeshFilter>() == null && go.GetComponent<MeshRenderer>() == null)
                    {
                        errorMessage += $"Warning: No suitable component on {target.name} as target of model generation!\n";
                        continue;
                    }
                    else if (go.GetComponent<MeshFilter>() == null && go.GetComponent<MeshRenderer>() != null)
                    {

                        go.AddComponent<MeshFilter>();

                        if (go.GetComponent<MeshFilter>() == null)
                        {
                            errorMessage += $"Warning: Lack of MeshFilter on {target.name} due to Component Confliction!\n";
                            continue;
                        }
                    }
                    else if (go.GetComponent<MeshRenderer>() == null && go.GetComponent<MeshFilter>() != null)
                    {

                        go.AddComponent<MeshRenderer>();

                        if (go.GetComponent<MeshRenderer>() == null)
                        {
                            errorMessage += $"Warning: Lack of MeshRenderer on {target.name} due to Component Confliction!\n";
                            continue;
                        }
                    }
                    
                    if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshRenderer>() != null)
                    {
                        hasMeshFilterAndMeshRenderer = true;
                        Action<BaseNode> mfAction = node => ((MeshFilterNode)node).owner = (target as GameObject);
                        nodeList.Add((typeof(MeshFilterNode), pos, mfAction));
                        edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (lastNodeIndex, "m_Mesh", null)));
                        Action<BaseNode> mrAction = node => ((MeshRendererNode)node).renderer = (target as GameObject).GetComponent<MeshRenderer>();
                        nodeList.Add((typeof(MeshRendererNode), new Vector2(pos.x + 350f, pos.y), mrAction));
                        edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                        pairNode.Add((nodeList.Count - 2, nodeList.Count - 1));
                    }
                }
                else if (target is SkinnedMeshRenderer)
                {
                    hasSkinnedMR = true;
                    Action<BaseNode> mrAction = node => ((SkinnedMeshRendererNode)node).renderer = target as SkinnedMeshRenderer;
                    nodeList.Add((typeof(SkinnedMeshRendererNode), new Vector2(pos.x, pos.y), mrAction));
                    edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (lastNodeIndex, "m_Mesh", null)));
                    edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                }
                else if (target is ParticleSystemRenderer)
                {
                    hasPSMR = true;
                    Action<BaseNode> mrAction = node => ((ParticleSystemRendererNode)node).renderer = target as ParticleSystemRenderer;
                    nodeList.Add((typeof(ParticleSystemRendererNode), new Vector2(pos.x, pos.y), mrAction));
                    edgeList.Add(((nodeList.Count - 1, "m_Mesh", null), (lastNodeIndex, "m_Mesh", null)));
                    edgeList.Add(((nodeList.Count - 1, "m_Materials", null), (lastNodeIndex, "m_Materials", null)));
                }

                pos.y += 400f;
            }
            var graph = CreateGraphTemplate(nodeList, edgeList);

            foreach(var pair in pairNode)
            {
                graph.nodes[pair.Item1].onProcessed += () => (graph.nodes[pair.Item2] as MeshRendererNode).NotifyFieldChanged("m_GO"); 
            }

            graph.name = graphName;
            graph.AddStickyNote(stickyNote);
            if (hasMeshFilterAndMeshRenderer || hasSkinnedMR || hasPSMR)
            {
                StickyNote extraNote = new StickyNote("节点信息", new Vector2(pos.x, orgPosY - 100f), 200f, 100f);
                extraNote.content = "";
                if (hasMeshFilterAndMeshRenderer)
                    extraNote.content += "运行MeshRendererNode和MeshFilterNode，对应的material和mesh将被替换。";
                if (hasSkinnedMR)
                    extraNote.content += "运行SkinnedMeshRendererNode，对应的material和mesh将被替换。";
                if (hasPSMR)
                    extraNote.content += "运行ParticleSystemRendererNode，对应的material和mesh将被替换。";

                graph.AddStickyNote(extraNote);
            }

            onGraphCreated?.Invoke(graph);
            if (errorMessage != "")
                TJAIGraphWindow.Open(graph).ShowNotification(new GUIContent(errorMessage));
            else
                TJAIGraphWindow.Open(graph);
        }

        public TJAIGraph CreateGraphTemplate(List<(Type, Vector2, Action<BaseNode>)> nodeList,
            List<((int, string, string), (int, string, string))> edgeList)
        {
            var graph = ScriptableObject.CreateInstance<TJAIGraph>();
            List<BaseNode> nodes = new List<BaseNode>();
            int computeOrder = 0;
            foreach (var nodeInfo in nodeList)
            {
                var node = BaseNode.CreateFromType(nodeInfo.Item1, nodeInfo.Item2);
                node.computeOrder = computeOrder++;
                nodeInfo.Item3?.Invoke(node);
                graph.AddNode(node);
                nodes.Add(node);
            }

            foreach (var edgeInfo in edgeList)
            {
                var inputPort = nodes[edgeInfo.Item1.Item1].GetPort(edgeInfo.Item1.Item2, edgeInfo.Item1.Item3);
                var outputPort = nodes[edgeInfo.Item2.Item1].GetPort(edgeInfo.Item2.Item2, edgeInfo.Item2.Item3);
                graph.Connect(inputPort, outputPort);
            }

            return graph;
        }

        private Texture2D LoadTexture(string textureName)
        {
            return Resources.Load<Texture2D>($"Icons/{textureName}");
        }

        private void SetCategory(Category category)
        {
            // 特殊处理"All"类别
            if (category == Category.All)
            {
                m_ActiveCategories = Category.All;
                UpdateButtonStyles();
                RefreshContent();
                return;
            }

            // 切换类别选择
            if (m_ActiveCategories.HasFlag(category))
            {
                // 如果类别已选中，则取消选择
                m_ActiveCategories &= ~category;

                // 如果取消后没有选中任何类别，则自动选择All
                if (m_ActiveCategories == Category.None)
                {
                    m_ActiveCategories = Category.All;
                }
            }
            else
            {
                // 确保选择新类别时取消All状态
                if (m_ActiveCategories.HasFlag(Category.All))
                {
                    m_ActiveCategories = category;
                }
                else
                {
                    // 添加新类别
                    m_ActiveCategories |= category;
                }
            }

            UpdateButtonStyles();
            RefreshContent();
        }

        private void UpdateButtonStyles()
        {
            // 按钮样式设置
            var activeStyle = new StyleColor(new Color(0.3f, 0.5f, 0.8f));
            var inactiveStyle = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            // 更新所有按钮状态
            allButton.style.backgroundColor = m_ActiveCategories.HasFlag(Category.All) ?
                activeStyle : inactiveStyle;

            twoDButton.style.backgroundColor = m_ActiveCategories.HasFlag(Category.TwoD) ?
                activeStyle : inactiveStyle;

            threeDButton.style.backgroundColor = m_ActiveCategories.HasFlag(Category.ThreeD) ?
                activeStyle : inactiveStyle;

            textureButton.style.backgroundColor = m_ActiveCategories.HasFlag(Category.Material) ?
                activeStyle : inactiveStyle;

            characterButton.style.backgroundColor = m_ActiveCategories.HasFlag(Category.Animation) ?
                activeStyle : inactiveStyle;
        }

        private void RefreshContent()
        {
            // 获取网格容器
            var gridContainer = contentScrollView.contentContainer[0] as VisualElement;
            gridContainer.Clear();

            // 过滤项目
            IEnumerable<TemplateItem> filteredItems;

            if (m_ActiveCategories == Category.All)
            {
                // 显示所有项目
                filteredItems = allItems;
            }
            else if (m_ActiveCategories == Category.None)
            {
                // 没有选择任何类别（理论上不会发生）
                filteredItems = Enumerable.Empty<TemplateItem>();
            }
            else
            {
                // 过滤包含任何激活类别的项目
                filteredItems = allItems.Where(item =>
                    (item.Categories & m_ActiveCategories) != Category.None);
            }

            // 创建项目卡片
            foreach (var item in filteredItems)
            {
                gridContainer.Add(CreateItemCard(item));
            }
        }

        //// 实现过滤方法：
        //private void FilterItems(string searchTerm)
        //{
        //    // 结合分类和搜索词过滤
        //    var filtered = allItems.Where(item =>
        //        (currentCategory == Category.All || item.Category == currentCategory) &&
        //        (string.IsNullOrEmpty(searchTerm) ||
        //         item.Title.Contains(searchTerm) ||
        //         item.Description.Contains(searchTerm))
        //    ).ToList();

        //    // 更新显示...
        //}

        private VisualElement CreateItemCard(TemplateItem item)
        {
            // 主卡片容器  
            var card = new VisualElement();
            card.AddToClassList("item-card");

            // 添加点击事件
            card.RegisterCallback<ClickEvent>(evt => OnItemClicked(item));

            // 添加悬停效果
            card.RegisterCallback<MouseEnterEvent>(evt => {
                card.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            });

            card.RegisterCallback<MouseLeaveEvent>(evt => {
                card.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            });

            // 图片占位符  
            var image = new Image();
            image.AddToClassList("item-card-image");
            image.image = item.PreviewImage;
            if (item.PreviewImage == null)
            {
                image.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
                image.style.alignItems = Align.Center;
                image.style.justifyContent = Justify.Center;
            }

            var imageText = new Label("Preview");
            imageText.AddToClassList("image-text");
            image.Add(imageText);

            // 标题  
            var title = new Label(item.Title);
            title.AddToClassList("item-card-title");

            // 描述  
            var description = new Label(item.Description);
            description.AddToClassList("item-card-description");

            // 组装卡片  
            card.Add(image);
            card.Add(title);
            card.Add(description);

            return card;
        }

        private void OnItemClicked(TemplateItem item)
        {
            item.OnCreateGraph?.Invoke(targets);
        }

        private class TemplateItem
        {
            public string Title;
            public string Description;
            public Category Categories;
            public Texture2D PreviewImage;
            public Action<UnityEngine.Object[]> OnCreateGraph;
            internal TJAIGraphWindow m_Window;

            public TemplateItem(string title, string description, Category categories,
                               Texture2D previewImage, Action<UnityEngine.Object[]> onCreateGraph)
            {
                Title = title;
                Description = description;
                Categories = categories;
                PreviewImage = previewImage;
                OnCreateGraph = onCreateGraph;
            }
        }
    }
}