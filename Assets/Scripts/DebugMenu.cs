using System.IO;
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
        
        [Header("sprite")]
        [SerializeField] private Button _btnLoadSprite;
        
        [Header("map-spritesheet")]
        [SerializeField] private GameObject _mapSpriteSheetHolder;
        [SerializeField] private GameObject _palMapBottomHolder;
        [SerializeField] private GameObject _palMapTopHolder;
        [SerializeField] private GameObject _cameraGO;
        [SerializeField,Range(0,20)] private float _cameraMoveSpeed = 5.0f;
        [SerializeField,Range(1,50)] private float _cameraOrthoSize = 5.0f;
        [SerializeField,Range(1,20)] private float _cameraOrthoChangeSpeed = 10.0f;
        
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
            InitDebugPlayerSprite();
        }

        private void Update()
        {
            UpdateForMoveCamera();
            UpdateForSwitchMap();
        }

        private void UpdateForMoveCamera()
        {
            if (_cameraGO == null)
            {
                return;
            }
            
            //_cameraOrthoSize
            if (Input.mouseScrollDelta.y != 0)
            {
                _cameraOrthoSize -= (Input.mouseScrollDelta.y * Time.deltaTime * _cameraOrthoChangeSpeed);
            }
            float othoSize = Mathf.Clamp(_cameraOrthoSize, 1.0f, 50.0f);
            _cameraGO.GetComponent<Camera>().orthographicSize = othoSize;
            

            Vector2 dir = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                dir += Vector2.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir += Vector2.down;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dir += Vector2.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir += Vector2.right;
            }
            if (dir.magnitude > 0.0f)
            {
                dir = dir.normalized * Time.deltaTime * _cameraMoveSpeed;
                Vector3 pos = _cameraGO.transform.localPosition;
                pos.x += dir.x;
                pos.y += dir.y;
                _cameraGO.transform.localPosition = pos;
            }
        }

        private void UpdateForSwitchMap()
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

        private void InitDebugPlayerSprite()
        {
            _btnLoadSprite.onClick.AddListener(LoadPlayerSprite);
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
            LoadMapWithSingleDrawCall(mapIndex);
            //LoadMapWithSeveralGameObjects(mapIndex);
        }

        private void OnClickLoadSprite()
        {
            
        }

        private void LoadMapWithSingleDrawCall(int mapIndex)
        {
            _mapService.LoadMap(mapIndex);
            Texture2D spriteSheetTex = _mapService.GetCurrentMap().GetTileMapTexture();
            if (spriteSheetTex != null && _mapSpriteSheetHolder != null)
            {
                var mat = _mapSpriteSheetHolder.GetComponent<MeshRenderer>().material;
                mat.SetFloat(Shader.PropertyToID("_UsePaletteLUT"), 0.0f);
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), spriteSheetTex);
                mat.SetTexture(Shader.PropertyToID("_PaletteTex"), _paletteService.GetPaletteTexture());
            }
            if (spriteSheetTex != null && _palMapBottomHolder != null)
            {
                var meshFilter = _palMapBottomHolder.GetComponent<MeshFilter>();
                meshFilter.mesh = _mapService.GetCurrentMap().GetTileMapMeshBottom();
                var mat = meshFilter.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), spriteSheetTex);
            }
            if (spriteSheetTex != null && _palMapTopHolder != null)
            {
                var meshFilter = _palMapTopHolder.GetComponent<MeshFilter>();
                meshFilter.mesh = _mapService.GetCurrentMap().GetTileMapMeshTop();
                var mat = meshFilter.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_SpriteSheetTex"), spriteSheetTex);
            }
        }

        private void LoadMapWithSeveralGameObjects(int mapIndex)
        {
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
        
        unsafe private void LoadPlayerSprite()
        {
            // @miao @test
            MKFLoader mgoMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "MGO.MKF"));
            mgoMKF.Load();

            int playerID = 0;       // 0:李逍遥
            int spriteIndex = 2;      // sprite index:2
            
            // 获取原始 sprite 数据 ,并解压缩
            byte[] sprite = mgoMKF.ReadChunk(spriteIndex);
            int decompressedSize = mgoMKF.GetDecompressedSize(spriteIndex);
            byte[] decompressedSprite = new byte[decompressedSize];
            fixed (byte* pChunkData = sprite)
            {
                fixed (byte* pDestData = decompressedSprite)
                {
                    Yj1Decompressor.YJ1_Decompress(pChunkData, pDestData,decompressedSize);
                    
                }
            }
            
            // 拿到 sprite 数据,去创建 texture,并展示出来
            PaletteColor[] paletteColors = _paletteService.GetPaletteColors();
            DebugHelper.CreateSpriteFramesGameObjects(decompressedSprite, paletteColors,_mapSpriteFramePrefab,_mapSpriteFramesHolder.transform);
        }
    }
}

