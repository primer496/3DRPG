using System.IO;
using GraphProcessor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class AssetRenameMonitor : AssetModificationProcessor
{

    /// <summary>
    /// The on will move asset. 当资产被移动时调用，这也包括重命名
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="destinationPath">The destination path.</param>
    /// <returns>The result.</returns>
    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        string sourceDir = Path.GetDirectoryName(sourcePath).Replace("\\", "/");
        if (!AssetUtils.callFromCode && sourceDir.Contains(PathUtils.GRAPH_OUT_PATH))
        {
            AssetUtils.callFromCode = false;
            Debug.LogWarning("Rename of move operation is not allowed for TuanjieAI generated resource, try to rename related exposedParameter.name please");
            return AssetMoveResult.FailedMove;
        }
        AssetUtils.callFromCode = false;
        return AssetMoveResult.DidNotMove;
    }
}
#endif
