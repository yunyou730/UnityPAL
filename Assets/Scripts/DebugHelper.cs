
using UnityEngine;
using ayy.pal.core;
using Renderer = ayy.pal.core.Renderer;

public class DebugHelper
{
    public static void CreateSpriteFramesGameObjects(byte[] sprite,PaletteColor[] paletteColors,GameObject prefab,Transform root)
    {
        int spriteFrameCount = Renderer.GetSpriteFrameCount(sprite);
        Texture2D[] textures = new Texture2D[spriteFrameCount];
        float baseY = 0.0f;
        for (int frameIndex = 0; frameIndex < spriteFrameCount; frameIndex++)
        {
            Texture2D tex = Renderer.CreateTexture(sprite, frameIndex,paletteColors);
            if (tex != null)
            {
                var go = GameObject.Instantiate(prefab);
                go.name = "sprite_frame[" + frameIndex + "]";
                go.transform.SetParent(root);
                float sizeX = go.transform.localScale.x;
                float sizeY = tex.height / (float)tex.width * sizeX;
                go.transform.localPosition = new Vector3(0, baseY + frameIndex * sizeY, 0);
                go.transform.localScale = new Vector3(sizeX, sizeY, 1.0f);
                var mat = go.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_Texture2D"), tex);
            }
            else
            {
                Debug.LogWarning("null sprite frame");
            }
        }
    }
}
