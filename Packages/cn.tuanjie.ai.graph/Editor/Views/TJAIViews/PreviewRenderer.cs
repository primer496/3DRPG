using System;
using System.Collections.Generic;
using UnityEditor.AIGraph.InternalBridge;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.Experimental.Video;
using UnityEngine.Video;

namespace UnityEditor.AIGraph
{
    public interface IPreviewRenderer
    {
        public bool HasPreviewGUI();
        public string GetPreviewTitle();
        public void OnPreviewSettings();
        public void OnPreviewGUI(Rect r, GUIStyle background);
        void Initialize(UnityEngine.Object target, SDNode node);
        void Cleanup();
        void Update(UnityEngine.Object target);
    }

    public static class PreviewRendererRegistry
    {
        private static readonly Dictionary<Type, Func<IPreviewRenderer>> rendererFactories =
            new Dictionary<Type, Func<IPreviewRenderer>>();

        static PreviewRendererRegistry()
        {
            Register<Mesh>(() => new MeshPreviewRenderer());
            Register<Material>(() => new MaterialPreviewRenderer());
            Register<Texture>(() => new Texture2DPreviewRenderer());
            Register<Texture2D>(() => new Texture2DPreviewRenderer());
            Register<AnimationClip>(() => new AnimationClipPreviewRenderer());
            Register<GameObject>(() => new GameObjectPreviewRenderer());
            Register<VideoClip>(() => new VideoClipPreviewRenderer());
            Register<AudioClip>(() => new AudioClipPreviewRenderer());
        }

        public static void Register<T>(Func<IPreviewRenderer> factory)
        {
            rendererFactories[typeof(T)] = factory;
        }

        public static IPreviewRenderer GetRenderer(Type type)
        {
            if (rendererFactories.TryGetValue(type, out var factory))
            {
                return factory();
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (rendererFactories.TryGetValue(interfaceType, out factory))
                {
                    return factory();
                }
            }

            Type baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (rendererFactories.TryGetValue(baseType, out factory))
                {
                    return factory();
                }
                baseType = baseType.BaseType;
            }

            throw new NotImplementedException();
        }
    }

    public abstract class BasePreviewRenderer<T> : IPreviewRenderer where T : class
    {
        protected T target;

        public virtual void Initialize(UnityEngine.Object target, SDNode node)
        {
            this.target = target as T;
        }
        public virtual void Cleanup() { }

        public virtual void Update(UnityEngine.Object target)
        {
            this.target = target as T;
        }

        public abstract bool HasPreviewGUI();
        public abstract string GetPreviewTitle();
        public abstract void OnPreviewSettings();
        public abstract void OnPreviewGUI(Rect r, GUIStyle background);

        protected static GUIContent GetPreviewTitleStatic(UnityEngine.Object target)
        {
            if (target == null)
                return new GUIContent("Null");

            GUIContent guiContent = new GUIContent();

            if (InternalAPI.Internal_NativeClassExtensionUtilities_ExtendsANativeType(target))
                guiContent.text += InternalAPI.Internal_MonoScript_FromScriptedObject(target).GetClass().Name;
            else
                guiContent.text += ObjectNames.NicifyVariableName(ObjectNames.GetClassName(target));

            guiContent.text += "s";

            return guiContent;
        }
    }

    public class DefaultPreviewRenderer : IPreviewRenderer
    {
        private UnityEngine.Object target;

        public void Initialize(UnityEngine.Object target, SDNode node) => this.target = target;
        public void Cleanup() { }
        public void Update(UnityEngine.Object target) => this.target = target;
        public bool HasPreviewGUI() { return true; }
        public string GetPreviewTitle() { return ""; }

        public void OnPreviewSettings() { }

        public void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            string text = target?.ToString() ?? "Null";
            GUI.Label(rect, text, background);
        }
    }
}

