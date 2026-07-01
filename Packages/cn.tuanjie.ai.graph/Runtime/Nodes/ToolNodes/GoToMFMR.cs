using System;
using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/Mesh And Material Getter"), UseProcessAsync]
    public class MeshAndMaterialNode : SDNode
    {
        [Input(name = "GameObject"), SerializeField]
        private GameObject m_Go;

        [Output(name = "Mesh"), SerializeField, HideInInspector]
        private Mesh m_Mesh;

        [Output(name = "Materials"), SerializeField, HideInInspector]
        private Material[] m_Materials;

        public override bool isRenamable => false;
        public override bool needTrigger => true;

        public override IEnumerator ProcessAsync()
        {
            if (m_Go == null)
                throw new NullReferenceException("Input GameObject is null");

            // try to get from skinned mesh renderer
            var skinnedMeshRenderer = m_Go.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                m_Mesh = skinnedMeshRenderer.sharedMesh;
                m_Materials = skinnedMeshRenderer.sharedMaterials;
                yield break;
            }

            // try to get from mesh filter and mesh renderer
            var meshFilter = m_Go.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                m_Mesh = meshFilter.sharedMesh;
                var meshRenderer = m_Go.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    m_Materials = meshRenderer.sharedMaterials;
                yield break;
            }

            // try to get from child
            var skinnedMeshRenderers = m_Go.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers is { Length: > 0 })
            {
                m_Mesh = skinnedMeshRenderers[0].sharedMesh;
                m_Materials = skinnedMeshRenderers[0].sharedMaterials;
                if (skinnedMeshRenderers.Length > 1)
                    Debug.LogWarning($"More than one mesh filter attached in {m_Go.name}, use first mesh");
                yield break;
            }
            var meshFilters = m_Go.GetComponentsInChildren<MeshFilter>();
            if (meshFilters is { Length: > 0 })
            {
                m_Mesh = meshFilters[0].sharedMesh;
                var meshRenderer = meshFilters[0].GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    m_Materials = meshRenderer.sharedMaterials;
                if (meshFilters.Length > 1)
                    Debug.LogWarning($"More than one mesh filter attached in {m_Go.name}, use first mesh");
            }
            else
                throw new NullReferenceException($"No mesh filter attached in {m_Go.name}");

            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}