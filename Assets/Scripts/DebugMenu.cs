using ayy.pal;
using ayy.pal.core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Renderer = ayy.pal.core.Renderer;

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
        [SerializeField] private bool _showMapBottomLayer = true;
        [SerializeField] private bool _showMapTopLayer = true;
        
        [Header("map-spritesheet")]
        [SerializeField] private GameObject _mapSpriteSheetHolder;
        
        private Texture2D[] _spriteFrames;

        // map tile 是 32x15 的 rect, 中间菱形部分有图像
        private float _mapTileWidth = 1.0f;
        private float _mapTileHeight = 15 / 32.0f;

        private MapServices _mapService = null;
        private PaletteService _paletteService = null;
        private Map _mapManager = null;
        
        void Start()
        {
            _mapService = PalGame.GetInstance().GetService<MapServices>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            _mapManager = _mapService.GetMapManager();
            
            InitDebugPalette();
            InitDebugMap();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int next = _dropdownMap.value + 1;
                if (next > _dropdownMap.options.Count - 1)
                {
                    next = 0;
                }
                _dropdownMap.value = next;
                _dropdownMap.onValueChanged.Invoke(next);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int next = _dropdownMap.value - 1;
                if (next < 0)
                {
                    next = _dropdownMap.options.Count - 1;
                }
                _dropdownMap.value = next;
                _dropdownMap.onValueChanged.Invoke(next);
            }
        }

        private void InitDebugPalette()
        {
            _btnLoadPalette.onClick.AddListener(OnClickLoadPalette);
            _dropdownPalette.onValueChanged.AddListener(OnClickPalette);
            _dropdownPalette.options.Clear();
            
            var mat = _paletteTextureHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_Texture2D"), _paletteService.GetPaletteTexture());
        }

        private void InitDebugMap()
        {
            _btnLoadMap.onClick.AddListener(OnClickLoadAllMaps);
            _dropdownMap.onValueChanged.AddListener(OnClickSwitchMap);
            _dropdownMap.options.Clear();
        }

        private void OnClickLoadPalette()
        {
            Debug.Log("Load All Palettes");
            _dropdownPalette.options.Clear();
            int cnt = _paletteService.GetPaletteCount();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownPalette.options.Add(new TMP_Dropdown.OptionData($"palette_[{i}]"));
            }
            _dropdownPalette.value = 0;
            _dropdownPalette.onValueChanged.Invoke(0);
        }

        private void OnClickPalette(int index)
        {
            Debug.Log($"Load Palette {index}");
            _paletteService.LoadPalette(index,false);
        }

        private void OnClickLoadAllMaps()
        {
            Debug.Log("Load All Maps");
            int mapCnt = _mapManager.GetMapCount();
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
            // @miao @test
            _mapService.LoadMap(mapIndex);
            Texture2D spriteSheetTex = _mapService.GetCurrentMap().GetTileMapTexture();
            if (spriteSheetTex != null && _mapSpriteSheetHolder != null)
            {
                //Color[] colors = _paletteService.GetPaletteColorsInUnity();
                var mat = _mapSpriteSheetHolder.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), spriteSheetTex);
                mat.SetTexture(Shader.PropertyToID("_PaletteTex"), _paletteService.GetPaletteTexture());
            }

            // Clear previous map's sprite frames
            for (int i = _mapSpriteFramesHolder.transform.childCount - 1; i >= 0; i--)
            {
                var child = _mapSpriteFramesHolder.transform.GetChild(i);
                Destroy(child.gameObject);
            }
            
            // Clear tiles
            for(int i = 0;i < _mapHolderBottom.transform.childCount;i++)
            {
                Destroy(_mapHolderBottom.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < _mapHolderTop.transform.childCount; i++)
            {
                Destroy(_mapHolderTop.transform.GetChild(i).gameObject);
            }


            // Load Map
            PALMap palMap = _mapManager.LoadMapWithIndex(mapIndex);
            if (palMap == null)
            {
                Debug.LogWarning($"there's no map with index {mapIndex}");
                return;
            }
            
            // Load palette
            PaletteColor[] paletteColors = _paletteService.GetPaletteColors();
            
            // Draw Sprite Frames
            ShowMapSpriteFrames(palMap,paletteColors);
            // Draw map tiles
            if (_showMapBottomLayer)
            {
                ShowMapTiles(palMap,false,_mapHolderBottom);                
            }
            if (_showMapTopLayer)
            {
                ShowMapTiles(palMap,true,_mapHolderTop);                
            }
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
            
            byte[] sprite = palMap.TileSprite;
            int spriteFrameCount = Renderer.GetSpriteFrameCount(sprite);
            _spriteFrames = new Texture2D[spriteFrameCount];
            float baseY = 0.0f;
            for (int frameIndex = 0; frameIndex < spriteFrameCount; frameIndex++)
            {
                Texture2D tex = Renderer.CreateTexture(sprite, frameIndex,paletteColors);
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
                            tile.name = $"tile:({x}, {y}, {h}),frame:{frameIndex},layer:{bottomOrTop}";
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
            float W = _mapTileWidth;
            float H = _mapTileHeight;
            float yCoord = -(y * H);
            float baseX = 0;
            if (h == 1)
            {
                baseX = baseX + W / 2;
                yCoord = yCoord - H / 2;
            }
            float xCoord = baseX + ( x * W);
            float zCoord = 0.0f;
            return new Vector3(xCoord,yCoord,zCoord);
        }
    }
}

