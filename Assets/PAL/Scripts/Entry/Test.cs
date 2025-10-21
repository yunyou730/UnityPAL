using System.IO;
using ayy.pal;
using ayy.pal.core;
using UnityEngine;
using Renderer = ayy.pal.core.Renderer;

public class Test : MonoBehaviour
{
    MKFLoader _mkfLoader = null;
    [SerializeField] private GameObject _spriteFramePrefab = null;
    
    void Start()
    {
        int mapIndex = 12;
        var map = new ayy.pal.core.PALMapWrapper();
        map.Load();
        
        // 调色板数据
        var palette = new Palette();
        palette.Load();
        PaletteColor[] paletteColors = palette.GetPalette(0,false);
        
        // 把地图里的 sprite, 存储为 texture
        var palMap = map.LoadMapWithIndex(mapIndex);
        byte[] sprite = palMap.TileSprite;
        int spriteFrameCount = Renderer.GetSpriteFrameCount(sprite);
        float baseY = 0.0f;
        for (int frameIndex = 0; frameIndex < spriteFrameCount; frameIndex++)
        {
            Texture2D tex = Renderer.CreateTexture(sprite, frameIndex,paletteColors);
            var go = GameObject.Instantiate(_spriteFramePrefab);
            go.name = "sprite_frame[" + frameIndex + "]";
            go.transform.SetParent(transform);
            float sizeX = go.transform.localScale.x;
            float sizeY = tex.height / (float)tex.width * sizeX;
            go.transform.localPosition = new Vector3(0, baseY + frameIndex * sizeY, 0);
            go.transform.localScale = new Vector3(sizeX, sizeY, 1.0f);
            var mat = go.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_Texture2D"), tex);
        }
    }
}
