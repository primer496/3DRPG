using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine.AIGraph.Backend;
using UnityEngine.AIGraph.Cache;

namespace UnityEngine.AIGraph
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SaveAttribute : Attribute
    {
        public Type ReceivedDataType { get; set; } = null;

        public Func<string, Type, object, BaseArtifact> customBaseArtifact { get; set; } = null;
    }

    [Serializable]
    public class TJAIBaseAssetNode : SDNode
    
    {
        // [SerializeField, SerializeReference, HideInInspector]
        [SerializeField, HideInInspector]
        public BaseArtifact currentArtifact;

        [HideInInspector] public bool saveHistory = true;
        [HideInInspector] public bool allowHistory = true;
        internal static ServerConfig serverConfig => ServerConfig.serverConfig;

        private Type m_SourceType;

        private Type m_ReceivedDataType;

        private Type m_DatasetType;

        private FieldInfo m_SaveFieldInfo;

        protected override void Enable()
        {
            base.Enable();
            hasSave = true;

            var fields = GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

            bool hasOneSaveField = false;
            foreach (var field in fields)
            {
                var saveAttr = field.GetCustomAttribute<SaveAttribute>();
                if (saveAttr != null)
                {
                    if (hasOneSaveField)
                        throw new ArgumentException("Only support one save field.");

                    m_SourceType = field.FieldType;

                    if (m_SourceType == null || !typeof(UnityEngine.Object).IsAssignableFrom(m_SourceType))
                        throw new ArgumentException($"Save field {m_SourceType} must be derived from UnityObject!");

                    Type targetInterface = typeof(IReceivedData);
                    m_ReceivedDataType = saveAttr.ReceivedDataType;
                    if (m_ReceivedDataType == null || !targetInterface.IsAssignableFrom(m_ReceivedDataType))
                        throw new ArgumentException($"ReceivedDataType {m_ReceivedDataType} must implement {targetInterface}.");

                    if (saveAttr.customBaseArtifact != null)
                        ArtifactFactory.RegisterArtifact(m_SourceType, m_DatasetType, saveAttr.customBaseArtifact);

                    m_SaveFieldInfo = field;

                    hasOneSaveField = true;
                }
            }

            if (!hasOneSaveField)
            {
                hasSave = false;
                saveHistory = false;
                throw new ArgumentException("Need one save field.");
            }

            currentArtifact = ArtifactFactory.CreateArtifact(m_SourceType, m_ReceivedDataType);
        }

        public virtual void UpdateHistory()
        {
            if (saveHistory)
                graph.history.AddAsset(this, currentArtifact);
        }

        public virtual IEnumerator RestoreHistory(string Guid)
        {
            currentArtifact.Guid = Guid;

            yield return currentArtifact.ReadFromCache(3);

            UpdateOutputPorts();
            InvokeOnProcessed();
        }

        public virtual void Refresh()
        {
            // UpdateOutputPorts();
        }

        /// <summary>
        /// 更新输出端口相关的数据
        /// </summary>
        public virtual void UpdateOutputPorts()
        {
        }

        public virtual void ClearHistory()
        {
            graph.history.ClearAssets(this);
        }

        public virtual void RegisterToHistory()
        {
            graph.history.RegisterNode(this);

            if (currentArtifact.GetCacheUnityObject() != null)
                graph.history.AddAsset(this, currentArtifact);
        }

        public virtual void UnregisterFromHistory()
        {
            graph.history.UnregisterNode(this);
        }

        internal virtual bool GetParam(out List<string> paramList)
        {
            paramList = null;
            return false;
        }

        public void TryExportCurrent()
        {
#if UNITY_EDITOR
            //if (currentArtifact == null) return;
            //ExportUtils.ExportArtifact(currentArtifact);

            if (m_SaveFieldInfo == null) return;
            var source = m_SaveFieldInfo.GetValue(this) as Object;
            var filePath = EditorUtility.SaveFilePanel("Save artifact", Application.dataPath, 
                GUID,"");
            if (string.IsNullOrEmpty(filePath)) return;
            var fileFolder = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
                
            // ExportUtils.SaveAsset(source, Path.GetDirectoryName(filePath),
            //     Path.GetFileNameWithoutExtension(filePath));
            // ExportUtils.SaveAsset(source, GlobalConstants.AI_SAVE_PATH, m_SaveFieldInfo.Name);

            if (m_SourceType == typeof(Texture2D))
            {
                var texture = (Texture2D)source;
                ExportUtils.SaveAsset(texture, fileFolder, fileName);
            }         
            else if (m_SourceType == typeof(GameObject))
            {
                var gameObject = (GameObject)source;
                ExportUtils.SaveAsset(gameObject, fileFolder, fileName);
            } 
            else if (m_SourceType == typeof(Material))
            {
                ExportUtils.SaveAsset((Material)source, fileFolder, fileName);
            }
            else if (m_SourceType == typeof(Video.VideoClip))
            {
                ExportUtils.SaveAsset((Video.VideoClip)source, fileFolder, fileName);
            }
            else if (m_SourceType == typeof(AudioClip))
            {
                ExportUtils.SaveAsset((AudioClip)source, fileFolder, fileName);
            }
            Debug.Log($"Artifact saved to {filePath}.");
#endif
        }

        protected void SetGameObject(ref GameObject go, ref Mesh mesh, ref List<Material> materials)
        {
            go.TryGetComponent<MeshFilter>(out var meshFilter);
            if (mesh != null)
                ReleaseObject(mesh);
            var toCopyMesh = meshFilter?.sharedMesh;
            if (toCopyMesh != null)
            {
                mesh = Object.Instantiate(toCopyMesh);
                mesh.name = toCopyMesh.name;
            }

            foreach (var mat in materials)
                ReleaseObject(mat);
            materials.Clear();
            go.TryGetComponent<Renderer>(out var renderer);
            if (renderer != null)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    var copiedMat = new Material(mat)
                    {
                        name = mat.name
                    };
                    materials.Add(copiedMat);
                }
            }
        }

        protected void RestoreGameObject(out GameObject go, ref Mesh mesh, ref List<Material> materials)
        {
            go = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = mesh.name
            };
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            if (mesh.blendShapeCount > 0 || mesh.boneWeights.Length > 0)
            {
                var skinnedMeshRenderer = go.AddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.sharedMaterials = materials.ToArray();
            }
            else
            {
                var meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = materials.ToArray();
            }
        }

        protected void ReleaseObject(Object toRelease)
        {
            if (toRelease == null) return;
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfPrefabAsset(toRelease)) return;
            Object.DestroyImmediate(toRelease, true);
#else
            Object.Destroy(toRelease);
#endif
            toRelease = null;
        }

        protected void SaveObject(Object toSave)
        {
            if (toSave == null) return;
#if UNITY_EDITOR
            if (AssetDatabase.IsSubAsset(toSave)) return;
            AssetDatabase.AddObjectToAsset(toSave, graph);
#endif
        }

        /// <summary>
        /// record our backend task id
        /// </summary>
        [SerializeField, HideInInspector] protected string taskID;
        /// <summary>
        /// graph有对应的Resources文件夹, assetPath为当前Node在该文件夹下对应资产的路径
        /// </summary>
        [SerializeField, HideInInspector] protected string assetPath;
        ///  任务执行预估时间
        [HideInInspector] public int taskCostTime;
        [HideInInspector] public int taskCostToken = 20;
    }
}
