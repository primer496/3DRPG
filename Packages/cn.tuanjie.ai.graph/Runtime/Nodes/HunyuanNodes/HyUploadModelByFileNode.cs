using System;
using System.Collections;
using System.IO;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    // [System.Serializable, NodeMenuItem("Hunyuan/Upload Model By File")]
    [UseProcessAsync]
    public class HyUploadModelByFileNode : SDNode
    {
        [Output(name = "Model Url")] public HyModelOutput ModelUrl;
        [HideInInspector] public string modelPath;
        protected bool uploaded = false;
        public override string description => DescriptionConstants.HyUploadModelNode;
        public override bool needTrigger => true;
        public override bool isRenamable => true;
        public override string name => LocalizationManager.Instance.GetLocalizedText("UploadModelByFile");
        
        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(modelPath))
                throw new NullReferenceException("Empty path is invalid");
            if (!File.Exists(modelPath))
                throw new NullReferenceException($"File {modelPath} does not exist");
            if (uploaded)
                yield break;
            var bytes = File.ReadAllBytes(modelPath);
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}

