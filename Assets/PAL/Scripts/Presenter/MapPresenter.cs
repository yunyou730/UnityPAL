using System;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class MapPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _texturePreviewer = null;
        [SerializeField] private GameObject _bottomLayer = null;
        [SerializeField] private GameObject _topLayer = null;

        private int _mapIndex = 0;
        private MapService _mapService = null;
        //private PaletteService _paletteService = null;
        
        void Awake()
        {
            _mapService = PalGame.GetInstance().GetService<MapService>();
            //_paletteService = PalGame.GetInstance().GetService<PaletteService>();
        }

        void Start()
        {
            
        }

        void OnDestroy()
        {
            
        }

        void Update()
        {
        
        }

        public void Load(int mapIndex)
        {
            _mapIndex = mapIndex;
            _mapService.LoadMap(mapIndex);
            
            Texture2D mapTexture = _mapService.GetCurrentMap().GetTileMapTexture();
            // 展示 map里 所有tile 拼成的大图
            if (mapTexture != null && _texturePreviewer != null)
            {
                var mat = _texturePreviewer.GetComponent<MeshRenderer>().material;
                mat.SetFloat(Shader.PropertyToID("_UsePaletteLUT"), 0.0f);
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), mapTexture);
                //mat.SetTexture(Shader.PropertyToID("_PaletteTex"), _paletteService.GetPaletteTexture());
            }
            // 展示 map 图像
            if (mapTexture != null && _bottomLayer != null)
            {
                var meshFilter = _bottomLayer.GetComponent<MeshFilter>();
                meshFilter.mesh = _mapService.GetCurrentMap().GetTileMapMeshBottom();
                var mat = meshFilter.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), mapTexture);
            }
            if (mapTexture != null && _topLayer != null)
            {
                var meshFilter = _topLayer.GetComponent<MeshFilter>();
                meshFilter.mesh = _mapService.GetCurrentMap().GetTileMapMeshTop();
                var mat = meshFilter.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), mapTexture);
            }
        }

        public void Unload()
        {
            _mapIndex = 0;
            _mapService.UnloadCurrentMap();
        }
    }
    
}

