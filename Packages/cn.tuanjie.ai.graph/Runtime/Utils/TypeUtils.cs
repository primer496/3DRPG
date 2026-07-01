using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    public class CustomTypeCoverter : ITypeAdapter
    {
        // NOTE: make sure all conversion is on pair, otherwise may cause exception
        public static string PromptToString(SDPrompt prompt)
        {
            return prompt?.prompt;
        }

        public static SDPrompt StringToPrompt(string prompt)
        {
            return new SDPrompt { prompt = prompt };
        }

        public static List<SDPrompt> StringToPromptList(string prompt)
        {
            return new List<SDPrompt> { StringToPrompt(prompt) };
        }

        public static string PromptListToString(List<SDPrompt> prompts)
        {
            if (prompts == null || prompts.Count != 1)
                throw new System.Exception("Prompt list should only have one element");
            return PromptToString(prompts[0]);
        }

        public static List<SDPrompt> SingleToList(SDPrompt val)
        {
            return new List<SDPrompt> { val };
        }

        public static SDPrompt ListToSingle(List<SDPrompt> val)
        {
            if (val == null || val.Count != 1)
                throw new System.ArgumentException("Prompt list should only have one element");
            return val[0];
        }

        public static List<int> SingleToList(int val)
        {
            return new List<int> { val };
        }

        public static int ListToSingle(List<int> val)
        {
            if (val == null || val.Count != 1)
                throw new System.Exception("Int list should only have one element");
            return val[0];
        }

        public static List<float> SingleToList(float val)
        {
            return new List<float> { val };
        }

        public static float ListToSingle(List<float> val)
        {
            if (val == null || val.Count != 1)
                throw new System.Exception("Float list should only have one element");
            return val[0];
        }

        public static List<string> SingleToList(string val)
        {
            return new List<string> { val };
        }

        public static string ListToSingle(List<string> val)
        {
            if (val == null || val.Count != 1)
                throw new System.Exception("String list should only have one element");
            return val[0];
        }
        
        public static List<Texture2D> SingleToList(Texture2D val)
        {
            return new List<Texture2D> { val };
        }

        public static Texture2D ListToSingle(List<Texture2D> val)
        {
            if (val == null || val.Count != 1)
                throw new System.Exception("Float list should only have one element");
            return val[0];
        }

        public static int FloatToInt(float val)
        {
            return (int)val;
        }

        public static float IntToFloat(int val)
        {
            return (float)val;
        }

        public static List<int> FloatToInt(List<float> val)
        {
            return val.Select(x => (int)x).ToList();
        }

        public static List<float> IntToFloat(List<int> val)
        {
            return val.Select(x => (float)x).ToList();
        }

        public static VastTextToModelOutput HunyuanToVast(HyModelOutput hyUrl)
        {
            var url = string.Empty;
            if (!string.IsNullOrEmpty(hyUrl.fbx_url))
                url = hyUrl.fbx_url;
            else if (!string.IsNullOrEmpty(hyUrl.glb_url))
                url = hyUrl.glb_url;
            else if (!string.IsNullOrEmpty(hyUrl.obj_url))
                url = hyUrl.obj_url;
            return new VastTextToModelOutput()
            {
                model = url, base_model = url, pbr_model = url, rendered_image = hyUrl.image_url
            };
        }

        public static HyModelOutput VastToHunyuan(VastTextToModelOutput vastUrl)
        {
            var url = string.Empty;
            if (!string.IsNullOrEmpty(vastUrl.base_model))
                url = vastUrl.base_model;
            else if (!string.IsNullOrEmpty(vastUrl.pbr_model))
                url = vastUrl.pbr_model;
            else
                url = vastUrl.model;
            var hyUrl = new HyModelOutput();
            if (string.IsNullOrEmpty(url))
                return hyUrl;
            if (url.EndsWith(".glb"))
                hyUrl.glb_url = url;
            else if (url.EndsWith(".fbx"))
                hyUrl.fbx_url = url;
            else if (url.EndsWith(".obj"))
                hyUrl.obj_url = url;
            return hyUrl;
        }
    }
    
    public class ConvertHelper
    {
        public static T ConvertToType<T>(object value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException e)
            {
                // 转换失败的处理
                Debug.LogError($"Conversion to {typeof(T).Name} failed.");
                throw e;
            }
        }


        public static object ConvertToType(object value, Type targetType)
        {
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch (InvalidCastException e)
            {
                // 转换失败的处理
                Debug.LogError($"Conversion to {targetType.Name} failed.");
                throw e;
            }
        }
    }
    
}