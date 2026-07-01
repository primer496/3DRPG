namespace UnityEngine.AIGraph {

    static class DescriptionConstants
    {
        public const string DoodleNode = "The node is designed to draw the doodle, which can be used into the generation controller. Drawing the doodle by double click the image.";
        public const string UpscalerNode = "The node is designed to upscale the image resolution by a factor of 2 or 4 times the original image.";
        public const string StyleNode = "The node is designed to provide style lora for the sprite generation";
        public const string SentisNode = "The node is designed to ";
        public const string SentisLayerNode = "The node is designed to add sentis layer before or after existing models, which can be used into downstreaming tasks";
        public const string RelayNode = "";
        public const string PromptNode = "The node is designed to provide positive prompt for the following generation";
        public const string PbrNode = "The node is designed to generate PBR maps according to the input texture. The node will generate diffuse map, metallic map, roughness map, normal map and AO map. The node will also provide the material based on the standard lit shader and PBR maps.";
        public const string InpaintNode = "The node is to make partial modifications with prompt based on AI model. Select the region to be modified by double clicking the image and painting with the brush.";
        public const string GenerateNode = "The node is designed to generate sprite/texture image with prompt. It takes 30s for one image and longer if more prompts are given.";
        public const string ControlNetNode = "The node is designed to provide controlnet function for sprite generation. It contains multiple controlnet functions, which all need images.";
        public const string ControlNetHubNode = "The node is designed to combine several controlnet functions and send them to the generation node. The max controlnet number to support is 3";
        public const string CaptionNode = "The node is designed to extract keywords and caption from the input image.";
        public const string BeautifyNode = "The node is designed to beautify the sprite/texture prompt with more details and information";
        public const string AudioDecoderNode = "The node is designed to decode the audio with sentis model";
        public const string Generate3dNode = "The node is designed to generate 3d model and its material with rodin (3rd party)";
        public const string VastTextToModelNode = "The node is designed to generate 3d model and its material with vast (3rd party)\n输入: 用户提供描述模型外观、风格、物体类型的文本提示。\n输出: 一个完整的3D网格模型以及附带的材质/纹理。";
        public const string VastImageToModelNode = "The node is designed to generate 3d model and its material with image (3rd party)\n输入: 一张或多张从不同角度拍摄的物体图片。\n输出: 一个根据输入图像重建的3D网格模型及其材质。";
        public const string VastTextureModelNode = "The node is designed to generate 3d model's material with texture (3rd party)\n输入: 一个现有的3D模型和描述所需材质外观的文本提示或基础纹理。\n输出: 应用于该模型的新材质/纹理贴图。";
        public const string VastRigNode = "The node is designed to rigging (3rd party)\n输入: 一个静态的、无骨骼的3D模型（通常是角色）。\n输出: 一个带有骨骼层次结构、权重和控制器的已绑定模型。";
        public const string VastRetargetNode = "The node is designed to retarget 3d model (3rd party)\n输入: 一个带有动画的源模型和一个需要应用该动画的目标模型。\n输出: 目标模型适配了源模型的动画数据。";
        public const string VastStylizeModelNode = "The node is designed to stylize 3d model (3rd party)\n输入: 一个3D模型和描述目标风格的文本提示（如“乐高”）。\n输出: 一个经过风格化处理的新3D模型或经过修改的材质。";
        public const string VastMultiviewToModelNode = "The node is designed to generate 3d model from multiview (3rd party)\n输入: 围绕同一物体拍摄的不同角度的图像。\n输出: 一个高精度3D模型。";
        public const string VastMeshSegmentationNode = "The node will separate the whole model in parts based on the type and shape of it (3rd party)\n根据模型的类型和形状（使用Vast第三方服务）将整个模型分割为多个部分。";
        public const string VastMeshCompletionNode = "Complete selected part\n补全指定部分的网格。";
        public const string VastLowpolyNode = "Generate lowpoly model\n生成低多边形模型，一个保留原始模型基本形状但面数减少的简化版本。";
        
        // public const string HyImageToTextureNode = "对3d模型进行纹理的生成";
        // public const string HyImageSubjectSegmentationNode = "去除输入图像的背景";
        // public const string HyTextToImageNode = "";
        // public const string HyViewsToGeometryNode = "多视图生成几何模型";
        // public const string HySemanticUVNode = "自动化UV展开算法, 输出带UV展开的3D模型";
        // public const string HyMotionRetargetNode = "对生成的mesh模型实现预定义的动作驱动";
        // public const string HyLowpolyNode = "基于输入的3d模型，通过大模型算法进行减面，生成低面片且布线更规整的3D模型";
        // public const string HyPoseStandardizationNode =
        //     "对输入的全身人像转换为标准t-pose姿态（建议输入真人正面站立图像，四肢未遮挡，无附加物品（如手提包、武器装备、篮球、足球），头发不过肩，衣服合身，无宽松长袖、长裙、古装等）";
        // public const string HyImageToGeometryNode = "单图生成几何模型";
        // public const string HyAutoRiggingNode = "自动绑骨蒙皮算法，输入3D模型，利用算法对模型进行自动化骨骼蒙皮预测，并输出带骨骼蒙皮的3D模型。";
        // public const string HyAnimationNode = "动画生成：对生成3D角色模型实现自动绑骨蒙皮，选择不同动作模版生成3D动画";
        // public const string HyUploadModelNode = "上传模型";
        // public const string HyImageControlnetGrayScaleNode = "游戏场景下的文生图功能，支持风格参考图和控制图";
        // public const string HyImageFlexibilityConsistencynNode = "根据参考图像生成角色一致的图像，同时支持文本引导的非角色区域编辑。适用于人物、角色等和其它物体有交互、变形幅度大的物体";
        // public const string HyImageStyleSwitchNode = "实现图片风格的转换";
        // public const string HyImageThreeViewNode = "上传角色全身设计，生成A/T pose三视图";
        // public const string HyImageGeneratingNode = "采用模型矩阵能力支持文生图任务，后端由人像、游戏、通用模型，以及各类功能插件组成，灵活适配多种文生图任务";
        
        // ------------------- Hunyuan Node -------------------
        public const string HyImageToTextureNode = "Generate textures for 3D models\n对3d模型进行纹理的生成";
        public const string HyImageSubjectSegmentationNode = "Remove the background from the input image\n去除输入图像的背景";
        public const string HyTextToImageNode = "";
        public const string HyViewsToGeometryNode = "Generate geometry from multiple views\n多视图生成几何模型";
        public const string HySemanticUVNode = "Automated UV unwrapping algorithm, output 3D model with UV unwrapping\n自动化UV展开算法, 输出带UV展开的3D模型";
        public const string HyMotionRetargetNode = "Drive the generated mesh model with predefined animations\n对生成的mesh模型实现预定义的动作驱动";
        public const string HyLowpolyNode = "Reduce the polygon count of the input 3D model using a large model algorithm to generate a low-poly 3D model with more regular topology\n基于输入的3d模型，通过大模型算法进行减面，生成低面片且布线更规整的3D模型";
        public const string HyPoseStandardizationNode =
            "Convert the input full-body portrait to a standard T-pose (it is recommended to input a front-facing standing image of a person with limbs not obstructed, no additional items (such as handbags, weapons, basketballs, footballs), hair not over the shoulders, fitted clothing, and no loose long sleeves, skirts, or traditional costumes)\n对输入的全身人像转换为标准t-pose姿态（建议输入真人正面站立图像，四肢未遮挡，无附加物品（如手提包、武器装备、篮球、足球），头发不过肩，衣服合身，无宽松长袖、长裙、古装等）";
        public const string HyImageToGeometryNode = "Generate geometry from a single image\n单图生成几何模型";
        public const string HyTextToGeometryNode = "Generate geometry from prompt\n文生成几何模型";
        public const string HyAutoRiggingNode = "Automated rigging and skinning algorithm, input a 3D model, use the algorithm to predict automated bone skinning for the model, and output a 3D model with bone skinning\n自动绑骨蒙皮算法，输入3D模型，利用算法对模型进行自动化骨骼蒙皮预测，并输出带骨骼蒙皮的3D模型。";
        public const string HyAnimationNode = "Animation generation: Automatically rig and skin the generated 3D character model, and generate 3D animations by selecting different animation templates\n动画生成：对生成3D角色模型实现自动绑骨蒙皮，选择不同动作模版生成3D动画";
        public const string HyUploadModelNode = "Upload model\n上传模型";
        public const string HyImageControlnetGrayScaleNode = "Text-to-image generation in game scenes, supporting style reference images and control maps\n游戏场景下的文生图功能，支持风格参考图和控制图";
        public const string HyImageFlexibilityConsistencynNode = "Generates character-consistent images from reference images while supporting text-guided editing of non-character areas. Suitable for characters, avatars, and other objects with large deformations or interactions\n根据参考图像生成角色一致的图像，同时支持文本引导的非角色区域编辑。适用于人物、角色等和其它物体有交互、变形幅度大的物体";
        public const string HyImageStyleSwitchNode = "Performs image style conversion\n实现图片风格的转换";
        public const string HyImageThreeViewNode = "Upload full-body character designs to generate A/T pose three-view diagrams\n上传角色全身设计，生成A/T pose三视图";
        public const string HyImageGeneratingNode = "Utilizes model matrix capabilities to support text-to-image tasks, with backend composed of portrait, game, general-purpose models, and various functional plugins for flexible adaptation to diverse text-to-image tasks\n采用模型矩阵能力支持文生图任务，后端由人像、游戏、通用模型，以及各类功能插件组成，灵活适配多种文生图任务";
        public const string HyTextToPanoramaNode = "Input text to panorama image";
        public const string HyFormatConversionNode = "Convert 3D model format\n模型格式转换";
        public const string HyImageClarityNode = "image super resolution\n输入图像，输出变清晰后的图片。使其在视觉上更加清晰、细节更加丰富。";
        public const string HySketch2MeshNode =
            "Sketch Generation: Input a sketch and text description to quickly convert a 2D sketch into a high-quality 3D model.\n输入草图与文字描述，将二维草图快速转换为高质量的3D。";
        public const string HyBackgroundReplacementNode = "replace image background\n输入通用图像，图像中一个物体的mask以及描述物体之外背景部分的描述，生成对应的主体和替换后的背景。";
    }

}