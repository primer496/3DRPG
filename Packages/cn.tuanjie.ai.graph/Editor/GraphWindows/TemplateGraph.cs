using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;
using static GraphProcessor.ProviderGroup;

namespace UnityEditor.AIGraph
{
    internal static class TemplateGraph
    {
        static float s_DefaultDist = 350f;

        public static ProviderGroup GetVastInputSession(Vector2 position, TJAIGraph owner, List<int> groupNodes, List<(int, string, string, bool)> connectPorts, bool isConnectedWithVast = false)
        {
            var group = new ProviderGroup("Tripo Text/Image to Model", position, new List<string>(){ "Text", "Image" },
                new List<GroupNodeEdgeInfo>()
                { 
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(StringNode).AssemblyQualifiedName, typeof(VastTextToModelNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){ new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null} },
                        connectPorts = new List<GroupConnectPort>() { new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 1, portField = isConnectedWithVast ? "outputModelID" : "m_Obj", portIdentifier = null} },
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(TextureAssetNode).AssemblyQualifiedName, typeof(VastImageToModelNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){ new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "inputImage", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null} },
                        connectPorts = new List<GroupConnectPort>() { new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 1, portField = isConnectedWithVast ? "outputModelID" : "m_Obj", portIdentifier = null}},
                    }
                },
            200f);

            foreach (var index in groupNodes)
                group.addNodeBeforeInit(owner.nodes[index]);

            foreach (var connectInfo in connectPorts)
            {
                group.setConnectPort(owner.nodes[connectInfo.Item1], connectInfo.Item2, connectInfo.Item3, connectInfo.Item4);
            }
            group.color = new Color(0.8f, 0.5f, 0.9f, 0.2f);
            StickyNote note = new StickyNote("选择输入方式", new Vector2(600f, -10f), 250f, 100f);
            note.content = "下拉条可切换输入方式，包括Text和Image两种，切换后先前历史信息会丢失请注意保存资产! 若删除初始节点可能导致Edge无法在切换后保持连接，需手动重连！";
            owner.AddStickyNote(note);
            return group;
        }

        public static ProviderGroup GetRodinInputSession(Vector2 position, TJAIGraph owner, List<int> groupNodes, List<(int, string, string, bool)> connectPorts)
        { 
            var group = new ProviderGroup("Rodin3D Text/Image to Model", position, new List<string>() { "Text", "Image" },
                new List<GroupNodeEdgeInfo>()
                {
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(StringNode).AssemblyQualifiedName, typeof(Rodin3DGenerationRegularNode).AssemblyQualifiedName, typeof(Rodin3DGenerationSketchNode).AssemblyQualifiedName, typeof(Rodin3DGenerationSmoothNode).AssemblyQualifiedName,
                                                     typeof(Rodin3DGenerationDetailNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){ new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null},
                                                      new EdgeInfo() { inputNodeIndex = 2, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null},
                                                      new EdgeInfo() { inputNodeIndex = 3, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null},
                                                      new EdgeInfo() { inputNodeIndex = 4, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null}},
                        connectPorts = new List<GroupConnectPort>() { new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 1, portField = "m_Obj", portIdentifier = null} },
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(TextureAssetNode).AssemblyQualifiedName, typeof(Rodin3DGenerationRegularNode).AssemblyQualifiedName , typeof(Rodin3DGenerationSketchNode).AssemblyQualifiedName, typeof(Rodin3DGenerationSmoothNode).AssemblyQualifiedName,
                                                     typeof(Rodin3DGenerationDetailNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){ new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "images", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null},
                                                      new EdgeInfo() { inputNodeIndex = 2, inputNodePortField = "images", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null},
                                                      new EdgeInfo() { inputNodeIndex = 3, inputNodePortField = "images", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null},
                                                      new EdgeInfo() { inputNodeIndex = 4, inputNodePortField = "images", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null}},
                        connectPorts = new List<GroupConnectPort>() { new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 1, portField = "m_Obj", portIdentifier = null}},
                    }
                },
            200f);

            foreach (var index in groupNodes)
                group.addNodeBeforeInit(owner.nodes[index]);

            foreach (var connectInfo in connectPorts)
            {
                group.setConnectPort(owner.nodes[connectInfo.Item1], connectInfo.Item2, connectInfo.Item3, connectInfo.Item4);
            }
            group.color = new Color(0.8f, 0.5f, 0.9f, 0.2f);
            StickyNote note = new StickyNote("选择输入方式", new Vector2(610f, -10f), 250f, 100f);
            note.content = "下拉条可切换输入方式，包括Text和Image两种，切换后先前历史信息会丢失请注意保存资产! 若删除初始节点可能导致Edge无法在切换后保持连接，需手动重连！";
            owner.AddStickyNote(note);
            return group;
        }


        public static ProviderGroup GetHunyuanInputSession(Vector2 position, TJAIGraph owner, List<int> groupNodes, List<(int, string, string, bool)> connectPorts)
        {
            var group = new ProviderGroup("Hunyuan Text/Image to Model", position, new List<string>() { "Text", "Text to Image", "Image" },
                new List<GroupNodeEdgeInfo>()
                {
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(StringNode).AssemblyQualifiedName, typeof(HyTextToGeometryNode).AssemblyQualifiedName, typeof(ModelSnapshotNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){
                            new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null},
                            new EdgeInfo() { inputNodeIndex = 2, inputNodePortField = "m_Go", inputNodePortIdentifier = null, outputNodeIndex = 1, outputNodePortField = "m_Obj", outputNodePortIdentifier = null },
                            },
                        connectPorts = new List<GroupConnectPort>() {
                            new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 1, portField = "outputModelUrl", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 1, nodeIndex = 2, portField = "m_OutputTexture", portIdentifier = null},
                        },
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(StringNode).AssemblyQualifiedName, typeof(HyImageGeneratingNode).AssemblyQualifiedName, typeof(HyImageToGeometryNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){ 
                            new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "prompt", inputNodePortIdentifier = null, outputNodeIndex = 0, outputNodePortField = "output", outputNodePortIdentifier = null},
                            new EdgeInfo() { inputNodeIndex = 2, inputNodePortField = "image", inputNodePortIdentifier = null, outputNodeIndex = 1, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null } 
                            },
                        connectPorts = new List<GroupConnectPort>() {
                            new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 2, portField = "outputModelUrl", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 1, nodeIndex = 1, portField = "m_OutputTexture", portIdentifier = null},
                            },
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(TextureAssetNode).AssemblyQualifiedName, typeof(HyImageToGeometryNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(){
                            new EdgeInfo() { inputNodeIndex = 1, inputNodePortField = "image", inputNodePortIdentifier =null, outputNodeIndex = 0, outputNodePortField = "m_OutputTexture", outputNodePortIdentifier = null}
                            },
                        connectPorts = new List<GroupConnectPort>() {
                            new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 1, portField = "outputModelUrl", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 1, nodeIndex = 0, portField = "m_OutputTexture", portIdentifier = null},
                            },
                    },
                },
            200f);

            foreach (var index in groupNodes)
                group.addNodeBeforeInit(owner.nodes[index]);

            foreach (var connectInfo in connectPorts)
            {
                group.setConnectPort(owner.nodes[connectInfo.Item1], connectInfo.Item2, connectInfo.Item3, connectInfo.Item4);
            }
            group.color = new Color(0.8f, 0.5f, 0.9f, 0.2f);

            StickyNote note = new StickyNote("选择输入方式", new Vector2(650f, -210f), 250f, 100f);
            note.content = "下拉条可切换输入方式，包括Text, Text to Image, Image三种，切换后先前历史信息会丢失请注意保存资产! 若删除初始节点可能导致Edge无法在切换后保持连接，需手动重连！";
            owner.AddStickyNote(note);
            return group;
        }

        public static float HunyuanTextToImage(float posx, float posy, 
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(HyImageGeneratingNode), new Vector2(posx + s_DefaultDist, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
            };

            note = new StickyNote("混元文生图", new Vector2(posx, posy - 100f), 150f, 100f);
            note.content = "输入图片描述Prompt生成图片。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanImageToImage(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 250f), null),
                (typeof(HyImageGeneratingNode), new Vector2(posx + s_DefaultDist, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((2, "prompt", null), (0, "output", null)),
                ((2, "image", null), (1, "m_OutputTexture", null)),
            };

            note = new StickyNote("混元图生图", new Vector2(posx, posy - 100f), 150f, 100f);
            note.content = "输入图片描述Prompt并选择参考图生成图片。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanTextToSprite(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(HyImageGeneratingNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(HyImageSubjectSegmentationNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(TextureDownSampleNode), new Vector2(posx + s_DefaultDist * 3, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "image", null), (1, "m_OutputTexture", null)),
                ((3, "m_Input", null), (2, "m_OutputTexture", null)),
            };

            note = new StickyNote("混元文生精灵图", new Vector2(posx, posy - 100f), 200f, 100f);
            note.content = "输入图片描述Prompt生成图片。" +
                            "去除所得图片的背景，并按照给定参数降采样。\n";

            return posx + 4 * s_DefaultDist;
        }
        public static float HunyuanImageToSprite(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(HyImageGeneratingNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(HyImageSubjectSegmentationNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(TextureDownSampleNode), new Vector2(posx + s_DefaultDist * 3, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((2, "prompt", null), (0, "output", null)),
                ((2, "image", null), (1, "m_OutputTexture", null)),
                ((3, "image", null), (2, "m_OutputTexture", null)),
                ((4, "m_Input", null), (3, "m_OutputTexture", null)),
            };

            note = new StickyNote("混元图生精灵图", new Vector2(posx, posy - 100f), 250f, 100f);
            note.content = "输入图片描述Prompt并选择参考图生成图片。\n" +
                            "去除所得图片的背景，并按照给定参数降采样。\n";

            return posx + 4 * s_DefaultDist;
        }
        public static float HunyuanImageColoring(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(HyImageControlnetGrayScaleNode), new Vector2(posx + s_DefaultDist, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((2, "prompt", null), (0, "output", null)),
                ((2, "image", null), (1, "m_OutputTexture", null))
            };

            note = new StickyNote("混元图片上色", new Vector2(posx, posy - 100f), 150f, 100f);
            note.content = "输入颜色描述Prompt并选择黑白图生成彩色图片。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanImageStyleSwitch(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy), null),
                (typeof(HyImageStyleSwitchNode), new Vector2(posx + s_DefaultDist, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "image", null), (0, "m_OutputTexture", null))
            };

            note = new StickyNote("混元图片风格转换", new Vector2(posx, posy - 100f), 200f, 100f);
            note.content = "输入图片并选择指定风格生成风格转换图片。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanCharacter3View(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy), null),
                (typeof(HyImageThreeViewNode), new Vector2(posx + s_DefaultDist, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "image", null), (0, "m_OutputTexture", null))
            };

            note = new StickyNote("混元角色图生三视图", new Vector2(posx, posy - 100f), 200f, 100f);
            note.content = "输入角色图片生成角色三视图。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanCharacterEditing(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(HyImageFlexibilityConsistencyNode), new Vector2(posx + s_DefaultDist, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((2, "prompt", null), (0, "output", null)),
                ((2, "image", null), (1, "m_OutputTexture", null))
            };

            note = new StickyNote("混元角色图编辑", new Vector2(posx, posy - 100f), 270f, 100f);
            note.content = "输入角色编辑描述Prompt并选择角色图生成新角色图片。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuaRemoveBG(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy), null),
                (typeof(HyImageSubjectSegmentationNode), new Vector2(posx + s_DefaultDist, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "image", null), (0, "m_OutputTexture", null))
            };

            note = new StickyNote("混元去除图片背景", new Vector2(posx, posy - 100f), 200f, 100f);
            note.content = "输入图片去除图片背景。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanReplaceBG(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(DoodlePadNode), new Vector2(posx + 2 * s_DefaultDist, posy + 500f), null),
                (typeof(HyImageSubjectSegmentationNode), new Vector2(posx + s_DefaultDist, posy + 200f), null),
                (typeof(HyBackgroundReplacementNode), new Vector2(posx + 2 * s_DefaultDist, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "prompt", null), (0, "output", null)),
                ((4, "image", null), (1, "m_OutputTexture", null)),
                ((3, "image", null), (1, "m_OutputTexture", null)),
                ((4, "mask", null), (3, "maskImage", null)),
                ((2, "inputImage", null), (1, "m_OutputTexture", null))
            };

            note = new StickyNote("混元替换图片背景", new Vector2(posx, posy - 100f), 270f, 100f);
            note.content = "根据给入图片进行背景分割获取mask，也可以利用绘制节点修改Mask，输入编辑prompt对mask外区域进行替换\n";

            return posx + 3 * s_DefaultDist;
        }
        public static float HunyuanSuperResolution(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy), null),
                (typeof(HyImageClarityNode), new Vector2(posx + s_DefaultDist, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "image", null), (0, "m_OutputTexture", null))
            };

            note = new StickyNote("混元图片超分", new Vector2(posx, posy - 100f), 200f, 100f);
            note.content = "对输入图片进行超分，提高像素。\n";

            return posx + 2 * s_DefaultDist;
        }
        public static float HunyuanTextToModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy - 200f), null),
                (typeof(HyTextToGeometryNode), new Vector2(posx - 50f + s_DefaultDist, posy - 200f), null),
                (typeof(ModelSnapshotNode), new Vector2(posx - 50f + s_DefaultDist * 2, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy + 200f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 5, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "m_Go", null), (1, "m_Obj", null)),
                ((5, "image", null), (2, "m_OutputTexture", null)),
                ((3, "inputModelUrl", null), (1, "outputModelUrl", null)),
                ((4, "inputModelUrl", null), (3, "outputModelUrl", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "m_Go", null), (5, "m_Obj", null))
            };

            note = new StickyNote("混元文/图生模型", new Vector2(posx - 50f, posy + 150f), 400f, 150f);
            note.content = "输入描述prompt或图片生成模型，有三种输入方式：\n" +
                            "1.直接输入prompt生成模型，注意在生成Geometry后需要截图采样生成的设计图，可手动选择截图角度(Text)\n" +
                            "2.先根据输入prompt生成图片，再由图片生成模型(Text to Image)\n" +
                            "3.直接根据图片生成模型(Image)\n"
                            + "生成模型后对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n";

            return posx + 6 * s_DefaultDist;
        }
        public static float HunyuanSketchToModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy - 100f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy + 20f), null),
                (typeof(HySketch2MeshNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(ModelSnapshotNode), new Vector2(posx + s_DefaultDist * 2, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy + 100f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 5, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((2, "prompt", null), (0, "output", null)),
                ((2, "sketch", null), (1, "m_OutputTexture", null)),
                ((3, "m_Go", null), (2, "m_Obj", null)),
                ((4, "inputModelUrl", null), (2, "outputModelUrl", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "inputModelUrl", null), (5, "outputModelUrl", null)),
                ((7, "m_Go", null), (6, "m_Obj", null)),
                ((6, "image", null), (3, "m_OutputTexture", null)),
            };

            note = new StickyNote("混元草图生模型", new Vector2(posx, posy - 200f), 300f, 100f);
            note.content = "选择模型草图生成模型，对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n"
                         + "注意在生成Geometry后会进行一个截图采样生成的设计图，可手动选择截图角度。\n";

            return posx + 6 * s_DefaultDist;
        }
        public static float HunyuanMultiviewToModel(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(HyViewsToGeometryNode), new Vector2(posx +  s_DefaultDist, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy + 100f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 5, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "inputModelUrl", null), (5, "outputModelUrl", null)),
                ((7, "inputModelUrl", null), (6, "outputModelUrl", null)),
                ((8, "m_Go", null), (7, "m_Obj", null)),
                ((7, "image", null), (0, "m_OutputTexture", null)),
            };

            note = new StickyNote("混元多视图生模型", new Vector2(posx - 300f, posy - 200f), 300f, 100f);
            note.content = "利用多视角设计图生成模型，对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n" +
                "至少需要提供FrontImage和BackImage/LeftImage/RightImage中的一个。\n";

            return posx + 6 * s_DefaultDist;
        }
        public static float HunyuanTextToCharacter(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy - 200f), null),
                (typeof(HyTextToGeometryNode), new Vector2(posx + s_DefaultDist, posy - 200f), null),
                (typeof(ModelSnapshotNode), new Vector2(posx + s_DefaultDist * 2, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy + 200f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
                (typeof(HyAutoRiggingNode), new Vector2(posx + s_DefaultDist * 5, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 5, posy + 400f), null),
                (typeof(HyMotionRetargetNode), new Vector2(posx + s_DefaultDist * 6, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "m_Go", null), (1, "m_Obj", null)),
                ((5, "image", null), (2, "m_OutputTexture", null)),
                ((3, "inputModelUrl", null), (1, "outputModelUrl", null)),
                ((4, "inputModelUrl", null), (3, "outputModelUrl", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "inputModelUrl", null), (5, "outputModelUrl", null)),
                ((7, "m_Go", null), (6, "m_Obj", null)),
                ((8, "inputModelUrl", null), (6, "outputModelUrl", null))
            };

            note = new StickyNote("混元文/图生角色", new Vector2(posx - 50f, posy + 150f), 400f, 200f);
            note.content = "输入描述prompt或图片生成模型，有三种输入方式：\n" +
                            "1.直接输入prompt生成模型，注意在生成Geometry后需要截图采样生成的设计图，可手动选择截图角度(Text)\n" +
                            "2.先根据输入prompt生成图片，再由图片生成模型(Text to Image)\n" +
                            "3.直接根据图片生成模型(Image)\n"
                            + "生成模型后对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n"
                            + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 5.5f * s_DefaultDist;
        }
        public static float HunyuanTextToImgToCharacter(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode), new Vector2(posx, posy), null),
                (typeof(HyImageGeneratingNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(HyImageToGeometryNode), new Vector2(posx + s_DefaultDist * 2, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 3, posy + 100f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 4, posy - 100f), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 5, posy), null),
                (typeof(HyAutoRiggingNode), new Vector2(posx + s_DefaultDist * 6, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 6, posy + 400f), null),
                (typeof(HyMotionRetargetNode), new Vector2(posx + s_DefaultDist * 7, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "image", null), (1, "m_OutputTexture", null)),
                ((5, "image", null), (1, "m_OutputTexture", null)),
                ((3, "inputModelUrl", null), (2, "outputModelUrl", null)),
                ((4, "inputModelUrl", null), (3, "outputModelUrl", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "inputModelUrl", null), (5, "outputModelUrl", null)),
                ((7, "m_Go", null), (6, "m_Obj", null)),
                ((8, "inputModelUrl", null), (6, "outputModelUrl", null)),
            };

            note = new StickyNote("混元文生图生角色", new Vector2(posx, posy - 150f), 300f, 150f);
            note.content = "输入prompt描述生成图片，再由图片生成模型，对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n"
                         + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 6.5f * s_DefaultDist;
        }
        public static float HunyuanViewsToCharacter(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy + 600f), null),
                (typeof(HyViewsToGeometryNode), new Vector2(posx + s_DefaultDist, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy + 100f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 3, posy - 100f), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
                (typeof(HyAutoRiggingNode), new Vector2(posx + s_DefaultDist * 5, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 5, posy + 400f), null),
                (typeof(HyMotionRetargetNode), new Vector2(posx + s_DefaultDist * 6, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {                            
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((7, "image", null), (0, "m_OutputTexture", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "inputModelUrl", null), (5, "outputModelUrl", null)),
                ((7, "inputModelUrl", null), (6, "outputModelUrl", null)),
                ((8, "inputModelUrl", null), (6, "outputModelUrl", null)),
                ((9, "m_Go", null), (8, "m_Obj", null)),
                ((10, "inputModelUrl", null), (8, "outputModelUrl", null)),
            };

            note = new StickyNote("混元多视图生角色", new Vector2(posx - 200f, posy - 200f), 200f, 150f);
            note.content = "利用多视角设计图，对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n"
                         + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 5.5f * s_DefaultDist;
        }
        public static float HunyuanSketchToCharacter(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy - 100f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy + 20f), null),
                (typeof(HySketch2MeshNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(ModelSnapshotNode), new Vector2(posx + s_DefaultDist * 2, posy - 200f), null),
                (typeof(HyLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy + 100f), null),
                (typeof(HySemanticUVNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
                (typeof(HyAutoRiggingNode), new Vector2(posx + s_DefaultDist * 5, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 5, posy + 400f), null),
                (typeof(HyMotionRetargetNode), new Vector2(posx + s_DefaultDist * 6, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((2, "prompt", null), (0, "output", null)),
                ((2, "sketch", null), (1, "m_OutputTexture", null)),
                ((3, "m_Go", null), (2, "m_Obj", null)),
                ((4, "inputModelUrl", null), (2, "outputModelUrl", null)),
                ((5, "inputModelUrl", null), (4, "outputModelUrl", null)),
                ((6, "inputModelUrl", null), (5, "outputModelUrl", null)),
                ((7, "inputModelUrl", null), (6, "outputModelUrl", null)),
                ((8, "m_Go", null), (7, "m_Obj", null)),
                ((9, "inputModelUrl", null), (7, "outputModelUrl", null)),
                ((6, "image", null), (3, "m_OutputTexture", null)),
            };

            note = new StickyNote("混元草图生角色", new Vector2(posx, posy - 250f), 300f, 150f);
            note.content = "选择模型草图生成模型，对模型进行重新布线并整体展开UV，再生成材质（可选择PBR材质）。\n"
                         + "注意在生成Geometry后会进行一个截图采样生成的设计图，可手动选择截图角度。\n"
                         + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 5.5f * s_DefaultDist;
        }
        public static float VastTextToModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(VastTextToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 2, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "m_Go", null), (1, "m_Obj", null)),
            };

            note = new StickyNote("Tripo文/图生模型", new Vector2(posx, posy + 350f), 200f, 100f);
            note.content = "输入prompt描述或图片生成模型。\n";

            return posx + 3 * s_DefaultDist;
        }
        public static float VastMultiviewToModel(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(VastMultiviewToModelNode), new Vector2(posx +  s_DefaultDist, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 2, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "m_Go", null), (4, "m_Obj", null)),
            };

            note = new StickyNote("Tripo多视图生模型", new Vector2(posx - 200f, posy - 200f), 200f, 100f);
            note.content = "利用多视角设计图生成模型。\n";

            return posx + 3 * s_DefaultDist;
        }
        public static float VastTextToLowPolyModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(VastTextToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "inputModelID", null), (1, "outputModelID", null)),
                ((3, "m_Go", null), (2, "m_Obj", null)),
            };

            note = new StickyNote("Tripo文/图生低模", new Vector2(posx, posy + 350f), 200f, 100f);
            note.content = "输入prompt描述或图片生成模型。完成后对模型进行简化。\n";

            return posx + 4 * s_DefaultDist;
        }
        public static float VastMultiViewToLowPolyModel(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(VastMultiviewToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "inputModelID", null), (4, "outputModelID", null)),
                ((6, "m_Go", null), (5, "m_Obj", null)),
            };

            note = new StickyNote("Tripo多视图生低模", new Vector2(posx - 200f, posy - 200f), 200f, 100f);
            note.content = "利用多视角设计图生成模型。完成后对模型进行简化。\n";

            return posx + 4 * s_DefaultDist;
        }
        public static float VastTextToStyleModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(VastTextToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastStylizeModelNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "inputModelID", null), (1, "outputModelID", null)),
                ((3, "m_Go", null), (2, "m_Obj", null)),
            };

            note = new StickyNote("Tripo文/图生风格化模型", new Vector2(posx, posy + 350f), 200f, 100f);
            note.content = "输入prompt描述或图片生成模型。完成后对模型进行风格化。\n";

            return posx + 4 * s_DefaultDist;
        }
        public static float VastMultiViewToStyleModel(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(VastMultiviewToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastStylizeModelNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy), null)
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "inputModelID", null), (4, "outputModelID", null)),
                ((6, "m_Go", null), (5, "m_Obj", null)),
            };

            note = new StickyNote("Tripo多视图生风格化模型", new Vector2(posx - 200f, posy - 200f), 200f, 100f);
            note.content = "利用多视角设计图生成模型。完成后对模型进行风格化。\n";

            return posx + 4 * s_DefaultDist;
        } 
        public static float VastTextToCharacter(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(VastTextToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastRigNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 2, posy + 400f), null),
                (typeof(VastRetargetNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((3, "m_Go", null), (2, "m_Obj", null)),
                ((2, "inputModelID", null), (1, "outputModelID", null)),
                ((4, "inputModelID", null), (2, "outputModelID", null)),
            };

            note = new StickyNote("Tripo文/图生角色", new Vector2(posx, posy + 350f), 200f, 100f);
            note.content = "输入prompt描述生成角色。\n"
                + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n"; ;

            return posx + 2.5f * s_DefaultDist;
        }
        public static float VastMultiviewToCharacter(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(VastMultiviewToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastRigNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 2, posy + 400f), null),
                (typeof(VastRetargetNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "inputModelID", null), (4, "outputModelID", null)),
                ((6, "m_Go", null), (5, "m_Obj", null)),
                ((7, "inputModelID", null), (5, "outputModelID", null)),
            };

            note = new StickyNote("Tripo多视图生角色", new Vector2(posx - 200f, posy - 200f), 200f, 100f);
            note.content = "利用多视角设计图生成角色。\n"
                 + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 2.5f * s_DefaultDist;
        }
        public static float VastTextToLowPolyCharacter(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(VastTextToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(VastRigNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy + 400f), null),
                (typeof(VastRetargetNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((4, "m_Go", null), (3, "m_Obj", null)),
                ((2, "inputModelID", null), (1, "outputModelID", null)),
                ((3, "inputModelID", null), (2, "outputModelID", null)),
                ((5, "inputModelID", null), (3, "outputModelID", null)),
            };

            note = new StickyNote("Tripo文/图生低模角色", new Vector2(posx, posy + 350f), 320f, 100f);
            note.content = "输入prompt描述生成角色。对得到的角色模型进行简化。\n"
                + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n"; ;

            return posx + 3.5f * s_DefaultDist;
        }
        public static float VastMultiviewToLowPolyCharacter(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(VastMultiviewToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastLowpolyNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(VastRigNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy + 400f), null),
                (typeof(VastRetargetNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "inputModelID", null), (4, "outputModelID", null)),
                ((6, "inputModelID", null), (5, "outputModelID", null)),
                ((7, "m_Go", null), (6, "m_Obj", null)),
                ((8, "inputModelID", null), (6, "outputModelID", null)),
            };

            note = new StickyNote("Tripo多视图生低模角色", new Vector2(posx - 200f, posy - 200f), 200f, 200f);
            note.content = "利用多视角设计图生成角色。对得到的角色模型进行简化。\n"
                 + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 3.5f * s_DefaultDist;
        }
        public static float VastTextToStylizeCharacter(float posx, float posy,
          out List<(Type, Vector2, Action<BaseNode>)> nodeList,
          out List<((int, string, string), (int, string, string))> edgeList,
          out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(VastTextToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastStylizeModelNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(VastRigNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy + 400f), null),
                (typeof(VastRetargetNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((4, "m_Go", null), (3, "m_Obj", null)),
                ((2, "inputModelID", null), (1, "outputModelID", null)),
                ((3, "inputModelID", null), (2, "outputModelID", null)),
                ((5, "inputModelID", null), (3, "outputModelID", null)),
            };

            note = new StickyNote("Tripo文生风格化角色", new Vector2(posx, posy + 350f), 320f, 100f);
            note.content = "输入prompt描述或图片生成角色。对得到的角色模型进行风格化。\n"
                + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n"; ;

            return posx + 3.5f * s_DefaultDist;
        }
        public static float VastMultiviewToStylizeCharacter(float posx, float posy,
           out List<(Type, Vector2, Action<BaseNode>)> nodeList,
           out List<((int, string, string), (int, string, string))> edgeList,
           out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode), new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode), new Vector2(posx, posy + 600f), null),
                (typeof(VastMultiviewToModelNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(VastStylizeModelNode), new Vector2(posx + s_DefaultDist * 2, posy), null),
                (typeof(VastRigNode), new Vector2(posx + s_DefaultDist * 3, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 3, posy + 400f), null),
                (typeof(VastRetargetNode), new Vector2(posx + s_DefaultDist * 4, posy), null),
            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((4, "frontImage", null), (0, "m_OutputTexture", null)),
                ((4, "backImage", null), (1, "m_OutputTexture", null)),
                ((4, "leftImage", null), (2, "m_OutputTexture", null)),
                ((4, "rightImage", null), (3, "m_OutputTexture", null)),
                ((5, "inputModelID", null), (4, "outputModelID", null)),
                ((6, "inputModelID", null), (5, "outputModelID", null)),
                ((7, "m_Go", null), (6, "m_Obj", null)),
                ((8, "inputModelID", null), (6, "outputModelID", null)),
            };

            note = new StickyNote("Tripo多视图生风格化角色", new Vector2(posx - 200f, posy - 200f), 200f, 200f);
            note.content = "利用多视角设计图生成角色。对得到的角色模型进行风格化。\n"
                 + "完成角色模型生成后利用Rigging和Retargeting节点分别完成骨骼绑定和动画素材生成。\n";

            return posx + 3.5f * s_DefaultDist;
        }
        public static float RodinTextToModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(posx, posy), null),
                (typeof(Rodin3DGenerationSketchNode), new Vector2(posx + 2 * s_DefaultDist, posy), null),
                (typeof(Rodin3DGenerationSmoothNode), new Vector2(posx + 3 * s_DefaultDist, posy ), null),
                (typeof(Rodin3DGenerationDetailNode), new Vector2(posx + 4 * s_DefaultDist, posy), null),
                (typeof(Rodin3DGenerationRegularNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 2, posy + 700f), null),

            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((1, "prompt", null), (0, "output", null)),
                ((2, "prompt", null), (0, "output", null)),
                ((3, "prompt", null), (0, "output", null)),
                ((4, "prompt", null), (0, "output", null)),
                ((5, "m_Go", null), (4, "m_Obj", null)),
            };

            note = new StickyNote("Rodin3D文/图生模型", new Vector2(posx, posy + 300f), 300f, 120f);
            note.content = "输入prompt描述或图片生成模型。有四种生成节点可选择，分别为regular（常规），sketch（快速），Smooth（光滑）和Detail（细节），可挑选合适的模型进行生成。\n";

            return posx + 3 * s_DefaultDist;
        }

        public static ProviderGroup GetRodinModelSession(Vector2 position, TJAIGraph owner, List<int> groupNodes, List<(int, string, string, bool)> connectPorts)
        {
            var connectPortList = new List<GroupConnectPort>() {
                            new GroupConnectPort() { connectPortInfoIndex = 0, nodeIndex = 0, portField = "m_Obj", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 1, nodeIndex = 0, portField = "images", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 2, nodeIndex = 0, portField = "images", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 3, nodeIndex = 0, portField = "images", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 4, nodeIndex = 0, portField = "images", portIdentifier = null},
                            new GroupConnectPort() { connectPortInfoIndex = 5, nodeIndex = 0, portField = "images", portIdentifier = null}
                        };
            var group = new ProviderGroup("Rodin3D Model", position, new List<string>() { "Regular", "Sketch", "Smooth", "Detail" },
                new List<GroupNodeEdgeInfo>()
                {
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(Rodin3DGenerationRegularNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(),
                        connectPorts = connectPortList
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(Rodin3DGenerationSketchNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(),
                        connectPorts = connectPortList,
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(Rodin3DGenerationSmoothNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(),
                        connectPorts = connectPortList,
                    },
                    new GroupNodeEdgeInfo()
                    {
                        nodes = new List<string>() { typeof(Rodin3DGenerationDetailNode).AssemblyQualifiedName },
                        edges = new List<EdgeInfo>(),
                        connectPorts = connectPortList,
                    }
                },
            100f);

            foreach (var index in groupNodes)
                group.addNodeBeforeInit(owner.nodes[index]);

            foreach (var connectInfo in connectPorts)
            {
                group.setConnectPort(owner.nodes[connectInfo.Item1], connectInfo.Item2, connectInfo.Item3, connectInfo.Item4);
            }
            group.color = new Color(0.8f, 0.5f, 0.9f, 0.2f);
            StickyNote note = new StickyNote("选择模型", new Vector2(610f, -10f), 300f, 100f);
            note.content = "下拉条可切换模型，包括regular（常规），sketch（快速），Smooth（光滑）和Detail（细节）四种，切换后先前历史信息会丢失请注意保存资产! 若删除初始节点可能导致Edge无法在切换后保持连接，需手动重连！";
            owner.AddStickyNote(note);
            return group;
        }

        public static float RodinViewsToModel(float posx, float posy,
            out List<(Type, Vector2, Action<BaseNode>)> nodeList,
            out List<((int, string, string), (int, string, string))> edgeList,
            out StickyNote note)
        {
            nodeList = new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode),  new Vector2(posx - 300f, posy), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 600f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 200f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy + 200f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy + 600f), null),
                (typeof(Rodin3DGenerationRegularNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + s_DefaultDist * 2, posy + 700f), null),

            };
            edgeList = new List<((int, string, string), (int, string, string))>()
            {
                ((5, "images", null), (0, "m_OutputTexture", null)),
                ((5, "images", null), (1, "m_OutputTexture", null)),
                ((5, "images", null), (2, "m_OutputTexture", null)),
                ((5, "images", null), (3, "m_OutputTexture", null)),
                ((5, "images", null), (4, "m_OutputTexture", null)),
                ((6, "m_Go", null), (5, "m_Obj", null)),
            };

            note = new StickyNote("Rodin3D多视图生模型", new Vector2(posx - 300f, posy - 150f), 300f, 150f);
            note.content = "输入多视角设计图生成模型。有四种生成节点可选择，分别为regular，sketch（快速），Smooth（光滑）和Detail（细节），可挑选合适的模型进行生成。\n" +
                "最多可以传入5张设计图，其中第一张设计图被用作材质生成的参考。\n";

            return posx + 3 * s_DefaultDist;
        }

        public static void RodinImgToMtl(float posx, float posy, int uploadIndex,
           ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
           ref List<((int, string, string), (int, string, string))> edgeList,
           ref List<StickyNote> notes, ref bool needStickyNotes)
        {
            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(StringNode),  new Vector2(550f, posy - 100f), null),
                (typeof(TextureAssetNode),  new Vector2(550f, posy + 100f), null),
                (typeof(RodinTextureNode), new Vector2(900f, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(1250f, posy), null)
            });
            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
            {
                ((nodeList.Count - 2, "prompt", null), (nodeList.Count - 4, "output", null)),
                ((nodeList.Count - 2, "image", null), (nodeList.Count - 3, "m_OutputTexture", null)),
                ((nodeList.Count - 2, "modelUrl", null), (uploadIndex, "url", null)),
                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),
            });

            if (needStickyNotes)
            {
                needStickyNotes = false;
                var note = new StickyNote("Rodin3D图生材质", new Vector2(posx + s_DefaultDist, posy - 100f), 200f, 100f);
                note.content = "输入描述Prompt和设计图并上传模型生成材质。";
                notes.Add(note);
            }
        }

        public static void HunyuanImgToMtl(float posx, float posy, int uploadIndex,
           ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
           ref List<((int, string, string), (int, string, string))> edgeList,
           ref List<StickyNote> notes, ref bool needStickyNotes)
        {
            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 100f), null),
                (typeof(HyImageToTextureNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + 2 * s_DefaultDist, posy), null)
            });
            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
            {
                ((nodeList.Count - 2, "image", null), (nodeList.Count - 3, "m_OutputTexture", null)),
                ((nodeList.Count - 2, "inputModelUrl", null), (uploadIndex, "outputModelUrl", null)),
                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),            
            });

            if (needStickyNotes)
            {
                needStickyNotes = false;
                var note = new StickyNote("混元图生材质", new Vector2(posx + s_DefaultDist, posy - 100f), 200f, 100f);
                note.content = "输入设计图并上传模型生成材质。";
                notes.Add(note);
            }
        }

        public static void HunyuanViewsToMtl(float posx, float posy, int uploadIndex,
           ref List<(Type, Vector2, Action<BaseNode>)> nodeList,
           ref List<((int, string, string), (int, string, string))> edgeList,
           ref List<StickyNote> notes, ref bool needStickyNotes)
        {
            nodeList.AddRange(new List<(Type, Vector2, Action<BaseNode>)>()
            {
                (typeof(TextureAssetNode),  new Vector2(posx - 350f, posy - 700f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 700f), null),
                (typeof(TextureAssetNode),  new Vector2(posx - 350f, posy - 400f), null),
                (typeof(TextureAssetNode),  new Vector2(posx, posy - 400f), null),
                (typeof(HyViewsToTextureNode), new Vector2(posx + s_DefaultDist, posy), null),
                (typeof(MeshAndMaterialNode),  new Vector2(posx + 2 * s_DefaultDist, posy), null)
            });
            edgeList.AddRange(new List<((int, string, string), (int, string, string))>()
            {
                ((nodeList.Count - 2, "frontImage", null), (nodeList.Count - 6, "m_OutputTexture", null)),
                ((nodeList.Count - 2, "backImage", null), (nodeList.Count - 5, "m_OutputTexture", null)),
                ((nodeList.Count - 2, "leftImage", null), (nodeList.Count - 4, "m_OutputTexture", null)),
                ((nodeList.Count - 2, "rightImage", null), (nodeList.Count - 3, "m_OutputTexture", null)),
                ((nodeList.Count - 2, "inputModelUrl", null), (uploadIndex, "outputModelUrl", null)),
                ((nodeList.Count - 1, "m_Go", null), (nodeList.Count - 2, "m_Obj", null)),
            });

            if (needStickyNotes)
            {
                needStickyNotes = false;
                var note = new StickyNote("混元多视图生材质", new Vector2(posx + s_DefaultDist, posy - 100f), 200f, 100f);
                note.content = "输入多视角设计图并上传模型生成材质。";
                notes.Add(note);
            }
        }
    }
}
