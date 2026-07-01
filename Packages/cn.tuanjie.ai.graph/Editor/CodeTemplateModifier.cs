using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 修改创建TJAI相关C#文件时默认的代码模板
/// </summary>
public class CodeTemplateModifier : UnityEditor.AssetModificationProcessor
{
    // 当Unity创建一个新资产时，这个方法会被调用  
    public static void OnWillCreateAsset(string path)
    {
        // 只对C#脚本文件进行处理  
        if (path.EndsWith(".cs.meta"))
        {
            // 获取脚本文件的真实路径  
            string filePath = path.Replace(".meta", "");
            // 只对TJAINodePath下的文件做处理
            string fileDir = Path.GetFileName(Path.GetDirectoryName(path));
            if (fileDir == "TJAINodes")
                CreateTJAINodeScript(filePath);
            else if (fileDir == "TJAIViews")
                CreateTJAIViewScript(filePath);
            else
                return;
            
            // 刷新编辑器以加载新的脚本内容  
            AssetDatabase.Refresh();
        }
    }

    static void CreateTJAINodeScript(string filePath)
    {
        // 如果不是原生模板就不需要覆盖
        try
        {
            string fileContent = File.ReadAllText(filePath);
            if (fileContent.Contains("BaseTJAINode") || fileContent.Contains("UnityChina") || !fileContent.Contains(": MonoBehaviour"))
                return;
        } catch (Exception ex)
        {
            Debug.LogError($"Failed to open file: {filePath}, error msg: {ex}");
        }
        // 定义新的脚本模板内容  
        string customTemplate =
@"/******************************************************************************
* Company:         UnityChina
* Author:          #Name#
* CreateTime:      #CreateTime#
* Version:         0.0.1   
* UnityVersion:    #UnityVersion#
* Description:
******************************************************************************/
using System;
using UnityEngine;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable, NodeMenuItem(""TJAI Graph/#SCRIPTNAME#"")]
    public class #SCRIPTNAME# : BaseTJAINode
    {
        public override string name => LocalizationManager.Instance.GetLocalizedText(""#SCRIPTNAME#"");
        public override bool isRenamable => true;

        /// <summary>
        /// Called when the node is enabled
        /// </summary>
        protected override void Enable()
        {
        }

        /// <summary>
        /// Called when the node is disabled
        /// </summary>
        protected override void Disable()
        {
        }


        /// <summary>
        /// Override this method to implement custom processing
        /// </summary>
        public override void Process()
        {
        }
    }
}";
        // 将默认脚本内容替换为自定义模板  
        customTemplate = customTemplate.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(filePath));
        customTemplate = customTemplate.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        customTemplate = customTemplate.Replace("#UnityVersion#", Application.unityVersion);
        customTemplate = customTemplate.Replace("#Name#", Environment.UserName);
        // 将替换后的内容写回脚本文件  
        File.WriteAllText(filePath, customTemplate);
    }

    static void CreateTJAIViewScript(string filePath)
    {        
        // 如果不是原生模板就不需要覆盖
        try
        {
            string fileContent = File.ReadAllText(filePath);
            if (fileContent.Contains("BaseTJAINode") || fileContent.Contains("UnityChina") || !fileContent.Contains(": MonoBehaviour"))
                return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open file: {filePath}, error msg: {ex}");
        }
        // 定义新的脚本模板内容  
        string customTemplate =
@"/******************************************************************************
* Company:         UnityChina
* Author:          #Name#
* CreateTime:      #CreateTime#
* Version:         0.0.1   
* UnityVersion:    #UnityVersion#
* Description:
******************************************************************************/
using System;
using UnityEngine;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEditor.AIGraph;

/// <summary>
/// 
/// </summary>
[NodeCustomEditor(typeof(#NODE#))]
public class #SCRIPTNAME# : BaseTJAINodeView
{
    private new #NODE# nodeTarget => base.nodeTarget as #NODE#;

    public override void Enable()
    {
    }
}";
        // 将默认脚本内容替换为自定义模板
        string nodeName = Path.GetFileNameWithoutExtension(filePath);
        customTemplate = customTemplate.Replace("#SCRIPTNAME#", nodeName);
        customTemplate = customTemplate.Replace("#NODE#", nodeName.Replace("View", ""));
        customTemplate = customTemplate.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        customTemplate = customTemplate.Replace("#UnityVersion#", Application.unityVersion);
        customTemplate = customTemplate.Replace("#Name#", Environment.UserName);
        // 将替换后的内容写回脚本文件  
        File.WriteAllText(filePath, customTemplate);
    }
}
