using System;
using ayy.pal;
using ayy.pal.core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ayy.debugging
{
    public class DebugMenu : MonoBehaviour
    {
        [Header("Palette")]
        [SerializeField] private TMP_Dropdown _dropdownPalette;
        
        [Header("Map")]
        [SerializeField] private TMP_Dropdown _dropdownMap;
        [SerializeField] private GameObject _mapSpriteFramePrefab;
        
        [Header("Sprite")]
        [SerializeField] private TMP_Dropdown _dropdownSprite;
        [SerializeField] private GameObject _spriteFramesHolder;
        [SerializeField] private GameObject _spriteSheetHolder;
        [SerializeField] private GameObject _spritePresenterPrefab = null;
        private SpritePresenter _spritePresenter = null;
        
        [Header("Camera")]
        [SerializeField] private GameObject _cameraGO;
        [SerializeField,Range(0,20)] private float _cameraMoveSpeed = 5.0f;
        [SerializeField,Range(1,50)] private float _cameraOrthoSize = 5.0f;
        [SerializeField,Range(1,20)] private float _cameraOrthoChangeSpeed = 10.0f;
        
        [Header("GamePlay")]
        [SerializeField] private Button _btnLoadDefaultGame;
        
        [Header("Pos")]
        [SerializeField] private TMP_InputField _inputFieldPos;
        [SerializeField] private Button _btnSetPos;

        [Header("Tile")]
        [SerializeField] private TMP_InputField _inputTileZ;
        [SerializeField] private Button _btnTileZ;

        [Header("Debug")] 
        [SerializeField] private Button _btnToggleMapTileInfo;
        [SerializeField] private Button _btnToggleCtrlDebug;
        private bool _enableMapTileDebug = false;
        private bool _enableInputDebug = false;
        
        private Texture2D[] _spriteFrames;
        
        private MapService _mapService = null;
        private PaletteService _paletteService = null;
        private SpriteService _spriteService = null;
        private ViewportService _viewportService = null;
        private MapEntityManager _mapEntityManager = null;
        private GameStateDataService _dataService = null;
        
        void Start()
        {
            _mapService = PalGame.GetInstance().GetService<MapService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            _spriteService = PalGame.GetInstance().GetService<SpriteService>();
            _viewportService = PalGame.GetInstance().GetService<ViewportService>();
            _mapEntityManager = PalGame.GetInstance().GetService<MapEntityManager>();
            _dataService = PalGame.GetInstance().GetService<GameStateDataService>();
            
            InitDebugPalette();
            InitDebugMap();
            InitDebugPlayerSprite();
            _btnLoadDefaultGame.onClick.AddListener(LoadDefaultGame);
            _btnSetPos.onClick.AddListener(SetTestPos);
            _btnTileZ.onClick.AddListener(SetTileZ);
            
            _btnToggleMapTileInfo.onClick.AddListener(OnClickToggleMapTileInfo);
            _btnToggleCtrlDebug.onClick.AddListener(OnClickToggleCtrlDebug);
            
            ApplyDebugFlag();
        }

        private void Update()
        {
            if (_enableInputDebug)
            {
                UpdateForSwitchMap();
                UpdateForSwitchSprite();
                UpdateForSwitchSpriteFrame();
            }
            else
            {
                UpdateForMoveCamera();                
            }
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

        private void UpdateForSwitchSprite()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int next = _dropdownSprite.value + 1;
                if (next > _dropdownSprite.options.Count - 1)
                {
                    next = 0;
                }
                _dropdownSprite.value = next;
                _dropdownSprite.onValueChanged.Invoke(next);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int next = _dropdownSprite.value - 1;
                if (next < 0)
                {
                    next = _dropdownSprite.options.Count - 1;
                }
                _dropdownSprite.value = next;
                _dropdownSprite.onValueChanged.Invoke(next);
            }
        }

        private void UpdateForSwitchSpriteFrame()
        {
            if (Input.GetKeyDown(KeyCode.P) && _spritePresenter != null)
            {
                _spritePresenter.SwitchNextFrame();
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                int viewportPixelX = 1152;
                int viewportPixelY = 832;
                _viewportService.SetPixelCoord(viewportPixelX, viewportPixelY);
                if(_spritePresenter != null)
                {
                    int spriteOffsetPixelX = 149;
                    int spriteOffsetPixelY = 66;
                    int pixelX = viewportPixelX + spriteOffsetPixelX;
                    int pixelY = viewportPixelY + spriteOffsetPixelY;
                    _spritePresenter.SetPixelPos(pixelX, pixelY);
                }
            }
        }

        private void InitDebugPalette()
        {
            _dropdownPalette.onValueChanged.AddListener(OnClickPalette);
            _dropdownPalette.options.Clear();
            int cnt = _paletteService.GetPaletteCount();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownPalette.options.Add(new TMP_Dropdown.OptionData($"palette_[{i}]"));
            }
        }

        private void InitDebugMap()
        {
            _dropdownMap.onValueChanged.AddListener(OnClickSwitchMap);
            _dropdownMap.options.Clear();
            int mapCnt = _mapService.GetMapWrapper().GetMapCount();
            for (int i = 0; i < mapCnt; i++)
            {
                _dropdownMap.options.Add(new TMP_Dropdown.OptionData($"map_{i}"));
            }
        }

        private void InitDebugPlayerSprite()
        {
            _dropdownSprite.onValueChanged.AddListener(OnClickSwitchSprite);
            int cnt = _spriteService.GetSpriteCount();
            _dropdownSprite.ClearOptions();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownSprite.options.Add(new TMP_Dropdown.OptionData($"sprite[{i}]"));
            }
        }

        private void OnClickPalette(int index)
        {
            _paletteService.LoadPalette(index,false);
        }

        private void OnClickToggleMapTileInfo()
        {
            _enableMapTileDebug = !_enableMapTileDebug;
            ApplyDebugFlag();
        }

        private void OnClickToggleCtrlDebug()
        {
            _enableInputDebug = !_enableInputDebug;
        }

        private void ApplyDebugFlag()
        {
            _viewportService.ToggleVisible(_enableMapTileDebug);
            _mapEntityManager.ToggleDisplayTileInfo(_enableMapTileDebug);
        }

        private void OnClickSwitchMap(int mapIndex)
        {
            _mapEntityManager.SwitchMapById(mapIndex);
        }
        
        
        private void LoadAllSprites()
        {
            int cnt = _spriteService.GetSpriteCount();
            _dropdownSprite.ClearOptions();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownSprite.options.Add(new TMP_Dropdown.OptionData($"sprite[{i}]"));
            }
        }
        
        private void OnClickSwitchSprite(int spriteIndex)
        {
            foreach (Transform child in _spriteFramesHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            // @miao @test
            //LoadSprite(spriteIndex);
            LoadSprite2(spriteIndex);
            RefreshSpritePresenter(spriteIndex);
        }

        unsafe private void LoadSprite(int spriteIndex)
        {
            // 获取原始 sprite 数据 ,并解压缩
            byte[] sprite = _spriteService.GetMgoMKF().ReadChunk(spriteIndex);
            int decompressedSize = _spriteService.GetMgoMKF().GetDecompressedSize(spriteIndex);
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
            SpriteTextureHelper.CreateSpriteFramesGameObjects(decompressedSprite, paletteColors,_mapSpriteFramePrefab,_spriteFramesHolder.transform);
        }
        
        private void LoadSprite2(int spriteIndex)
        {
            PALSprite sprite = _spriteService.GetSprite(spriteIndex);
            if (sprite.GetTexture() == null)
            {
                return;
            }

            var tex = sprite.GetTexture();
            var mat = _spriteSheetHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_MainTex"),sprite.GetTexture());
            
            float sy = _spriteSheetHolder.transform.localScale.y;
            float sx = sy * tex.width / tex.height;
            _spriteSheetHolder.transform.localScale = new Vector3(sx,sy,1);
        }

        private void RefreshSpritePresenter(int spriteIndex)
        {
            if (_spritePresenter == null)
            {
                _spritePresenter = GameObject.Instantiate(_spritePresenterPrefab).GetComponent<SpritePresenter>();
            }
            _spritePresenter.SwitchSpriteFrame(spriteIndex,0);
        }
        
        private void LoadDefaultGame()
        {
            Debug.Log("Load Default Game");
            PalGame.GetInstance().GetService<LoadGameService>().LoadDefaultGame();
        }

        private void SetTestPos()
        {
            try
            {
                string str = _inputFieldPos.text;
                string[] strs = str.Split(",");
                int viewportX = int.Parse(strs[0]);
                int viewportY = int.Parse(strs[1]);
                int partyOffsetX = int.Parse(strs[2]);
                int partyOffsetY = int.Parse(strs[3]);
                
                _dataService.SetViewportXY(viewportX, viewportY);
                if (_spritePresenter != null)
                {
                    int px = viewportX + partyOffsetX;
                    int py = viewportY + partyOffsetY;
                    _spritePresenter.SetPixelPos(px, py);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void SetTileZ()
        {
            string str = _inputTileZ.text;
            string[] strs = str.Split(",");
            int x = int.Parse(strs[0]);
            int y = int.Parse(strs[1]);
            int h = int.Parse(strs[2]);
            
            MapWrapper mapWrapper = _mapService.GetCurrentMap();
            // @miao @Test
            int topHeight = mapWrapper.GetPalMap().GetMapTileLogicHeight(x,y,h,ETileLayer.Top);
            int bottomHeight = mapWrapper.GetPalMap().GetMapTileLogicHeight(x,y,h,ETileLayer.Bottom);
            Debug.Log($"x:{x},y:{y},h:{h},logicHeight,top:{topHeight},bottom:{bottomHeight}");
            
            mapWrapper.SetTileVertexColor(ETileLayer.Top,x,y,h,Color.blue);
            mapWrapper.SetTileVertexColor(ETileLayer.Bottom,x,y,h,Color.blue);
        }
    }
}