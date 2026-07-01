using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	[Serializable]
	public class ExposedParameter : ISerializationCallbackReceiver
	{
        [Serializable]
        public class Settings
        {
            public bool isHidden = false;
            public bool expanded = false;
            public ParameterAccessor accessor;

            [SerializeField]
            internal string guid = null;

            public override bool Equals(object obj)
            {
                if (obj is Settings s && s != null)
                    return Equals(s);
                else
                    return false;
            }

            public virtual bool Equals(Settings param)
                => isHidden == param.isHidden && expanded == param.expanded;

            public override int GetHashCode() => base.GetHashCode();
        }

		public string				guid; // unique id to keep track of the parameter
        [SerializeField]
        private string _name; // 私有字段，使用 [SerializeField] 标记

        public virtual string name
        {
            get { return _name; }
            set { _name = value; }
        }
        
        [Obsolete("Use GetValueType()")]
		public string				type;
		[Obsolete("Use value instead")]
		public SerializableObject	serializedValue;
        [SerializeReference]
		public Settings             settings;
		public string shortType => GetValueType()?.Name;
        public string assetPath;
        public virtual string assetExtension => "asset";

        public virtual void Initialize(string name, object value)
        {
			guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter
            settings = CreateSettings();
            settings.guid = guid;
			this.name = name;
			this.value = value;
            //InitAsset($"{name}-{guid[..8]}.{assetExtension}");
        }

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// SerializeReference migration step:
#pragma warning disable CS0618
			if (serializedValue?.value != null) // old serialization system can't serialize null values
			{
				value = serializedValue.value;
				Debug.Log("Migrated: " + serializedValue.value + " | " + serializedValue.serializedName);
				serializedValue.value = null;
			}
#pragma warning restore CS0618
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {}

        protected virtual Settings CreateSettings() => new Settings();

        public virtual object value { get; set; }
        public virtual Type GetValueType() => value.GetType();

        static Dictionary<Type, Type> exposedParameterTypeCache = new Dictionary<Type, Type>();
        internal ExposedParameter Migrate()
        {
            if (exposedParameterTypeCache.Count == 0)
            {
                foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
                {
                    if (type.IsSubclassOf(typeof(ExposedParameter)) && !type.IsAbstract)
                    {
                        var paramType = Activator.CreateInstance(type) as ExposedParameter;
                        exposedParameterTypeCache[paramType.GetValueType()] = type;
                    }
                }
            }
#pragma warning disable CS0618 // Use of obsolete fields
            var oldType = Type.GetType(type);
#pragma warning restore CS0618
            if (oldType == null || !exposedParameterTypeCache.TryGetValue(oldType, out var newParamType))
                return null;
            
            var newParam = Activator.CreateInstance(newParamType) as ExposedParameter;

            newParam.guid = guid;
            newParam.name = name;
            newParam.settings = newParam.CreateSettings();
            newParam.settings.guid = guid;
            newParam.assetPath = assetPath;

            return newParam;
     
        }

        public static bool operator ==(ExposedParameter param1, ExposedParameter param2)
        {
            if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))
                return true;
            if (ReferenceEquals(param1, param2))
                return true;
            if (ReferenceEquals(param1, null))
                return false;
            if (ReferenceEquals(param2, null))
                return false;

            return param1.Equals(param2);
        }

        public static bool operator !=(ExposedParameter param1, ExposedParameter param2) => !(param1 == param2);

        public bool Equals(ExposedParameter parameter) => guid == parameter.guid;

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            else
                return Equals((ExposedParameter)obj);
        }

        public override int GetHashCode() => guid.GetHashCode();

        public virtual ExposedParameter Clone()
        {
            var clonedParam = Activator.CreateInstance(GetType()) as ExposedParameter;

            clonedParam.guid = guid;
            clonedParam.name = name;
            clonedParam.settings = settings;
            clonedParam.value = value;

            // init asset
            string assetName = $"{name}-cloned-{Guid.NewGuid().ToString()}.{assetExtension}";
            clonedParam.InitAsset(assetName);

            return clonedParam;
        }

        public virtual void InitAsset(string assetName) { }
        public virtual void DeleteAsset() 
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetPath))
                return;
            // Debug.Log("Try to releasing asset resource");
            bool succeed = AssetDatabase.DeleteAsset(assetPath);
            if (!succeed)
                Debug.Log($"failed to delete asset: {assetPath}");
#endif
        }

        /// <summary>
        /// rename asset when parameter.name is updated
        /// </summary>
        /// <param name="newName"></param>
        protected virtual void Rename(string newName)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(assetPath))
            {
                string newAssetName = Path.GetFileName(assetPath);
                newAssetName = newName + newAssetName[_name.Length..];
                string newPath = Path.GetDirectoryName(assetPath) + "/" + newAssetName;
                newPath = newPath.Replace("\\", "/");
                AssetUtils.callFromCode = true;
                string msg = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (!string.IsNullOrEmpty(msg))
                    Debug.Log($"Failed to rename asset: {msg}");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                assetPath = newPath;
            }
#endif
        }
    }

    // Due to polymorphic constraints with [SerializeReference] we need to explicitly create a class for
    // every parameter type available in the graph (i.e. templating doesn't work)
    [System.Serializable]
    public class ColorParameter : ExposedParameter
    {
        public enum ColorMode
        {
            Default,
            HDR
        }

        [Serializable]
        public class ColorSettings : Settings
        {
            public ColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((ColorSettings)param).mode;
        }

        [SerializeField] Color val;

        public override object value { get => val; set => val = (Color)value; }
        protected override Settings CreateSettings() => new ColorSettings();
    }

    [System.Serializable]
    public class FloatParameter : ExposedParameter
    {
        public enum FloatMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class FloatSettings : Settings
        {
            public FloatMode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((FloatSettings)param).mode && min == ((FloatSettings)param).min && max == ((FloatSettings)param).max;
        }

        [SerializeField] float val;

        public override object value { get => val; set => val = (float)value; }
        protected override Settings CreateSettings() => new FloatSettings();
    }

    [System.Serializable]
    public class Vector2Parameter : ExposedParameter
    {
        public enum Vector2Mode
        {
            Default,
            MinMaxSlider,
        }

        [Serializable]
        public class Vector2Settings : Settings
        {
            public Vector2Mode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((Vector2Settings)param).mode && min == ((Vector2Settings)param).min && max == ((Vector2Settings)param).max;
        }

        [SerializeField] Vector2 val;

        public override object value { get => val; set => val = (Vector2)value; }
        protected override Settings CreateSettings() => new Vector2Settings();
    }

    [System.Serializable]
    public class Vector3Parameter : ExposedParameter
    {
        [SerializeField] Vector3 val;

        public override object value { get => val; set => val = (Vector3)value; }
    }

    [System.Serializable]
    public class Vector4Parameter : ExposedParameter
    {
        [SerializeField] Vector4 val;

        public override object value { get => val; set => val = (Vector4)value; }
    }

    [System.Serializable]
    public class IntParameter : ExposedParameter
    {
        public enum IntMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class IntSettings : Settings
        {
            public IntMode mode;
            public int min = 0;
            public int max = 10;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((IntSettings)param).mode && min == ((IntSettings)param).min && max == ((IntSettings)param).max;
        }

        [SerializeField] int val;

        public override object value { get => val; set => val = (int)value; }
        protected override Settings CreateSettings() => new IntSettings();
    }

    [System.Serializable]
    public class Vector2IntParameter : ExposedParameter
    {
        [SerializeField] Vector2Int val;

        public override object value { get => val; set => val = (Vector2Int)value; }
    }

    [System.Serializable]
    public class Vector3IntParameter : ExposedParameter
    {
        [SerializeField] Vector3Int val;

        public override object value { get => val; set => val = (Vector3Int)value; }
    }

    [System.Serializable]
    public class DoubleParameter : ExposedParameter
    {
        [SerializeField] Double val;

        public override object value { get => val; set => val = (Double)value; }
    }

    [System.Serializable]
    public class LongParameter : ExposedParameter
    {
        [SerializeField] long val;

        public override object value { get => val; set => val = (long)value; }
    }

    [System.Serializable]
    public class StringParameter : ExposedParameter
    {
        [SerializeField] string val;

        public override object value { get => val; set => val = (string)value; }
        public override Type GetValueType() => typeof(String);
    }

    [System.Serializable]
    public class RectParameter : ExposedParameter
    {
        [SerializeField] Rect val;

        public override object value { get => val; set => val = (Rect)value; }
    }

    [System.Serializable]
    public class RectIntParameter : ExposedParameter
    {
        [SerializeField] RectInt val;

        public override object value { get => val; set => val = (RectInt)value; }
    }

    [System.Serializable]
    public class BoundsParameter : ExposedParameter
    {
        [SerializeField] Bounds val;

        public override object value { get => val; set => val = (Bounds)value; }
    }

    [System.Serializable]
    public class BoundsIntParameter : ExposedParameter
    {
        [SerializeField] BoundsInt val;

        public override object value { get => val; set => val = (BoundsInt)value; }
    }

    [System.Serializable]
    public class AnimationCurveParameter : ExposedParameter
    {
        [SerializeField] AnimationCurve val;

        public override object value { get => val; set => val = (AnimationCurve)value; }
        public override Type GetValueType() => typeof(AnimationCurve);
    }

    [System.Serializable]
    public class GradientParameter : ExposedParameter
    {
        public enum GradientColorMode
        {
            Default,
            HDR,
        }

        [Serializable]
        public class GradientSettings : Settings
        {
            public GradientColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((GradientSettings)param).mode;
        }

        [SerializeField] Gradient val;
        [SerializeField, GradientUsage(true)] Gradient hdrVal;

        public override object value { get => val; set => val = (Gradient)value; }
        public override Type GetValueType() => typeof(Gradient);
        protected override Settings CreateSettings() => new GradientSettings();
    }

    [System.Serializable]
    public class GameObjectParameter : ExposedParameter
    {
        [SerializeField] GameObject val;

        public override object value { get => val; set => val = (GameObject)value; }
        public override Type GetValueType() => typeof(GameObject);
    }

    [System.Serializable]
    public class BoolParameter : ExposedParameter
    {
        [SerializeField] bool val;

        public override object value { get => val; set => val = (bool)value; }
    }

    [System.Serializable]
    public class Texture2DParameter : ExposedParameter
    {
        [SerializeField] Texture2D val;
        public override string name
        {
            get => base.name;
            set
            {
                if (base.name == value)
                    return;
                Rename(value);
                base.name = value;
            }
        }

        public override object value { get => val;
            set
            {
                val = (Texture2D)value;
                if (!string.IsNullOrEmpty(assetPath) && val != null)
                    val.name = Path.GetFileNameWithoutExtension(assetPath);
            }
        }
        public override Type GetValueType() => typeof(Texture2D);
        public override string assetExtension => "png";

        public override void InitAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogWarning($"empty assetName is not allowed.");
                return;
            }
            val = new Texture2D(256, 256);
            assetPath = Path.Join(PathUtils.GRAPH_OUT_PATH, assetName);
            assetPath = assetPath.Replace("\\", "/");
            PathUtils.CreateDirectory(Path.GetDirectoryName(assetPath));
            byte[] bytes = val.EncodeToPNG();
            File.WriteAllBytes(assetPath, bytes);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(assetPath);
#endif
            string resourcePath = assetPath.Replace("Assets/Resources/", "");
            resourcePath = Path.GetDirectoryName(resourcePath) + "/" + Path.GetFileNameWithoutExtension(resourcePath);
            val = Resources.Load<Texture2D>(resourcePath);
        }
    }

    [System.Serializable]
    public class RenderTextureParameter : ExposedParameter
    {
        [SerializeField] RenderTexture val;
        public override string name
        {
            get => base.name;
            set
            {
                if (base.name == value)
                    return;
                Rename(value);
                base.name = value;
            }
        }
        public override object value
        {
            get => val;
            set
            {
                // TODO: do we need to recover the original asset?
                val = (RenderTexture)value;
                if (!string.IsNullOrEmpty(assetPath) && val != null)
                    val.name = Path.GetFileNameWithoutExtension(assetPath);
            }
        }
        public override Type GetValueType() => typeof(RenderTexture);
        public override void InitAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogWarning($"empty assetName is not allowed.");
                return;
            }
            val = new RenderTexture(256, 256, 24);
            assetPath = Path.Join(PathUtils.GRAPH_OUT_PATH, assetName);
            assetPath = assetPath.Replace("\\", "/");
            PathUtils.CreateDirectory(Path.GetDirectoryName(assetPath));
#if UNITY_EDITOR
            // TODO: use addressable
            if (AssetDatabase.LoadAssetAtPath<RenderTexture>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(val, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
#endif
            val.Release();
        }
    }

    [System.Serializable]
    public class MeshParameter : ExposedParameter
    {
        [SerializeField] Mesh val;
        public override string name
        {
            get => base.name;
            set
            {
                if (base.name == value)
                    return;
                Rename(value);
                base.name = value;
            }
        }
        public override object value { get => val;
            set
            {
                Mesh mesh = value as Mesh;
                if (val == null)
                {
                    val = mesh;
                    return;
                }
                if (mesh == null)
                    return;
                // copy mesh parameter
                val.vertices = mesh.vertices;
                val.uv = mesh.uv;
                val.normals = mesh.normals;
                // TOCHECK: what if quad mesh?
                val.triangles = mesh.triangles;
                val.uv2 = mesh.uv2;
                val.uv3 = mesh.uv3;
                val.uv4 = mesh.uv4;
                val.uv5 = mesh.uv5;
                val.uv6 = mesh.uv6;
                val.uv7 = mesh.uv7;
                val.uv8 = mesh.uv8;
                val.tangents = mesh.tangents;
                val.colors = mesh.colors;
                val.colors32 = mesh.colors32;
                val.boneWeights = mesh.boneWeights;
                val.bindposes = mesh.bindposes;
            }
        }
        public override Type GetValueType() => typeof(Mesh);
        public override void InitAsset(string assetName)
        {
            // create a sqare mesh
            val = new Mesh();
            val.vertices = new Vector3[4]
            {
                new Vector3(1, 1, 0), new Vector3(3, 1, 0),
                new Vector3(1, 3, 0), new Vector3(3, 3, 0)
            };
            val.uv = new Vector2[4]
            {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)
            };
            val.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
            val.RecalculateNormals();

            assetPath = Path.Join(PathUtils.GRAPH_OUT_PATH, assetName);
            assetPath = assetPath.Replace("\\", "/");
            PathUtils.CreateDirectory(Path.GetDirectoryName(assetPath));
#if UNITY_EDITOR
            // TODO: use addressable
            if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(val, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
#endif
        }
    }

    [System.Serializable]
    public class MaterialParameter : ExposedParameter
    {
        // NOTE: although it's a little wired but the name "val" is not allowed to modify
        [SerializeField] Material val;
        public override string name { get => base.name;
            set
            {
                if (base.name == value)
                    return;
                Rename(value);
                base.name = value;
            }
        }

        public override object value
        {
            get => val;
            set
            {
                Material mat = value as Material;
                if (val == null)
                    val = mat;
                else if (mat != null)
                    val.CopyMatchingPropertiesFromMaterial(mat);
            }
        }

        public override Type GetValueType() => typeof(Material);
        public override string assetExtension => "mat";

        public override void InitAsset(string assetName)
        {
            val = new Material(Shader.Find("Unlit/Color"));
            assetPath = Path.Join(PathUtils.GRAPH_OUT_PATH, assetName);
            assetPath = assetPath.Replace("\\", "/");
            PathUtils.CreateDirectory(Path.GetDirectoryName(assetPath));
#if UNITY_EDITOR
            // TODO: use addressable
            if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(val, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
#endif
        }
    }

    // NOTE: add here if you want to extend ExposedParameter
    [System.Serializable]
    public class AudioClipParameter : ExposedParameter
    {
        [SerializeField] AudioClip val;
        public override string name
        {
            get => base.name;
            set
            {
                if (base.name == value)
                    return;
                Rename(value);
                base.name = value;
            }
        }

        public override object value 
        { 
            get => val;
            set
            {
                val = (AudioClip)value;
                if (!string.IsNullOrEmpty(assetPath) && val != null)
                    val.name = Path.GetFileNameWithoutExtension(assetPath);
                //AudioClip clip = (AudioClip)value;
                //if (clip == null)
                //    return;
                //GraphExportUtils.ExportAudioClip(clip, assetPath);
                //Resources.UnloadAsset(val);
                //string resourcePath = assetPath.Replace("Assets/Resources/", "");
                //resourcePath = Path.GetDirectoryName(resourcePath) + "/" + Path.GetFileNameWithoutExtension(resourcePath);
                //val = Resources.Load<AudioClip>(resourcePath);
            }
        }
        public override Type GetValueType() => typeof(AudioClip);
        public override string assetExtension => "wav";

        public override void InitAsset(string assetName)
        {
            float[] data = new float[3 * 16000];
            val = AudioClip.Create("default", data.Length, 1, 16000, false);
            assetPath = Path.Join(PathUtils.GRAPH_OUT_PATH, assetName);
            assetPath = assetPath.Replace("\\", "/");
            PathUtils.CreateDirectory(Path.GetDirectoryName(assetPath));
            GraphExportUtils.ExportAudioClip(val, assetPath);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(assetPath);
#endif
            string resourcePath = assetPath.Replace("Assets/Resources/", "");
            resourcePath = Path.GetDirectoryName(resourcePath) + "/" + Path.GetFileNameWithoutExtension(resourcePath);
            val = Resources.Load<AudioClip>(resourcePath);
        }

        public override void DeleteAsset()
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                File.Delete(assetPath);
                File.Delete(assetPath + ".meta");
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }
    }
}