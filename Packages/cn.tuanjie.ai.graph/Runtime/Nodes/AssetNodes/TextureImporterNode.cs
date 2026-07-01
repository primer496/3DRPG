#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using GraphProcessor;
using UnityEditor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/TextureImporterNode"), UseProcessAsync]
    public class TextureImporterNode : SDNode
    {
        [SerializeField]
        private TextureImporter m_Importer;

        [SerializeField, Preview, HideInInspector, HideInPreviewSelector]
        [Input("Texture")] private Texture2D m_Texture;

        public TextureImporter importer
        {
            get => m_Importer;
            set
            {
                if (m_Importer != value)
                {
                    m_Importer = value;
                    m_Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(m_Importer.assetPath);
                    this?.NotifyFieldChanged("m_Texture");
                }
            }
        }

        protected override void Enable()
        {
            base.Enable();
        }

        public override void SetTarget(Object target)
        {
            if (target is TextureImporter textureImporter)
                importer = textureImporter;
        }

        public override bool isRenamable => true;

        public override bool needTrigger => true;

        protected override void Destroy()
        {
            base.Destroy();
        }

        public override void CollectSubAssets()
        {
            base.CollectSubAssets();
        }

        public override IEnumerator ProcessAsync()
        {
            if (importer == null)
                yield break;


            if (!EditorUtility.DisplayDialog("Confirm Texture Replacement",
                $"Replace texture at '{importer.assetPath}' ? The orginal image will be removed.\n\nThis operation cannot be undone.",
                "Replace", "Cancel"))
            {
                yield break;
            }

            string path = importer.assetPath;

            try
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
               
                System.IO.File.WriteAllBytes(path, EncodeTextureBasedOnExtension(m_Texture, path));

                // Apply the old importer settings to the new texture
                TextureImporter newImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                newImporter.SetTextureSettings(settings);
                newImporter.SaveAndReimport();
                importer = newImporter;
            }
            catch (System.Exception e)
            {
                throw new Exception($"Error While Replacing Old Texture: {e.Message}");
            }

            yield return null;
        }

        private byte[] EncodeTextureBasedOnExtension(Texture2D texture, string originalPath)
        {
            string extension = Path.GetExtension(originalPath).ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return texture.EncodeToJPG();
                case ".tga":
                    return texture.EncodeToTGA();
                case ".exr":
                    return texture.EncodeToEXR();
                case ".png":
                default:
                    return texture.EncodeToPNG();
            }
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
#endif