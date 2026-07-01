using System.Collections.Generic;

namespace GraphProcessor
{
    public enum LanguageMode
    {
        English,
        Chinese
    }
    public class LocalizationManager
    {
        private static LocalizationManager _instance;
        private LanguageMode currentLanguageMode;
        private static readonly Dictionary<string, (string English, string Chinese)> localizedTexts = 
            new Dictionary<string, (string English, string Chinese)>
            {
                { "Artifact",("Artifact","图像资产")},
                { "AO",("AO","环境遮挡(AO)")},
                { "Asset",("Asset","资产")},
                { "Auto Update",("Auto Update","自动更新")},
                { "Beautify",("Beautify","美化")},
                { "Cancel",("Cancel","取消")},
                { "Center",("Center","中心")},
                { "Continue",("Continue","继续")},
                { "ControlNet",("ControlNet","控制模组")},
                { "ControlNet Hub",("ControlNet Hub","控制模组组合")},
                { "Debug",("Debug","调试")},
                { "Diffuse",("Diffuse","漫反射")},
                { "Doodle",("Doodle","涂鸦")},
                { "Mode",("Mode","模式")},
                { "GameObject",("GameObject","3D对象")},
                { "Generate",("Generate","生成")},
                { "Height",("Height","高度")},
                { "Image",("Image","图片")},
                { "Image Weight",("Image Weight","图片权重")},
                { "Image2Txt",("Image2Txt","图片转文字")},
                { "Img2Txt",("Img2Txt","图片转文字")},
                { "Inpaint",("Inpaint","局部修改")},
                { "Input",("Input","输入")},
                { "Inputs",("Inputs","输入")},
                { "Material",("Material","材质")},
                { "Mask Weight",("Mask Weight","修改权重")},
                { "MetallicGloss",("MetallicGloss","金属光泽")},
                { "Muse Graph",("Muse Graph","Muse图")},
                { "Model",("Model","模型")},
                { "Normal",("Normal","法线")},
                { "Operator",("Operator","操作")},
                { "Output",("Output","输出")},
                { "Pause",("Pause","暂停")},
                { "PBR",("PBR","PBR生成")},
                { "Preprocess",("Preprocess","预处理")},
                { "Preview",("Preview","预览")},
                { "Prompt",("Prompt","提示词")},
                { "Prompt Beautify",("Prompt Beautify","提示词美化")},
                { "Quality",("Quality","质量")},
                { "Reset",("Reset","重置")},
                { "Rodin Generate",("Rodin Generate","Rodin生成")},
                { "Run All",("Run All","执行")},
                { "Run Step",("Run Step","单步执行")},
                { "Save",("Save","存储")},
                { "Save History",("Save History","历史存储")},
                { "Seed",("Seed","种子")},
                { "Show History Assets",("Show History Assets","展示历史资产")},
                { "Show In Project",("Show In Project","文件夹中展示")},
                { "Show Mask",("Show Mask","修改区域展示")},
                { "Show Parameters",("Show Parameters","展示参数")},
                { "Size Mode",("Size Mode","放大比例")},
                { "Style",("Style","风格")},
                { "Style (for Sprite)",("Style (for Sprite)","风格(精灵体)")},
                { "Sprite",("Sprite","精灵体")},
                { "Tier",("Tier","类型")},
                { "Texture",("Texture","纹理")},
                { "TextureNode",("TextureNode","图像节点")},
                { "UpScale",("UpScale","提高分辨率")},
                { "Weight",("Weight","权重")},
                { "Width",("Width","宽度")},
                { "3D Generate",("3D Generate","3D生成")},
                { "TTS",("TextToSpeech","文字转语音")},
                { "Audio Tokenizer",("Audio Tokenizer","文本转音素张量")},
                { "Run Sentis",("Run Sentis","运行Sentis模型") },
                { "Texture Quality", ("Texture Quality", "纹理质量")},
                { "Vast Model Version", ("Model Version", "模型版本")},
                { "Vast Compression Type", ("Compression Type", "压缩类型")},
                { "ImageToModel(Tripo)", ("ImageToModel(Tripo)", "图生3D")},
                { "RetargetModel(Tripo)", ("Retarget Model(Tripo)", "骨骼重定向")}
            };

        private LocalizationManager()
        {

            currentLanguageMode = LanguageMode.English;
        }

        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationManager();
                }
                return _instance;
            }
        }


        public void SetLanguageMode(LanguageMode mode)
        {
            currentLanguageMode = mode;
        }

        public string GetLocalizedText(string key)
        {
            if (localizedTexts.TryGetValue(key, out var texts))
            {
                return currentLanguageMode == LanguageMode.English ? texts.English : texts.Chinese;
            }

            return key;
        }
    }
}
