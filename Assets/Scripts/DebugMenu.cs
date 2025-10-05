using System;
using System.Security;
using ayy.pal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ayy.debugging
{
    public class DebugMenu : MonoBehaviour
    {
        [Header("palette")]
        [SerializeField] private Button _btnLoadPalette;
        [SerializeField] private TMP_Dropdown _dropdownPalette;
        [SerializeField] private GameObject _paletteTextureHolder;
        
        [Header("map")]
        [SerializeField] private Button _btnLoadMap;
        [SerializeField] private TMP_Dropdown _dropdownMap;
        [SerializeField] private GameObject _mapSpriteFramesHolder;
        [SerializeField] private GameObject _mapHolderBottom;
        [SerializeField] private GameObject _mapHolderTop;
        [SerializeField] private GameObject _mapSpriteFramePrefab;
        
        private ayy.pal.Palette _palette;
        private Texture2D _paletteTexture;
        private int _paletteIndex = -1;
        
        private ayy.pal.Map _map;
        private Texture2D[] _spriteFrames;

        // map tile 是 32x15 的 rect, 中间菱形部分有图像
        private float _mapTileWidth = 1.0f;
        private float _mapTileHeight = 15 / 32.0f;
        
        void Start()
        {
            InitDebugPalette();
            InitDebugMap();
        }

        private void InitDebugPalette()
        {
            _btnLoadPalette.onClick.AddListener(OnClickLoadPalette);
            _dropdownPalette.onValueChanged.AddListener(OnClickPalette);
            _dropdownPalette.options.Clear();
            
            _palette = new ayy.pal.Palette();
            _palette.Load();
            _paletteTexture = _palette.CreateDebugTexture();
            var mat = _paletteTextureHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_Texture2D"), _paletteTexture);
        }

        private void InitDebugMap()
        {
            _btnLoadMap.onClick.AddListener(OnClickLoadAllMaps);
            _dropdownMap.onValueChanged.AddListener(OnClickSwitchMap);
            _dropdownMap.options.Clear();
            
            _map = new ayy.pal.Map();
            _map.Load();
        }

        private void OnClickLoadPalette()
        {
            Debug.Log("Load All Palettes");
            _dropdownPalette.options.Clear();
            int paletteCount = _palette.GetPaletteCount();
            for (int i = 0;i < paletteCount;i++)
            {
                _dropdownPalette.options.Add(new TMP_Dropdown.OptionData($"palette_[{i}]"));
            }

            _dropdownPalette.value = 0;
            _dropdownPalette.onValueChanged.Invoke(0);
        }

        private void OnClickPalette(int index)
        {
            Debug.Log($"Load Palette {index}");
            ayy.pal.PaletteColor[] colors = _palette.GetPalette(index,false);
            for (int i = 0;i < ayy.pal.Palette.PALETTE_COLOR_COUNT;i++)
            {
                int x = i % 16;
                int y = i / 16;
                _paletteTexture.SetPixel(x,y,colors[i].ConvertToColor());
            }
            _paletteTexture.Apply();
            _paletteIndex = index;
        }

        private void OnClickLoadAllMaps()
        {
            Debug.Log("Load All Maps");
            int mapCnt = _map.GetMapCount();
            _dropdownMap.options.Clear();
            for (int i = 0; i < mapCnt; i++)
            {
                _dropdownMap.options.Add(new TMP_Dropdown.OptionData($"map_{i}"));
            }
            _dropdownMap.value = 0;
            _dropdownMap.onValueChanged.Invoke(0);
        }

        private void OnClickSwitchMap(int mapIndex)
        {
            // Clear previous map's sprite frames
            for (int i = _mapSpriteFramesHolder.transform.childCount - 1; i >= 0; i--)
            {
                var child = _mapSpriteFramesHolder.transform.GetChild(i);
                Destroy(child.gameObject);
            }
            
            // Load Map
            _map.LoadMapWithIndex(mapIndex);
            PALMap palMap = _map.GetPALMap();
            if (palMap == null)
            {
                Debug.LogWarning($"there's no map with index {mapIndex}");
                return;
            }
            
            // Load palette
            PaletteColor[] paletteColors = _palette.GetPalette(_paletteIndex,false);
            
            // Draw Sprite Frames
            ShowMapSpriteFrames(palMap,paletteColors);
            // Draw map tiles
            ShowMapTiles(palMap,false,_mapHolderBottom);
            ShowMapTiles(palMap,true,_mapHolderTop);
        }

        private void ShowMapSpriteFrames(PALMap palMap,PaletteColor[] paletteColors)
        {
            if (_spriteFrames != null)
            {
                foreach (var spriteFrame in _spriteFrames)
                {
                    Destroy(spriteFrame);
                }
            }
            
            
            var renderer = new ayy.pal.Renderer();
            byte[] sprite = palMap.TileSprite;
            int spriteFrameCount = renderer.GetSpriteFrameCount(sprite);
            _spriteFrames = new Texture2D[spriteFrameCount];
            float baseY = 0.0f;
            for (int frameIndex = 0; frameIndex < spriteFrameCount; frameIndex++)
            {
                Texture2D tex = renderer.CreateTexture(sprite, frameIndex,paletteColors);
                var go = GameObject.Instantiate(_mapSpriteFramePrefab);
                go.name = "sprite_frame[" + frameIndex + "]";
                go.transform.SetParent(_mapSpriteFramesHolder.transform);
                float sizeX = go.transform.localScale.x;
                float sizeY = tex.height / (float)tex.width * sizeX;
                go.transform.localPosition = new Vector3(0, baseY + frameIndex * sizeY, 0);
                go.transform.localScale = new Vector3(sizeX, sizeY, 1.0f);
                var mat = go.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_Texture2D"), tex);
                _spriteFrames[frameIndex] = tex;
            }
        }

        private void ShowMapTiles(PALMap palMap,bool bottomOrTop,GameObject parent)
        {
            for (int y = 0;y < 64;y++)
            {
                for (int h = 0; h < 2; h++)
                {
                    for (int x = 0; x < 128; x++)
                    {
                        //Vector3 tilePos = GetMapTilePos(x,y,h);
                        Vector3 tilePos = GetMapTilePos(y,x,h);
                        
                        
                        // data
                        int frameIndex = palMap.GetSpriteIndexBottomLayer(x,y,h);
                        if (bottomOrTop)
                        {
                            frameIndex = palMap.GetSpriteIndexTopLayer(x,y,h);
                        }
                        
                        
                        // Create tile ,set position
                        if (frameIndex >= 0 && frameIndex < _spriteFrames.Length)
                        {
                            var tile = GameObject.Instantiate(_mapSpriteFramePrefab,parent.transform);
                            tile.name = $"tile-({x}, {y}, {h})";
                            tile.transform.localScale = new Vector3(1, _mapTileHeight / _mapTileWidth, 1);
                            tile.transform.localPosition = tilePos;
                            var mat = tile.GetComponent<MeshRenderer>().material;
                            var tex = _spriteFrames[frameIndex]; // @miao @temp 临时使用一个 sprite frame index 
                            mat.SetTexture(Shader.PropertyToID("_Texture2D"), tex);                            
                        }

                    }
                }
            }
        }

        private Vector3 GetMapTilePos(int x,int y,int h)
        {
            //x = 128 - x;
            float W = _mapTileWidth;
            float H = _mapTileHeight;
            //float H = 16.0f / 32.0f;
            //float baseY = -y * H - h * H / 2;
            //float yCoord = -(y * H + h * H);
            //float yCoord = (y * H * 2);
            float yCoord = -(y * H);
            float baseX = 0;
            if (h == 1)
            //if (h == 0)
            {
                baseX = baseX + W / 2;
                yCoord = yCoord - H / 2;
                //yCoord = yCoord + H;
            }

            // {
            //     baseX = baseX + W / 2;
            //     baseY = baseY - H / 2;
            // }
            float xCoord = baseX + ( x * W);
            return new Vector3(xCoord,yCoord,0);
            //return new Vector3(yCoord,xCoord,0);
        }
    }
}

