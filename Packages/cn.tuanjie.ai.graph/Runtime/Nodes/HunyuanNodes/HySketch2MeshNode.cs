using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Sketch to Mesh(Hunyuan)")]
    [UseProcessAsync]
    public class HySketch2MeshNode : BaseHyModelNode
    {
        [Input(name = "Prompt")] 
        [Tooltip("Description of the sketch for 3D mesh generation")]
        public string prompt;
        
        [Input(name = "Sketch")] 
        [Tooltip("Input sketch. Minimum resolution 128x128, size < 10MB, supports PNG/JPG/JPEG/WEBP formats")]
        public Texture2D sketch;
        
        [Input(name = "Enable PBR"), ShowAsDrawer]
        [Tooltip("Generate PBR texture")]
        public bool enablePBR = false;
        
        [Input(name = "Strict Mode"), ShowAsDrawer]
        [Tooltip("Generate face count in a strict mode if true, else may adjust face count according to geometry")]
        public bool strictMode = false;

        [HideInInspector] public int faceCount = 50000;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("Sketch to Mesh(Hunyuan)");
        
        public override string description => DescriptionConstants.HySketch2MeshNode;

        public override IEnumerator ProcessAsync()
        {
            // Part 2: 检查输入是否为空
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt is required", nameof(prompt));
                
            if (sketch == null)
                throw new ArgumentException("Sketch is required", nameof(sketch));
            
            // Part 3：生成对应的request并调用rest call
            var request = new HySketch2MeshRequest()
            {
                prompt = prompt, enablePbr = enablePBR,
                sketchImage = sketch.ToBase64(),
                faceCount = faceCount, strictMode = strictMode
            };
            
            var restCall = new HySketch2MeshRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Face Count: {faceCount}", $"Strict Mode: {strictMode}",
                $"Enable PBR: {enablePBR}"
            };
            return true;
        }

        protected override void Enable()
        {
            base.Enable();
            taskCostTime = 5;
        }
    }
}