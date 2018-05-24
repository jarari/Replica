using UnityEditor;
using UnityEngine;

internal sealed class SpriteImportSettings : AssetPostprocessor {
    void OnPreprocessTexture() {
        TextureImporter importer = assetImporter as TextureImporter;
        importer.spritePixelsPerUnit = 1;
        importer.compressionQuality = 100;
        importer.maxTextureSize = 8192;
        importer.filterMode = FilterMode.Point;
    }
}
