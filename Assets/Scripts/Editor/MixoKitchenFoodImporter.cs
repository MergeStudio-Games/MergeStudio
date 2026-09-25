using UnityEditor;

namespace MergeStudio.Editor
{
    public sealed class MixoKitchenFoodImporter : AssetPostprocessor
    {
        private const string FoodPath = "Assets/Resources/MixoKitchen/Food/";
        private const string PremiumFoodPath = "Assets/Resources/MixoKitchen/PremiumFood/";
        private const string UiPath = "Assets/Resources/MixoKitchen/UI/";

        private void OnPreprocessTexture()
        {
            bool food = assetPath.StartsWith(FoodPath, System.StringComparison.Ordinal) ||
                        assetPath.StartsWith(PremiumFoodPath, System.StringComparison.Ordinal);
            bool ui = assetPath.StartsWith(UiPath, System.StringComparison.Ordinal);
            if (!food && !ui) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = food;
            importer.mipmapEnabled = false;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = ui ? 2048 : 256;
        }
    }
}
