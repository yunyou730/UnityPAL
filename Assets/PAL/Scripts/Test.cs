using System.IO;
using ayy.pal;
using UnityEngine;

public class Test : MonoBehaviour
{
    MKFLoader _mkfLoader = null;
    
    //Material _material = null;
    [SerializeField] private GameObject _spriteFramePrefab = null;
    
    void Start()
    {
        int mapIndex = 12;

        // 地图 tile数据, sprite 数据
        var mapMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "MAP.MKF"));
        var gopMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "GOP.MKF"));
        mapMKF.Load();
        gopMKF.Load();
        var map = new ayy.pal.Map();
        map.LoadMap(mapIndex,mapMKF,gopMKF);
        
        // 调色板数据
        var palette = new ayy.pal.Palette();
        PaletteColor[] paletteColors = palette.GetPalette(0,false);
        
        // 把地图里的 sprite, 存储为 texture
        var renderer = new ayy.pal.Renderer();
        byte[] sprite = map.GetPALMap().TileSprite;
        int spriteFrameCount = renderer.GetSpriteFrameCount(sprite);
        float baseY = 0.0f;
        for (int frameIndex = 0; frameIndex < spriteFrameCount; frameIndex++)
        {
            Texture2D tex = renderer.CreateTexture(sprite, frameIndex,paletteColors);
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
    
    void Update()
    {
        
    }
}
