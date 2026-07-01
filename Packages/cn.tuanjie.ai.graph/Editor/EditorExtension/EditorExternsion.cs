#if TUANJIE_1_7_OR_NEWER && !TUANJIE_1_7_0
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.AIGraph
{
    [InitializeOnLoad]
    public static class EditorExternsion
    {
        static EditorExternsion()
        {
            TJAiGraphCallBackRegister.OnTuanJieAICallBackForMaterialEditor = LoadMaterialAndEditTemplate;

            TJAiGraphCallBackRegister.OnTuanJieAICallBackForAnimationClip = LoadAnimationCliplAndEditTemplate;

            TJAiGraphCallBackRegister.OnTuanJieAICallBackForObjectSelector = ObjectSelectorCallbacks;

            TJAiGraphCallBackRegister.OnTuanJieAICallBackForTextureImporter = LoadTexture2DTemplate;
        }

        static void LoadAnimationCliplAndEditTemplate(AnimationClip[] clips)
        {
            var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
            templateWindow.targets = new UnityEngine.Object[1];
            for (int i = 0; i < clips.Length; ++i)
            {
                templateWindow.targets[i] = clips[i];
            }
            templateWindow.activeCategories = TemplateWindow.Category.Animation;
        }

        static void LoadTexture2DTemplate(TextureImporter textureImporter)
        {
            var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
            templateWindow.targets = new UnityEngine.Object[1];
            templateWindow.targets[0] = textureImporter;
            templateWindow.activeCategories = TemplateWindow.Category.TwoD;
        }

        static void LoadMaterialAndEditTemplate(Material mat, Renderer[] associateRenderers)
        {
            var templateWindow = EditorWindow.GetWindow<TemplateWindow>();

            if (associateRenderers.Length != 0)
            {
                templateWindow.targets = new UnityEngine.Object[associateRenderers.Length];
                for (int i = 0; i < associateRenderers.Length; ++i)
                {
                    templateWindow.targets[i] = associateRenderers[i];
                }
            }
            else
            {
                if (IsMaterialInAssetDatabaseAndNotBuiltin(mat))
                {
                    templateWindow.targets = new UnityEngine.Object[1];
                    templateWindow.targets[0] = mat;
                }
            }

            templateWindow.activeCategories = TemplateWindow.Category.Material;
        }

        static bool IsMaterialInAssetDatabaseAndNotBuiltin(Material material)
        {
            if (material == null)
                return false;
            string path = AssetDatabase.GetAssetPath(material);

            if (string.IsNullOrEmpty(path))
                return false;

            if (path.Contains("Resources/unity_builtin_extra") || path.Contains("Library/unity default resources"))
                return false;

            return true;
        }

        static List<GameObject> FilterAllSelectedGO(Type filterType)
        {
            var activeGO = Selection.gameObjects;
            List<GameObject> goToBeEdited = new List<GameObject>();
            foreach (var go in activeGO)
            {
                if (go.GetComponent(filterType))
                {
                    goToBeEdited.Add(go);
                }
            }

            return goToBeEdited;
        }

        static void SetTargets(Type targetType, List<GameObject> goList, TemplateWindow window)
        {
            window.targets = new UnityEngine.Object[goList.Count];
            for (int i = 0; i < window.targets.Length; ++i)
            {
                window.targets[i] = goList[i].GetComponent(targetType);
            }
        }

        private static bool checkRequiredComponentOnGO(UnityEngine.Object go, Type requiredType)
        {
            return go != null && go is GameObject && ((go as GameObject).GetComponent(requiredType) != null || (go as GameObject).GetComponentInChildren(requiredType) != null);
        }

        private static void showWindowWithType(Type targetType, TemplateWindow.Category category)
        {
            List<GameObject> goToBeEdited = FilterAllSelectedGO(targetType);
            var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
            SetTargets(targetType, goToBeEdited, templateWindow);
            templateWindow.activeCategories = category;
        }

        static void ObjectSelectorCallbacks(UnityEngine.Object objectBeingEdited, string[] requiredTypes)
        {
            if (objectBeingEdited is MeshRenderer)
            {
                List<GameObject> goToBeEdited = FilterAllSelectedGO(typeof(MeshRenderer));

                var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
                templateWindow.targets = goToBeEdited.ToArray();
                templateWindow.activeCategories = TemplateWindow.Category.ThreeD
                    | TemplateWindow.Category.Material;
            }
            else if (objectBeingEdited is SkinnedMeshRenderer)
                showWindowWithType(typeof(SkinnedMeshRenderer), TemplateWindow.Category.ThreeD | TemplateWindow.Category.Animation
                    | TemplateWindow.Category.Material);
            else if (objectBeingEdited is Animation)
                showWindowWithType(typeof(Animation), TemplateWindow.Category.Animation);
            else if (objectBeingEdited is ParticleSystemRenderer)
                showWindowWithType(typeof(ParticleSystemRenderer), TemplateWindow.Category.ThreeD | TemplateWindow.Category.Material);
            else if (objectBeingEdited is RenderSettings)
            {
                var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
                templateWindow.targets = new UnityEngine.Object[1];
                templateWindow.targets[0] = objectBeingEdited;
                templateWindow.activeCategories = TemplateWindow.Category.Material;
            }
            else if (objectBeingEdited is SpriteRenderer)
                showWindowWithType(typeof(SpriteRenderer), TemplateWindow.Category.TwoD);
            else if (requiredTypes[0] == "Material" 
                && (checkRequiredComponentOnGO(objectBeingEdited, typeof(Skybox)) 
                || Selection.gameObjects.Any(go => checkRequiredComponentOnGO(go, typeof(Skybox)))))
                showWindowWithType(typeof(Skybox), TemplateWindow.Category.Material);
            else if ((objectBeingEdited == null || objectBeingEdited is Mesh)
                && requiredTypes[0] == "Mesh"
                && (checkRequiredComponentOnGO(objectBeingEdited, typeof(MeshFilter))
                || Selection.gameObjects.Any(go => checkRequiredComponentOnGO(go, typeof(MeshFilter)))))
            {
                List<GameObject> goToBeEdited = FilterAllSelectedGO(typeof(MeshFilter));

                var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
                templateWindow.targets = goToBeEdited.ToArray();
                templateWindow.activeCategories = TemplateWindow.Category.ThreeD | TemplateWindow.Category.Animation
                    | TemplateWindow.Category.Material;
            }
            else if (requiredTypes[0] == "Motion")
            {
                var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
                templateWindow.activeCategories = TemplateWindow.Category.Animation;
                templateWindow.targets = new UnityEngine.Object[1];
                templateWindow.targets[0] = Selection.activeObject;
            }
            else
            {
                var templateWindow = EditorWindow.GetWindow<TemplateWindow>();
                templateWindow.targets = new UnityEngine.Object[1];
                templateWindow.targets[0] = objectBeingEdited == null ? Selection.activeObject : objectBeingEdited;
            }
        }
    }
}

#endif
#endif