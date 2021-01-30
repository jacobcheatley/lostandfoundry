using UnityEditor;

public class SpritePixelPerUnitDefault : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.spritePixelsPerUnit = 512;
    }
}
