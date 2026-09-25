using UnityEditor;

namespace MergeStudio.Editor
{
    public sealed class MixoKitchenFoodImporter : AssetPostprocessor
    {
        private const string FoodPath = "Assets/Resources/MixoKitchen/Food/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(FoodPath, System.StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 256;
        }
    }
}
