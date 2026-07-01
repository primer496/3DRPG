using UnityEditor;

namespace UnityEngine.AIGraph
{
    public static class ObjectCopier
    {
        public static T CopyObject<T>(T original) where T : Object
        {
            if (original == null) return null;
            return Object.Instantiate(original);
        }

#if UNITY_EDITOR
        public static GameObject CopyObject(GameObject originalPrefab)
        {
            return PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
        }

        public static AnimationClip CopyObject(AnimationClip originalClip)
        {
            AnimationClip clip = new AnimationClip();
            EditorUtility.CopySerialized(originalClip, clip);
            return clip;
        }
#endif
    }
}