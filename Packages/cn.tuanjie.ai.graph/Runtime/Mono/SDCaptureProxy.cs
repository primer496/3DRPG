namespace UnityEngine.AIGraph
{
    public class SDCaptureProxy : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, SDTextureHandle.IconsPath + "Eye.png", true);
        }
    }
}