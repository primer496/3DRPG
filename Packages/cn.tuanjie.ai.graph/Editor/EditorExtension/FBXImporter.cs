using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AIGraph;

public class FBXImporter : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!IsOurModel()) return;

        var importer = assetImporter as ModelImporter;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
    }

    private static readonly List<string> SupportedPrefixes = new()
    {
        "vast", "hunyuan", "meshy", "rodin"
    };

    private static readonly List<string> SupportedExtensions = new()
    {
        ".fbx", ".glb", ".obj"
    };

    private static readonly List<string> SupportedFolders = new()
    {
        GlobalConstants.AI_GRAPH_FOLDER, GlobalConstants.AI_GRAPH_EXAMPLE_FOLDER
    };

    private bool IsOurModel()
    {
        if (string.IsNullOrEmpty(assetPath)) return false;
        var assetName = Path.GetFileNameWithoutExtension(assetPath);
        if (string.IsNullOrEmpty(assetName)) return false;
        if (SupportedPrefixes.Any(prefix => assetName.StartsWith($"{prefix}_")))
            return true;
        var isSupportedExtension = SupportedExtensions.Contains(Path.GetExtension(assetPath).ToLower());
        return isSupportedExtension && SupportedFolders.Any(assetPath.StartsWith);
    }

    void OnPostprocessModel(GameObject gameObject)
    {
        if (!IsOurModel()) return;

        Vector3 rotationAngle = default;
        var assetName = Path.GetFileNameWithoutExtension(assetPath);
        if (assetName.StartsWith("vast_VastImageToModelNode"))
        {
            rotationAngle = new Vector3(-90f, 0f, 180f);
        }
        else if (assetName.StartsWith("vast_VastTextToModelNode"))
        {
            rotationAngle = new Vector3(-90f, 180f, 0f);
        }
        else if (assetName.StartsWith("vast_VastStylizeModelNode"))
        {
            rotationAngle = new Vector3(0f, 180f, 0f);
        }
        else if (assetName.StartsWith("vast_VastLowpolyNode"))
        {
            rotationAngle = new Vector3(0f, 180f, 0f);
        }
        else if (assetName.StartsWith("vast_VastTextureModelNode"))
        {
            rotationAngle = new Vector3(-90f, 180f, 0f);
        }
        else if (assetName.StartsWith("vast_"))
        {
            rotationAngle = new Vector3(180f, 180f, 0f);
        }
        else if (assetName.StartsWith("hunyuan_HyImageToGeometryNode"))
        {
            rotationAngle = new Vector3(90f, 90f, 0f);
        }
        else if (assetName.StartsWith("hunyuan_HySketch2MeshNode"))
        {
            rotationAngle = new Vector3(90f, 0f, 0f);
        }
        else if (assetName.StartsWith("hunyuan_HyTextToGeometryNode"))
        {
            rotationAngle = new Vector3(90f, 0f, 0f);
        }
        else if (assetName.StartsWith("hunyuan_HyLowpolyNode"))
        {
            rotationAngle = new Vector3(90f, 90f, 0f);
        }
        else if (assetName.StartsWith("hunyuan_HySemanticUVNode"))
        {
            rotationAngle = new Vector3(0f, 90f, 0f);
        }
        else if (assetName.StartsWith("hunyuan_HyImageToTextureNode"))
        {
            rotationAngle = new Vector3(90f, 90f, 0f);
        }
        else if (assetName.StartsWith("rodin_Rodin3DGenerationRegularNode"))
        {
            rotationAngle = new Vector3(-90f, 0f, 0f);
        }
        else if (assetName.StartsWith("rodin_Rodin3DGenerationSmoothNode"))
        {
            rotationAngle = new Vector3(-90f, 0f, 0f);
        }
        else if (assetName.StartsWith("rodin_Rodin3DGenerationSketchNode"))
        {
            rotationAngle = new Vector3(-90f, 0f, 0f);
        }
        else if (assetName.StartsWith("rodin_Rodin3DGenerationDetailNode"))
        {
            rotationAngle = new Vector3(-90f, 0f, 0f);
        }

        List<MeshFilter> meshFilters = new List<MeshFilter>(gameObject.GetComponentsInChildren<MeshFilter>());
        List<SkinnedMeshRenderer> skinnedMeshes = new List<SkinnedMeshRenderer>(gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                ProcessMesh(mf.sharedMesh, rotationAngle);
            }
        }
        foreach (SkinnedMeshRenderer mf in skinnedMeshes)
        {
            if (mf.sharedMesh != null)
            {
                ProcessMesh(mf.sharedMesh, rotationAngle);
            }
        }

        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void ProcessMesh(Mesh mesh, Vector3 rotationAngles)
    {
        Quaternion rotation = Quaternion.Euler(rotationAngles);
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
        }
        mesh.vertices = vertices;

        if (mesh.normals != null && mesh.normals.Length > 0)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = rotation * normals[i];
            }
            mesh.normals = normals;
        }

        if (mesh.tangents != null && mesh.tangents.Length > 0)
        {
            Vector4[] tangents = mesh.tangents;
            for (int i = 0; i < tangents.Length; i++)
            {
                Vector3 tangentDir = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
                tangentDir = rotation * tangentDir;
                tangents[i] = new Vector4(tangentDir.x, tangentDir.y, tangentDir.z, tangents[i].w);
            }
            mesh.tangents = tangents;
        }

        mesh.RecalculateBounds();

        mesh.UploadMeshData(false);
    }
}