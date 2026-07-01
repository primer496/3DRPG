using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;

namespace GLTFast.Editor
{
    public class ReflectionDelegate
    {
        public static TDelegate GetStatic<TDelegate>([CanBeNull] MethodInfo methodInfo) where TDelegate : Delegate
        {
            if (methodInfo == null)
                throw new ArgumentException("Method not found.");
            if (!methodInfo.IsStatic)
                throw new AggregateException("Method is not static.");

            return (TDelegate)methodInfo.CreateDelegate(typeof(TDelegate));
        }

        public static TDelegate GetInstance<TDelegate>([CanBeNull] MethodInfo methodInfo) where TDelegate : Delegate
        {
            if (methodInfo == null)
                throw new ArgumentException("Method not found.");
            if (methodInfo.IsStatic)
                throw new AggregateException("Method is static.");

            return (TDelegate)methodInfo.CreateDelegate(typeof(TDelegate), null);
        }
    }

    public static class ScriptedImporterAssetPostProcessing
    {
        public static Event<Action<GameObject>> OnPostprocessModel;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            AssetPostProcessorInfo[] assetPostProcessors = TypeCache.GetTypesDerivedFrom<AssetPostprocessor>()
                                                                    .Select(type => new AssetPostProcessorInfo(type))
                                                                    .ToArray();

            OnPostprocessModel = Event<Action<GameObject>>.Create(nameof(OnPostprocessModel), assetPostProcessors);
        }

        private static Action<AssetPostprocessor, AssetImportContext> AssetPostprocessor_context_set =
            ReflectionDelegate.GetInstance<Action<AssetPostprocessor, AssetImportContext>>(
                typeof(AssetPostprocessor)
                   .GetProperty(nameof(AssetPostprocessor.context))?
                   .GetSetMethod(nonPublic: true)
            );

        public class Event<TDelegate> where TDelegate : Delegate
        {
            private readonly string name;
            private readonly Action<AssetImportContext> configure;
            private readonly TDelegate handler;

            public Event(Action<AssetImportContext> configure, TDelegate handler)
            {
                this.configure = configure;
                this.handler = handler;
            }

            public TDelegate Invoke(AssetImportContext context)
            {
                configure(context);
                return handler;
            }

            public string CustomDependency => $"{nameof(ScriptedImporterAssetPostProcessing)}/{name}";

            public static Event<TDelegate> Create(string name, AssetPostProcessorInfo[] assetPostProcessors)
            {
                AssetPostProcessorInfo[] relevant = assetPostProcessors.Where(x => x.methods.ContainsKey(name))
                                                                       .OrderBy(x => x.order)
                                                                       .ToArray();

                Hash128 hash =
                    Hash128.Compute(relevant.Select(assetPostProcessor => assetPostProcessor.version).ToArray());

                Event<TDelegate> @event = new(
                    context => {
                        foreach (AssetPostProcessorInfo assetPostProcessor in relevant)
                        {
                            assetPostProcessor.instance.assetPath = context.assetPath;
                            // assetPostProcessor.instance.context = context;
                            AssetPostprocessor_context_set(assetPostProcessor.instance, context);
                        }
                    },
                    (TDelegate)relevant
                              .Select(assetPostProcessor =>
                                          assetPostProcessor.methods[name]
                                                            .CreateDelegate(
                                                                 typeof(TDelegate), assetPostProcessor.instance))
                              .Aggregate((Delegate)null, Delegate.Combine)
                );

                AssetDatabase.RegisterCustomDependency(@event.CustomDependency, hash);

                return @event;
            }
        }

        public class AssetPostProcessorInfo
        {
            public AssetPostprocessor instance;
            public uint version;
            public int order;
            public Dictionary<string, MethodInfo> methods;

            public AssetPostProcessorInfo(Type type)
            {
                instance = (AssetPostprocessor)type.GetConstructor(Array.Empty<Type>())!
                                                   .Invoke(Array.Empty<object>());

                version = instance.GetVersion();
                order = instance.GetPostprocessOrder();

                methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              .GroupBy(method => method.Name)
                              .Where(group => group.Count() == 1)
                              .ToDictionary(group => group.Key, group => group.First());
            }
        }
    }
}
