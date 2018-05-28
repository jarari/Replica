using UnityEditor;
using UnityEngine;

internal sealed class SpriteImportSettings : AssetPostprocessor {
    void OnPreprocessTexture() {
        TextureImporter importer = assetImporter as TextureImporter;
        importer.spritePixelsPerUnit = 1;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.crunchedCompression = true;
        importer.maxTextureSize = 2048;
        importer.filterMode = FilterMode.Point;
    }
}
